# Noly-Endfield UI 框架学习笔记

> 说明：本文档基于实际代码（非注释）整理。文中专门标注了「⚠️ 注释误导」的部分，
> 表示原代码注释有错误或容易误导，以正文为准。

---

## 1. 框架概览

| 项 | 内容 |
| --- | --- |
| 框架代码位置 | `Assets/Scripts/Core/UI/`（框架）+ `Assets/Scripts/Module/UI/`（业务视图） |
| 依赖注入 | VContainer（`jp.hadashikick.vcontainer` 1.19.0） |
| 资源加载 | Unity Addressables（经 `ResourcesLoader` 封装） |
| 异步 | UniTask |
| 视图形态 | **视图类是纯 C# 类（不是 MonoBehaviour）**，通过持有 `Transform` 引用操作 UI |

### 目录结构

```
Assets/Scripts/
├── Core/UI/                  ← UI 框架
│   ├── IView.cs              接口：生命周期契约
│   ├── BaseView.cs           页面级视图基类
│   ├── BaseWidget.cs         组件级控件基类
│   ├── ViewEntry.cs          视图描述符（类型 + prefab 路径 + 名字）
│   ├── UIRegister.cs         视图注册表（手动列举）
│   └── UIManager.cs          单例管理器（打开/关闭/缓存/分层/弹窗栈）
├── Core/
│   ├── GameLifetimeScope.cs  VContainer 容器（注册模型 + 视图 + 手动注入单例）
│   ├── GameLauncher.cs       启动入口（初始化 UIManager、打开首个 UI）
│   ├── Singleton.cs          C# 单例 / MonoBehaviour 单例基类
│   ├── ResourceLoader.cs     Addressables 封装（带引用计数缓存）
│   ├── Model/                BaseModel / IModel / ModelManager（数据层）
│   └── Event/                EventCenter + Events（结构体消息事件）
└── Module/UI/                ← 业务视图
    ├── MainView/MainView.cs
    └── TopToolBar/TopToolBarView.cs
```

---
 
## 2. 核心类职责

### 2.1 IView（接口）
```
UniTask Initialize(Transform root, string viewName = "");
void Show(object data = null);
void Hide();
void Destroy();
```
统一的生命周期契约，`BaseView` 与 `BaseWidget` 都实现它。

### 2.2 ViewEntry（视图描述符）
```csharp
public class ViewEntry {
    [CanBeNull] public Type ViewType { get; }   // 视图的 C# 类型
    public string PrefabPath { get; }           // Addressables 资源路径
    public string Name { get; }                 // 由 prefab 文件名提取
}
```
`Name` = `Path.GetFileNameWithoutExtension(PrefabPath)`，用于给实例化后的 GameObject 命名。

### 2.3 UIRegister（视图注册表）
```csharp
public static readonly ViewEntry MainView = new(typeof(MainView), "Assets/Res/Prefab/UI/MainView/MainView.prefab");
public static readonly ViewEntry TopToolBarView = new(typeof(TopToolBarView), ".../TopToolBarView.prefab");
public static IReadOnlyList<ViewEntry> All { get; } = new[] { MainView, TopToolBarView };
```
**新增视图需要手动在这里加一行**（没有自动扫描机制）。

### 2.4 UIManager（单例管理器）
- `_viewCache`：`Dictionary<string, IView>`，key 是 **PrefabPath**。一个 prefab 对应一个视图实例（页面级单例缓存）。
- `_viewStack`：`Stack<IView>`，**只有 `UILayer.Pop` 的视图会入栈**（用于弹窗栈管理）。
- `_canvasCache`：`Dictionary<UILayer, Transform>`，缓存 UI 根 Canvas 下 4 个分层节点。

---

## 3. 分层机制

### 3.1 UILayer 枚举
```csharp
public enum UILayer { Bottom = 0, Middle = 2000, Top = 5000, Pop = 9000 }
```

⚠️ **注释误导**：这些数字（0 / 2000 / 5000 / 9000）看起来像「渲染排序值（sortingOrder）」，但实际上**它们只是字典 key 的枚举标识，从未被当作排序值使用**。真正的遮挡顺序由场景里 4 个子节点在 Hierarchy 中的**兄弟顺序**决定。

### 3.2 场景结构（真实渲染层级来源）
场景 `GameLauncher.unity` 中存在一个 `UI` 根 Canvas，其子节点顺序为：

