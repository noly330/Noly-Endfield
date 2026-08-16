# UI 框架学习笔记 · 参照 lemo 版（2035client）

> 说明：这是对照参考项目 `D:\Project\Unity\work\2035client` 重新梳理的笔记。
> 你的 Noly-Endfield 里的 UI 框架就是抄这个项目改的。
> 本笔记**以 lemo（主程）的代码为准**，其他人的代码只作业务思路参考，不学他们的风格。
> 文末专门列出主程承认的 bug（集中在 UIManager / BaseView）。

---

## 0. 先分清「谁的代码能信」

git 作者一共有这些人：

| 作者 | 角色 | 代码可信度 |
| --- | --- | --- |
| **Lemo** | 主程 | ✅ 精读、照着学 |
| Maple_LeavesY / Noly / 西夏笠谷 / BIUBI | 高中生/大学生 | ⚠️ 只看业务思路，别学实现 |

**lemo 纯血（他一个人写的，最值得精读）**：

- 框架全部：`Core/UI/*`（BaseView / BaseWidget / IView / UIManager / UIRegister / ViewEntry）
- `Core/ResourceLoader.cs`、`Core/Singleton.cs`、`Core/GameLauncher.cs`、`Core/GameLifetimeScope.cs`
- `Core/Model/*`（BaseModel / IModel / ModelManager）
- `Module/CycleList/CycleList.cs`（复用滚动列表，很有参考价值）
- `Module/ScanView/ScanView.cs`（最简 View 范例）
- `Module/Event/EventSystem.cs`（事件中心）
- `Module/CharacterView/CharacterModel.cs`（Model 范例）

**lemo 为主、被学生改过**：MainView / CharacterView / CardListView / CardView(卡牌) / CraftView / CraftWidget / AttackView 等——主体结构是 lemo 的，能学。

**学生为主**：SelectView（Maple_LeavesY）、CraftModel（Noly）、OverView_One、StoryView、UIRegister（多人共改）——参考即可。

---

## 1. 一句话理解这个框架

> **视图是「纯 C# 类」，不是 MonoBehaviour。**
> 它通过持有 `Transform` 引用来操作一个 `Instantiate` 出来的 prefab 实例。

- 依赖注入：VContainer（视图拿到 Model、拿到容器）
- 资源加载：Addressables（经 `ResourcesLoader` 封装，带引用计数）
- 异步：UniTask
- 数据/事件：Model 层（`BaseModel`）+ 事件中心（`EventManager`）

**View 和 Widget 是两个层级**：

| | BaseView（页面） | BaseWidget（控件） |
| --- | --- | --- |
| 粒度 | 全屏/独立界面 | 可复用的 UI 小块（列表项、卡牌、槽位） |
| 挂在哪 | UIManager 的分层节点（Bottom/Middle/Top/Pop） | 任意 parent（通常是某个 View 或 Widget 下） |
| 自己加载 prefab？ | 不，由 UIManager 加载并实例化 | 是，`Initialize` 里自己加载自己的 prefab |
| 依赖注入 | 有 `[Inject] Container` + 构造注入 Model | **无任何 DI**，直接 `new` |
| prefab 来源 | `ViewEntry.PrefabPath`（UIManager 用） | 自己的抽象属性 `ViewEntry` |

---

## 2. 核心类逐个拆

### 2.1 `ViewEntry` —— 视图描述符（一张「名片」）
```csharp
public class ViewEntry {
    [CanBeNull] public Type ViewType { get; }   // 视图的 C# 类型（可为 null）
    public string PrefabPath { get; }           // Addressables 完整路径
    public string Name { get; }                 // 文件名（不含扩展名）
}
```
`Name = Path.GetFileNameWithoutExtension(PrefabPath)`，用于给实例化后的 GameObject 命名。