```
UI (Canvas, ScreenSpaceOverlay, 1920x1080)
├── Bottom   ← 最底层
├── Middle
├── Top
└── Pop      ← 最顶层
```

Unity UI 在同一 Canvas 内按 Hierarchy 顺序渲染（越靠后的兄弟越靠上）。所以真正决定"谁挡住谁"的是 **Bottom→Middle→Top→Pop 这个节点顺序**，而不是枚举数值。

`UIManager.InitCanvasLayers()` 只是用 `transform.Find("Bottom")` 等**按名字**查找并缓存这 4 个节点；`OpenView` 把视图实例化到对应节点下。

### 3.3 SortOrder（视图内独立排序）
`BaseView` / `BaseWidget` 都有：
```csharp
protected virtual int SortOrder => 0;   // 子类可覆写
```
当 `SortOrder > 0` 时，会给视图根节点动态加一个**嵌套 Canvas**，并 `overrideSorting = true`，从而让该视图无视父级、独立控制自己的渲染顺序。这是"子 Canvas 独立排序"的标准做法。

---

## 4. 打开 / 关闭 / 销毁流程

### 4.1 打开视图 `UIManager.OpenView(entry, data, layer)`

```
1. 查 _viewCache（key = entry.PrefabPath）
   └─ 命中 → cachedView.Show(data)；若是 Pop 层则入栈；return
2. 未命中 → ResourcesLoader.LoadPrefab(entry)  ← Addressables 异步加载 prefab
3. Instantiate(prefab, 对应 layer 节点, false)
4. viewGo.name = entry.Name
5. Container.Resolve(entry.ViewType)  ← 创建视图「类实例」（纯 C# 对象）+ 注入依赖
6. await viewInstance.Initialize(viewGo.transform, entry.Name)
7. viewInstance.Show(data)
8. _viewCache[entry.PrefabPath] = viewInstance
9. 若是 Pop 层 → _viewStack.Push(viewInstance)
```

**关键理解**：步骤 5 的 `Resolve` 创建的是**纯 C# 视图对象**，它和步骤 3 实例化出来的 GameObject 是分开的。两者通过 `Initialize(transform, name)` 建立联系——视图对象持有 `RootTransform` 引用去操作那个 GameObject。视图**不是**通过 `AddComponent` 挂上去的 MonoBehaviour。

### 4.2 关闭视图（两个重载，默认行为不同）

```csharp
CloseView(ViewEntry entry, bool destroy = false);  // 默认只 Hide（保留缓存）
CloseView(IView view, bool destroy = true);        // 默认 Destroy（释放资源）
```

⚠️ **注释误导 / 隐患**：两个同名方法对 `destroy` 的**默认值语义相反**（一个 false 一个 true），调用时极易混淆，务必显式传参。

关闭时都会执行一次"从 `_viewStack` 中移除该视图"的栈重建（用临时栈过滤掉目标视图）。

### 4.3 销毁
- `DestroyView(ViewEntry entry)`：`CloseView(entry)`（Hide）→ `view.Destroy()` → 移除缓存 → 释放资源。
  ⚠️ **隐患**：`CloseView(entry)` 里的 `Hide()` 已经触发一次 `OnHidden()`，紧接着 `view.Destroy()` 又触发一次 `OnHidden()`，所以 `OnHidden()` **会被调用两次**。
- `CloseAllViews()`：先把 `_viewStack` 里的视图（即 Pop 层）逐个 `Hide()`，再遍历 `_viewCache` 全部 `Destroy()` 并释放资源，最后清空缓存。

---

## 5. 视图生命周期（BaseView）

```csharp
public virtual async UniTask Initialize(Transform root, string viewName = "")
    → 记录 _viewName / RootTransform
    → 若 SortOrder > 0：动态加嵌套 Canvas（overrideSorting）
    → await OnInit(root)          // 子类覆写：缓存组件、绑定事件

public virtual void Show(object data = null)
    → SetActive(true)
    → OnShown(data)               // 子类覆写：显示逻辑

public virtual void Hide()
    → SetActive(false)
    → OnHidden()                  // 子类覆写：隐藏逻辑

public void Destroy()
    → OnHidden() + OnDestroy()    // 子类覆写：清理逻辑
    → Object.Destroy(RootGameObject)
    → 清空组件缓存，置空 RootTransform
```

子类可覆写的钩子：`OnInit` / `OnShown` / `OnHidden` / `OnDestroy`。

### 组件缓存 GetComponent
```csharp
protected T GetComponent<T>(string path = "", Transform parent = null)
```
- 缓存 key = `$"{path}_{typeof(T).Name}"`。
- ⚠️ **注释误导（日志文案错误）**：当"路径找到了但该路径下没有目标组件"时，错误日志却打印 `Path not found: {path}`（`BaseView.cs` 第 87 行）。这是 copy-paste 错误，实际应是"组件不存在"，会误导排查方向。
- ⚠️ 缓存 key 不包含 `parent`，若同名 path + 同类型但不同 parent 会命中同一个缓存，属边界坑。

### SetWidget（嵌入子控件）
```csharp
public async UniTask<T> SetWidget<T>(ViewEntry entry, Transform parent) where T : BaseWidget
```
⚠️ **设计误导**：方法接收 `entry` 参数，但实际**只用了 `entry.ViewType` 去 Resolve**，`entry.PrefabPath` 被**完全忽略**。真正的 prefab 路径来自 widget 自身覆写的 `ViewEntry` 属性（见下）。所以调用方传入的 `entry.PrefabPath` 是"死参数"，不会影响加载哪个 prefab。

---

## 6. 控件生命周期（BaseWidget）

与 BaseView 的差异：

1. 有抽象属性 `protected abstract ViewEntry ViewEntry { get; }`，**每个 widget 自己声明要加载哪个 prefab**。
2. `Initialize(Transform parent, string _ = "")` 自己加载 prefab 并 `Instantiate(prefab, parent)`，实例化后先 `SetActive(false)`。
3. `Show` 是 `async void`，内部 `await UniTask.WaitUntil(() => gameObject != null)` 等初始化完成再显示。
4. `Destroy` 会 `ResourcesLoader.Instance.Release(ViewEntry.PrefabPath)`。

⚠️ 需要注意的点：
- `async void` 是反模式（调用方无法 await、异常传播不可控），虽然内部有 try/catch 兜底。
- `Hide()` 即使 `gameObject == null` 也会执行 `OnHidden()`（`OnHidden` 在 if 之外），与 `Show` 的等待逻辑不对称。

---

## 7. 依赖注入（VContainer）

### 7.1 注册（GameLifetimeScope.Configure）

```csharp
// 1. 反射扫描：所有 BaseModel 子类注册为单例
foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
    if (type.IsSubclassOf(typeof(BaseModel)) && !type.IsAbstract)
        builder.Register(type, Lifetime.Singleton).AsSelf();

// 2. 所有 UI 视图注册为瞬时（每次 Resolve 都是新实例）
foreach (var ve in UIRegister.All)
    if (ve.ViewType != null)
        builder.Register(ve.ViewType, Lifetime.Transient);

// 3. 对「不是容器创建的」单例，手动补依赖注入
builder.RegisterBuildCallback(container => {
    container.Inject(UIManager.Instance);
    container.Inject(ModelManager.Instance);
    container.Inject(UserDataService.Instance);
});
```

### 7.2 为什么 `Resolve(entry.ViewType)` 能成功

`builder.Register(ve.ViewType, Lifetime.Transient)` **没有**显式调用 `.AsSelf()`。VContainer 内部逻辑（`Registry.Build`）是：当 `InterfaceTypes == null` 时，把**实现类型本身当作 key** 注册。因此用具体类型（如 `typeof(MainView)`）去 `Resolve` 能命中。

### 7.3 注入发生时机（重要）

`BaseView.Container` 的注入发生在 `UIManager.OpenView` 里的 `Container.Resolve(entry.ViewType)` 那一刻：

```
Resolve(typeof(MainView))
  → new MainView()                  ← 此时 Container 属性还是 null
  → VContainer 扫描并注入所有 [Inject] 成员（含继承自 BaseView 的）
  → 返回实例                        ← Container 已可用
  → viewInstance.Initialize(...)    ← 注入早已结束
```

⚠️ **注释误导**：`BaseView.cs` 第 15 行注释写"在 UIManager.OpenView 时注入"，方向没错，但精确地说注入发生在 OpenView 内部的 `Resolve` 调用时，而不是 OpenView 一进入就注入。

---

## 8. 资源加载（Addressables + 引用计数）