### 2.2 `UIRegister` —— 注册表（手动列举）
```csharp
public static readonly ViewEntry MainView = new(typeof(MainView), "Assets/Res/Prefab/MainView/MainView.prefab");
public static IReadOnlyList<ViewEntry> All { get; } = new[] { MainView, CharacterView, ... };
```
- **新增视图 = 手动加一行 + 加入 `All` 数组**（没有自动扫描）。
- `All` 被 `GameLifetimeScope` 遍历，用于注册进容器。
- `ViewType` 可以为 null（如 `GatherView = new(null, "...")`），null 的条目不会注册进容器（见 GameLifetimeScope 的 `if (ve.ViewType != null)`），只能当「纯 prefab 路径」用。

### 2.3 `IView` —— 生命周期契约
```csharp
UniTask Initialize(Transform root, string viewName = "");
void Show(object data = null);
void Hide();
void Destroy();
```

### 2.4 `UIManager` —— 单例管理器（核心）
成员：
- `_viewCache`：`Dictionary<string, IView>`，**key = PrefabPath**。一个 prefab 对应一个视图实例（页面级缓存）。
- `_viewStack`：`Stack<IView>`，**只有 `UILayer.Pop` 的视图会入栈**（弹窗栈；当前业务代码几乎没用 Pop）。
- `_canvasCache`：`Dictionary<UILayer, Transform>`，缓存根 Canvas 下的 4 个分层节点。
- `[Inject] IObjectResolver Container`：注入的 VContainer 容器。

### 2.5 `BaseView` —— 页面基类
关键成员：
- `[Inject] protected IObjectResolver Container`（**属性注入**，用于 `SetWidget`）
- `protected virtual int SortOrder => 0`（可覆写，独立排序）
- `protected Transform RootTransform` / `RootGameObject`
- `_componentCache`：`Dictionary<string, Component>`（组件缓存）

### 2.6 `BaseWidget` —— 控件基类
关键成员：
- `protected abstract ViewEntry ViewEntry { get; }`（**每个控件自己声明要加载哪个 prefab**）
- `protected Transform Parent`
- `transform` / `gameObject` / `transformRect`
- **没有任何 `[Inject]` 成员，没有构造依赖** → 所以能直接 `new`。

---

## 3. 依赖注入机制（最容易「吃不透」的部分）

### 3.1 容器注册 `GameLifetimeScope.Configure`

```csharp
// ① 反射扫描：所有 BaseModel 子类 → 注册为单例
foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
    if (type.IsSubclassOf(typeof(BaseModel)) && !type.IsAbstract)
        builder.Register(type, Lifetime.Singleton).AsSelf();

// ② 遍历 UIRegister.All：所有视图 → 注册为瞬时（每次 Resolve 新实例）
foreach (var ve in UIRegister.All)
    if (ve.ViewType != null)
        builder.Register(ve.ViewType, Lifetime.Transient);

// ③ 容器构建完成后：对「不是容器创建的」单例手动补注入
builder.RegisterBuildCallback(container => {
    container.Inject(UIManager.Instance);
    container.Inject(ModelManager.Instance);
});
```

### 3.2 视图怎么拿到 Model —— 构造注入（推荐写法）

```csharp
public class MainView : BaseView {
    private readonly MainModel _model;
    public MainView(MainModel model) { _model = model; }   // ← 构造注入
}
```
`UIManager.OpenView` 里 `Container.Resolve(typeof(MainView))` 时，VContainer 看到构造函数需要 `MainModel`，而 `MainModel` 已被 ① 注册为单例，于是自动 `new MainModel()` 传进去。**这是整个 DI 的核心价值：视图不用自己 `new Model`，容器帮你把依赖装好。**

### 3.3 视图怎么拿到 Container —— 属性注入

`BaseView` 有 `[Inject] IObjectResolver Container`。Resolve 视图时，VContainer 会：
1. 先按构造函数 `new MainView(model)`（此时 `Container` 还是 null）
2. 再扫描 `[Inject]` 成员（含继承自 BaseView 的 `Container`），赋值为容器
3. 返回实例（此时 `Container` 已可用）

**注入时机精确说法**：发生在 `OpenView` 内部的 `Container.Resolve(...)` 那一刻，不是 `OpenView` 一进来就注入。

### 3.4 为什么 `Resolve(entry.ViewType)` 能命中

`builder.Register(ve.ViewType, Lifetime.Transient)` **没写 `.AsSelf()`**。VContainer 内部（`Registry.Build`）在 `InterfaceTypes == null` 时，默认把**实现类型本身当 key** 注册，所以用具体类型去 `Resolve` 能命中。

### 3.5 另一种拿 Model 的写法（AttackView，字段初始化，较脆弱）

```csharp
private AttackModel _attackModel = ModelManager.Instance.Get<AttackModel>();
```
这是**字段初始化**，在视图构造时立刻执行 `ModelManager.Instance.Get<T>()`（内部 `Container.Resolve<T>()`）。
- 它之所以能工作，是因为所有视图都是在 `GameLauncher.Start`（容器早已 build 完、`ModelManager` 已被注入）之后才打开的。
- 但它是**脆弱写法**：如果视图比容器 build 更早被 Resolve，`ModelManager.Instance.Container` 还是 null 就会 NRE。
- **学就用构造注入，别学这个字段初始化写法。**

---

## 4. 资源加载（理解 bug 的关键）

`ResourcesLoader : Singleton<ResourcesLoader>` 封装 Addressables：

| 方法 | 作用 |
| --- | --- |
| `Load<T>(path)` | 带缓存 + 引用计数。已加载则 refCount++；首次则 `Addressables.LoadAssetAsync<T>` |
| `Release(path)` | refCount--，归 0 才真正 `Addressables.Release` |
| `ReleaseAll()` | 全部释放 |
| `LoadNoCache<T>(path)` | 加载后立即释放（不能长期持有返回值） |
| `Loads<T>(paths)` | 批量并行加载 |
| `LoadPrefab(ViewEntry)` | `Load<GameObject>(entry.PrefabPath)` |

**核心规则：缓存/释放的 key 是「完整的 prefab 路径」**，例如 `"Assets/Res/Prefab/MainView/MainView.prefab"`。
记住这句话——下面要讲的 bug 就出在这里。

---

## 5. 生命周期

```
Initialize(root, viewName)
   ├─ 记录 _viewName / RootTransform
   ├─ SortOrder > 0 时：动态加嵌套 Canvas（overrideSorting）
   └─ await OnInit(root)     ← 子类覆写：缓存组件、绑定事件、订阅事件

Show(data)   → SetActive(true) → OnShown(data)   ← 显示逻辑
Hide()       → SetActive(false) → OnHidden()      ← 隐藏逻辑
Destroy()    → OnHidden() + OnDestroy() → 销毁 GameObject → 清缓存
```

子类可覆写的钩子：`OnInit` / `OnShown` / `OnHidden` / `OnDestroy`。

**BaseView 与 BaseWidget 生命周期差异**：

| | BaseView | BaseWidget |
| --- | --- | --- |
| `Initialize` | 只接收 UIManager 传进来的 transform | **自己加载 prefab** 再实例化到 parent |
| 实例化后 | （UIManager 已 SetActive） | `SetActive(false)`，等 Show 再开 |
| `Show` | 同步 `void` | `async void`，`WaitUntil(gameObject != null)` |
| `Destroy` 释放资源 | `Release(_viewName)` ❌ 有 bug | `Release(ViewEntry.PrefabPath)` ✅ 正确 |

---

## 6. 分层机制

### 6.1 UILayer 枚举值不是排序值
```csharp
public enum UILayer { Bottom = 0, Middle = 2000, Top = 5000, Pop = 9000 }
```
这些数字**只是字典 key 的枚举标识，从未被当作渲染排序值**。真正的遮挡顺序由场景 `Launcher.unity` 里 `UI` 根 Canvas 下 4 个子节点的 **Hierarchy 兄弟顺序**决定：

```
UI (Canvas)
├── Bottom   ← 最底
├── Middle
├── Top
└── Pop      ← 最顶
```

`UIManager.InitCanvasLayers()` 用 `transform.Find("Bottom")` 等**按名字**查找并缓存这 4 个节点。

### 6.2 SortOrder —— 视图内独立排序
当 `SortOrder > 0` 时，`Initialize` 会给视图根节点动态加**嵌套 Canvas** 并 `overrideSorting = true`，让该视图无视父级、独立控制渲染顺序。例如卡牌：
```csharp
// CardView（卡牌 widget）
protected override int SortOrder => (int)UILayer.Top + 100;
```