`ResourcesLoader : Singleton<ResourcesLoader>` 封装 Addressables：

- `Load<T>(path)`：带缓存 + 引用计数。
  - 已加载 → 引用计数 +1，直接返回。
  - 首次 → `Addressables.LoadAssetAsync<T>`，成功后缓存并记 refCount = 1。
- `Release(path)`：引用计数 -1，归 0 才真正 `Addressables.Release`。
- `LoadPrefab(ViewEntry entry)` → `Load<GameObject>(entry.PrefabPath)`。
- `LoadNoCache<T>(path)`：加载后立即释放（不缓存）。
- `ReleaseAll()`：全部释放（`GameLauncher.OnDestroy` 调用）。

⚠️ 注意：`LoadNoCache` 无论成功失败都会在 finally 里 `Release`，所以调用方拿到的是**可能已经被释放的句柄结果**——它只适合"加载后立刻复制出数据"的场景，不能长期持有返回的引用。

---

## 9. 数据与事件

- **Model 层**：`BaseModel`（`Init`/`Dispose`）→ `ModelManager.Get<T>()` 通过 `Container.Resolve<T>()` 取单例。所有 BaseModel 子类被反射注册为单例。
- **事件中心**：`EventCenter.SubscribeListener<T>` / `UnsubscribeListener<T>` / `DispatchMessage<T>`，`T : struct`。事件消息是结构体，定义在 `Events` 静态类里（如 `OnDefenseBreakApplied`）。
- 视图与数据的典型交互：视图在 `OnInit` 里 `ModelManager.Get<XxxModel>()` 拿数据 + `EventCenter.SubscribeListener<...>` 订阅刷新；在 `OnDestroy` 里 `UnsubscribeListener`。

---

## 10. 如何新增一个视图（实践指南）

1. 在 `Assets/Scripts/Module/UI/` 下新建视图类，继承 `BaseView`（页面）或 `BaseWidget`（控件）。
2. 覆写 `OnInit`（缓存组件 + 绑定事件 + 订阅事件），必要时覆写 `OnShown/OnHidden/OnDestroy`。
3. 制作 prefab，放到 `Assets/Res/Prefab/UI/...`（Addressables 路径）。
4. 在 `UIRegister` 里加一行 `ViewEntry`，并加入 `All` 数组（否则不会被注册进容器）。
5. 调用 `UIManager.Instance.OpenView(UIRegister.YourView, data, UILayer.Top).Forget()` 打开。
   - 普通界面用 `UILayer.Bottom/Middle/Top`；
   - 弹窗用 `UILayer.Pop`（会入栈，供 `CloseAllViews` / 栈管理）。

---

## 11. ⚠️ 注释/代码问题清单（汇总）

| # | 位置 | 问题 |
| --- | --- | --- |
| 1 | `BaseView.cs:87` | 组件不存在时日志却打印 `Path not found`，文案误导 |
| 2 | `BaseView.cs:15` | "OpenView 时注入"——实际是 OpenView 内 `Resolve` 时注入 |
| 3 | `UIManager.cs:10-16` | `UILayer` 数值(0/2000/5000/9000)看似排序值，实为字典 key，渲染层级由场景节点顺序决定 |
| 4 | `UIManager.cs:90` vs `121` | 两个 `CloseView` 的 `destroy` 默认值相反（false / true），易混淆 |
| 5 | `UIManager.cs:153-160` | `DestroyView` 导致 `OnHidden()` 被调用两次（冗余） |
| 6 | `BaseView.cs:95-104` | `SetWidget` 的 `entry.PrefabPath` 被忽略，实际路径来自 widget 自身 `ViewEntry` |
| 7 | `BaseView.cs:43-47` | `Show` 防御性补 Canvas 时漏加 `CanvasRenderer`（与 Initialize 不一致，轻微） |
| 8 | `BaseWidget.cs:52` | `async void Show` 反模式（内部有 try/catch 兜底） |
| 9 | `BaseWidget.cs:66-70` | `Hide` 在 gameObject 为 null 时仍触发 `OnHidden()` |
| 10 | `GameLifetimeScope.cs:30-35` | 注册视图未显式 `.AsSelf()`，能 Resolve 是因为 VContainer 默认按实现类型注册（注释未说明） |
| 11 | `ViewEntry.cs:9` | `ViewType` 标注可空但无校验，防御在调用方（GameLifetimeScope） |