---

## 7. 事件系统（lemo 纯血：EventManager）

```csharp
EventManager.Instance.AddListener(key, callback);   // 订阅
EventManager.Instance.RemoveListener(key, callback);// 取消
EventManager.Instance.Dispatch(key, args...);       // 广播
```

特点：
- 用 **字符串 key**（不是泛型/结构体），事件名集中在 `GlobalEvent` 静态类里定义常量。
- 回调签名统一是 `Action<object[]>`，参数装箱成 `object[]`。
- **延迟移除**：Dispatch 过程中 RemoveListener 会先入队，派发完再真正移除（避免枚举时改集合）。
- **异常隔离**：每个回调单独 try/catch，一个回调抛异常不影响其它回调。

**和你的 Noly-Endfield 对比**：你的项目里用的是另一套 `EventCenter`（结构体消息 `where T : struct`）。这是两套不同的事件方案，别搞混——2035client 这套是字符串 key。

---

## 8. 真实案例拆解（lemo 的范例，照着学）

### 8.1 最简 View：`ScanView`
```csharp
public class ScanView : BaseView {
    public Image layer1; ... // 直接 public 字段

    protected override UniTask OnInit(Transform root) {
        layer1 = GetComponent<Image>("Main/layer1");
        ...
        return UniTask.CompletedTask;
    }
    protected override void OnShown(object data) {
        // 播 LeanTween 动画
        TimerManager.Instance.AddTimer("ScanViewCountDown", 5f, () => {
            UIManager.Instance.CloseView(this);   // 5 秒后自己关掉
        });
    }
    protected override void OnHidden() { CancelEffect(); }  // 清理动画
}
```
要点：`OnInit` 缓存组件；`OnShown` 开动画/计时器；`OnHidden` 清理。**关闭自己用 `UIManager.Instance.CloseView(this)`。**

### 8.2 View + 构造注入 Model：`MainView`
```csharp
public class MainView : BaseView {
    private readonly MainModel _model;
    public MainView(MainModel model) { _model = model; }   // 构造注入

    protected override async UniTask OnInit(Transform root) {
        _firstLineBtn = GetComponent<Button>("Background/.../FirstLineButton");
        _mapBtnInfos = await _model.GetMapBtnInfos();      // 用 Model 拿数据
        _firstLineBtn.onClick.AddListener(ShowStoryInfoView);
        await UniTask.CompletedTask;
    }
    protected override void OnDestroy() {
        _firstLineBtn.onClick.RemoveAllListeners();        // 记得解绑
        base.OnDestroy();
    }
    public void ShowStoryInfoView() =>
        UIManager.Instance.OpenView(UIRegister.StoryView, layer: UILayer.Middle).Forget();
}
```
要点：`GetComponent<Button>("路径")` 按 Hierarchy 路径拿组件；按钮在 `OnDestroy` 里 `RemoveAllListeners`；打开别的 View 用 `UIManager.Instance.OpenView(...).Forget()`。

### 8.3 View + 复用滚动列表：`CharacterView`（含 CycleList）
- `OnInit` 里用 `GetComponent` 拿 ScrollRect / Item 模板 / Content。
- 计算可视数量 `count = 可视高度 / (item高度+间距)`，`Instantiate` 出 `count` 个 item（复用池思想）。
- 每个 item 里再嵌一个 `OneLineDataWidget`，`await oneLineData.Initialize(item.transform)` 逐个初始化。
- 用 `CycleList` 做滚动复用（见 8.10）。

### 8.4 View + SetWidget 动态生成 Widget：`CardListView`（手牌栏）
```csharp
protected override async UniTask OnInit(Transform root) {
    EventManager.Instance.AddListener(GlobalEvent.OnCardDrawn, OnCardDrawn);  // 订阅
    ...
}
private async void OnCardDrawn(object[] args) {
    var cardConfig = args[0] as CardConfig;
    var card = await SetWidget<CardView>(UIRegister.BaseCard, _cardDeckTrans);  // ← SetWidget
    card.gameObject.name = $"BaseCard_{_cardViews.Count}";
    _cardViews.Add(card);
    card.Show(cardConfig);
}
```
要点：**View 动态生成 Widget 用 `SetWidget<T>(entry, parent)`**；`OnDestroy` 里要 `RemoveListener` 解绑所有订阅。

### 8.5 View + 直接 new Widget + 嵌套 + 拖拽：`CraftView`
```csharp
protected override async UniTask OnInit(Transform root) {
    var container = GetComponent<RectTransform>("Main/CraftWidgetContainer");
    _craftWidget = new CraftWidget();                 // ← 直接 new（不是 SetWidget）
    await _craftWidget.Initialize(container);
    await _craftWidget.InitSlots();
    _craftWidget.Show();
    EventManager.Instance.AddListener(GlobalEvent.OnCardDragStart, OnCardDragStart);
    ...
}
protected override void OnDestroy() {
    ...RemoveListener...
    _craftWidget?.Destroy();                          // 记得销毁子 widget
}
```
要点：**View 也可以直接 `new Widget()` + `Initialize`**；子 widget 要自己负责 `Destroy`。

### 8.6 最简 Widget：`OneLineDataWidget`
```csharp
public class OneLineDataWidget : BaseWidget {
    public Image Icon; public Text Name; public Text Property;
    protected override ViewEntry ViewEntry => UIRegister.OneLineDataWidget;  // 自己声明 prefab
    protected override void OnInit() {
        Icon = GetComponent<Image>("Main/Icon"); ...
    }
    protected override void OnShown(object data) {
        if (data is Args args) { ... }
    }
    public class Args { ... }   // 用内部类当「参数结构体」
}
```
要点：Widget 必须覆写 `ViewEntry`；用 `data is Args args` 接收参数；**用内部类 `Args` 传参**是 lemo 的统一习惯。

### 8.7 Widget + 业务逻辑：`CardView`（卡牌 widget）
```csharp
public class CardView : BaseWidget {
    protected override int SortOrder => (int)UILayer.Top + 100;   // 独立排序
    protected override ViewEntry ViewEntry => UIRegister.BaseCard;
    private void OnInit() { ... cardItem = gameObject.GetComponent<CardItem>(); ... }
    private void OnShown(object data) { CardConfig = data as CardConfig; ... }
}
```
要点：Widget 也能覆写 `SortOrder`；Widget 里可以 `GetComponent` 拿自己挂的普通脚本（`CardItem`）。

### 8.8 Widget 嵌套 Widget：`CraftWidget` / `CraftSlotWidget`
```csharp
// CraftWidget 里
public async UniTask InitSlots() {
    for (int i = 0; i < 3; i++) {
        var slot = new CraftSlotWidget();          // Widget 里只能 new（没有 Container）
        await slot.Initialize(_cardPoints[i]);
        slot.Show(new CraftSlotWidget.Args(i));
        Slots[i] = slot;
    }
}
```
要点：**Widget 创建子 Widget 只能用 `new`**（因为 BaseWidget 没有 Container，用不了 SetWidget）。

### 8.9 Model + 预加载：`CharacterModel`（lemo 纯血）
```csharp
public class CharacterModel : BaseModel {
    bool isPreLoadDown = false;
    public CharacterModel() { InitPropertySprites().Forget(); }   // 构造即预加载
    private async UniTask InitPropertySprites() {
        var sprites = await ResourcesLoader.Instance.Loads<Sprite>(paths);
        ...
        isPreLoadDown = true;
    }
    public async UniTask<List<PropertyData>> GetProperties() {
        await UniTask.WaitUntil(() => isPreLoadDown);   // 等预加载完
        return properties;
    }
}
```
要点：Model 负责**数据预加载**，用 `WaitUntil` 挡住调用方直到数据就绪。

### 8.10 复用滚动列表：`CycleList`（lemo 纯血，值得单独研究）
- 核心思想：只实例化「可视数量 + 缓冲」个 item，滚动时循环复用，不随数据量增长。
- `CycleListSetting`（配置结构体）+ `ICycleListItem` / `ICycleListData` 两个接口。
- 支持等距（`itemSpace`）和不等距（`onLayoutFunc` 动态算高度）两种布局。
- 用法见 CharacterView：new 一个 `CycleList(setting)` → `SetItem()`。

---

## 9. Widget 的两种创建方式（关键差异，务必分清）

| 方式 | 代码 | 能用在哪 | 原理 |
| --- | --- | --- | --- |
| SetWidget | `await SetWidget<T>(entry, parent)` | 只有 **View 内**（有 Container） | `Container.Resolve(entry.ViewType)` 创建 |
| 直接 new | `var w = new T(); await w.Initialize(parent);` | **任何地方**（View/Model/Widget） | 直接 `new`（BaseWidget 无依赖） |

为什么两种都行？因为 **BaseWidget 没有任何 `[Inject]` 成员、没有构造函数依赖**，所以 `Resolve` 它和 `new` 它结果一样（都是 `new T()`）。

**lemo 的实际用法**：
- CardListView（View）→ 用 `SetWidget<CardView>(...)`
- CraftView（View）→ 用 `new CraftWidget()`
- CraftWidget / CardModel（Widget/Model）→ 用 `new`

结论：**View 里两种都可以；Model/Widget 里只能用 `new`。** 团队实际偏向直接 `new` 居多。

---

## 10. 主程承认的 bug（集中在 UIManager / BaseView）

### 🐛 Bug 1（核心）：`BaseView.Destroy()` 用「显示名」释放资源 → 泄漏/假警告

lemo 的 `BaseView.Destroy()` 最后一行：
```csharp
ResourcesLoader.Instance.Release(_viewName);   // _viewName 是 entry.Name = "MainView"
```
而 `_viewName` 是**文件名（不含扩展名）**，`ResourcesLoader` 却按**完整路径**（`"Assets/Res/Prefab/MainView/MainView.prefab"`）做 key。所以：

- `Release("MainView")` 在 `_refCounts` 里找不到 → 打印警告「Trying to release unloaded...」→ **啥也没释放**。

**后果分两种路径**：

| 关闭路径 | 是否真正释放资源 | 现象 |
| --- | --- | --- |
| `CloseView(entry, destroy:true)` | ✅ 释放（因为后面还有一行 `Release(entry.PrefabPath)`） | 但多打一条假警告 |
| `CloseView(view, destroy:true)` | ❌ 泄漏 | 只调 `view.Destroy()`，没正确释放 |
| `DestroyView(entry)` | ❌ 泄漏 | 只调 `view.Destroy()` |
| `CloseAllViews()` | ❌ 泄漏全部 | 只调 `view.Destroy()` |

**对比：`BaseWidget.Destroy()` 是对的**——它用 `Release(ViewEntry.PrefabPath)`（完整路径）。

**你在 Noly-Endfield 里的改法（是对的）**：把 `Release` 从 `BaseView.Destroy()` 里删掉，改到 `UIManager` 里统一按 `PrefabPath` 释放。所以你的版本在「资源释放」上比 lemo 原始版更正确。

### 🐛 Bug 2：`DestroyView` 触发两次 `OnHidden`

```csharp
public void DestroyView(ViewEntry entry) {
    ...
    CloseView(entry);   // destroy=false → Hide() → OnHidden() 第一次
    view.Destroy();     // 内部 OnHidden() 第二次 + OnDestroy()
    ...
}
```
`OnHidden()` 被调两次，若里面有清理逻辑会重复执行（lemo 原始版和你抄的版本都有这个）。

### 🐛 Bug 3：两个 `CloseView` 的 `destroy` 默认值相反

```csharp
public void CloseView(ViewEntry entry, bool destroy = false); // 默认只 Hide
public void CloseView(IView view, bool destroy = true);       // 默认 Destroy
```
语义相反，调用时务必显式传参。

### 🐛 Bug 4：`SetWidget<T>(entry, parent)` 的 `entry.PrefabPath` 是死参数

`SetWidget` 只用 `entry.ViewType` 去 Resolve，**真正加载哪个 prefab 由 widget 自己的 `ViewEntry` 属性决定**，`entry.PrefabPath` 被忽略。所以调用时 `entry` 和 `T` 必须指向同一个 widget 类型，否则会「Resolve 了 A 却加载了 B 的 prefab」。

### 🐛 Bug 5：`BaseWidget.Show` 是 `async void`

`async void` 反模式：调用方无法 await、异常传播不可控（虽然内部有 try/catch 兜底）。

### 其它小问题（供了解，不必记）
- `BaseView.Show()` 防御性补 Canvas 时漏加 `CanvasRenderer`（与 `Initialize` 不一致，轻微，不影响渲染）。
- `BaseWidget.Hide()` 在 `gameObject == null` 时仍会执行 `OnHidden()`（`OnHidden` 在 if 之外）。
- `GetComponent` 缓存 key 不含 `parent`，同名 path + 同类型但不同 parent 会命中同一缓存（边界坑）。

---

## 11. 如何新增一个 View / Widget（完整清单）

### 新增 View（页面）
1. `Module/<模块>/` 下建类，继承 `BaseView`。
2. 需要数据就构造注入 Model：`public XxxView(XxxModel model) { _model = model; }`（Model 需继承 `BaseModel`，自动被反射注册）。
3. 覆写 `OnInit(root)`：`GetComponent<类型>("Hierarchy路径")` 缓存组件 + `AddListener` 绑按钮 + `EventManager.AddListener` 订事件。
4. 覆写 `OnShown / OnHidden / OnDestroy` 处理显隐与清理（`OnDestroy` 里 `RemoveAllListeners` + `RemoveListener` + 销毁子 widget）。
5. 做 prefab，放 `Assets/Res/Prefab/...`。
6. `UIRegister` 加一行 `ViewEntry` 并加入 `All`。
7. 调用：`UIManager.Instance.OpenView(UIRegister.Xxx, data, UILayer.Top).Forget();`

### 新增 Widget（控件）
1. 继承 `BaseWidget`，覆写 `protected override ViewEntry ViewEntry => UIRegister.Xxx;`。
2. 覆写 `OnInit` 缓存组件；`OnShown(object data)` 里 `if (data is Args args)` 取参。
3. 定义一个内部类 `Args` 传参。
4. 做 prefab。
5. `UIRegister` 注册。
6. 创建：View 内 `SetWidget<T>(entry, parent)` 或直接 `new T()` + `Initialize(parent)`。

---

## 12. 命名陷阱（实际踩过坑的地方）

- **`CardView` 这个类名是「卡牌 widget」，但 `UIRegister.CardView` 这个注册名对应的却是 `CardListView`（手牌栏 View）**。
  - `UIRegister.BaseCard = new(typeof(CardView), "Card/BaseCard.prefab")` ← 卡牌 widget
  - `UIRegister.CardView = new(typeof(CardListView), "MainView/CardView.prefab")` ← 手牌栏 View
  - 即：**注册名（CardView）≠ C# 类名（CardView）**，读代码时先看 `typeof(...)` 别被名字带偏。（这是 Maple_LeavesY「修改 Register 注册名和 Prefab 名」留下的，不是 lemo 的锅。）

- `ViewEntry.Name` 只是「文件名」，不是「资源 key」，别拿它去 `Release`（Bug 1 就是这么来的）。

---

## 13. 速查：lemo 的固定习惯

| 习惯 | 例子 |
| --- | --- |
| 组件缓存用 `GetComponent<T>("路径")` | `GetComponent<Button>("Main/CloseBtn")` |
| 参数用内部类 `Args` | `OneLineDataWidget.Args`、`CraftSlotWidget.Args` |
| 拿数据用构造注入 Model | `public MainView(MainModel model)` |
| 事件用字符串 key | `GlobalEvent.OnCardDrawn` |
| 打开/关闭视图用 UIManager | `OpenView(...).Forget()` / `CloseView(this)` |
| 异步用 UniTask，`Forget()` 丢弃 | `ShowPropertyList().Forget()` |
| 按钮绑定前先 `RemoveAllListeners` | `btn.onClick.RemoveAllListeners()` |
| 销毁时解绑一切 | `OnDestroy` 里 `RemoveAllListeners` + `RemoveListener` |
