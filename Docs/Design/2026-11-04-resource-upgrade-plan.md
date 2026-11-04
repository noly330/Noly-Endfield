# 资源管理升级计划（ResourceScope）

> 创建时间：2026-11-04
> 分支：`feature/resource-upgrade`（从 `main@b9de065` 拉出）
> 参考实现：`D:\Project\Unity\work\2035client`（`Assets/Scripts/Core/ResourceScope.cs`、`ResourceLoader.cs`、`Core/UI/*`）
> 参考提交：`4595583`（Widget 兼容 VContainer + ResourceLoader 优化）、`56a3a17`、`dbefdf5`
> 后续分支：`feature/ui-mvvm`（本分支完成并合并后再开）

---

## 1. 背景与目标

### 现状问题

| 问题 | 表现 |
|---|---|
| 资源靠手工 `Release` 配对 | 每个 `Destroy` 都要记得释放，漏一处就常驻内存；`BaseView`/`BaseWidget` 各写一遍 |
| 并发加载没有去重 | 同一 path 同时 `Load` 会发多次 Addressables 请求，缓存/引用计数可能错乱 |
| `UIManager` 失败路径泄漏 | `Resolve` 失败、`Initialize` 抛异常时，`viewGo` 不销毁、资源不释放 |
| 无法同步取已加载资源 | 只能 `await Load`，预载后没有安全的同步入口 |

### 本分支目标

1. 引入 **`ResourceScope`**：作用域内加载的资源，Dispose 时自动按引用计数释放，不再手工配对。
2. `ResourcesLoader` 补 **并发去重**、**`Get/TryGet`**、**`BeginScope`**。
3. **View / Widget 级资源所有权**：界面销毁时，它加载的 Prefab / Sprite / 图集一起回收。
4. **`UIManager` 失败路径清理**：不残留 GameObject 和 Addressables handle。

### 明确不做（避免范围膨胀）

- ❌ 毛玻璃背景（`blurEnabled` / `RefreshBackgroundBlur`）
- ❌ `IView.Update` + `UIManager.Tick` 的 EntryPoint 化
- ❌ `OpenView(..., Immediately)` 参数
- ❌ `UIManager` / `ModelManager` 改成 `RegisterEntryPoint`
- ❌ MVVM 相关的一切（ViewModel / 绑定 / 基类泛型化）→ 留给 `feature/ui-mvvm`

---

## 2. 分支与交付节奏

```
main@b9de065
  └─ feature/resource-upgrade   ← 本分支：只做资源层 + 必要的生命周期修正
       └─ (合并回 main)
            └─ feature/ui-mvvm  ← 后续分支：MVVM 基础设施 + 逐屏迁移
```

原则：

- 本分支每个提交保持**可编译、可运行**；
- 不顺手夹带 UI 重构或功能改动；
- 资源层稳定并验收通过后，再开 MVVM 分支。

---

## 3. 改造清单

### 3.1 新增 `Assets/Scripts/Core/ResourceScope.cs`

对齐 2035 的 `ResourceScope`，命名空间改为 `Endfield.Core`。

```csharp
public sealed class ResourceScope : IDisposable
{
    public UniTask<T> Load<T>(string path) where T : Object;   // 加载并纳入跟踪
    public UniTask Preload<T>(params string[] paths) where T : Object;
    public UniTask<T[]> Loads<T>(params string[] paths) where T : Object;
    public T Get<T>(string path) where T : Object;             // 同步取已加载资源
    public void Dispose();                                      // 对本作用域跟踪的 path 逐个 Release
}
```

要求：

- [ ] `Dispose` 幂等（重复调用不重复 Release）
- [ ] 只释放本作用域加载的资源，不影响外部/全局预载
- [ ] `Load` 失败（返回 null）时不纳入跟踪

### 3.2 `Assets/Scripts/Core/ResourceLoader.cs`

保留现有 API，做增量增强：

- [ ] 新增 `ResourceScope BeginScope()`
- [ ] 新增 `T Get<T>(string path)`：未加载时抛 `InvalidOperationException`（显式错误，不返回 null）
- [ ] 新增 `bool TryGet<T>(string path, out T asset)`：安全版本
- [ ] 新增并发去重：`Dictionary<string, UniTaskCompletionSource<AsyncOperationHandle>> _inflight`
- [ ] 加载失败时 `Addressables.Release(handle)` 并返回 `default`
- [ ] `ReleaseAll` 一并清空 `_inflight`
- [ ] **保留** `Load/Release/LoadNoCache/Loads/LoadPrefab(ViewEntry)`，避免影响现有调用方

### 3.3 `Assets/Scripts/Core/UI/BaseView.cs`

- [ ] 新增 `protected ResourceScope Scope { get; private set; }`
- [ ] 新增重载 `Initialize(Transform root, ResourceScope scope)`，内部赋值 `Scope` 后走原初始化流程
- [ ] **保留** 现有 `Initialize(Transform root, string viewName)`，现有 View 不需要改
- [ ] `Destroy()` 里 `Scope?.Dispose(); Scope = null;`
- [ ] 不改 `OnInit/OnShown/OnHidden/OnDestroy` 签名

### 3.4 `Assets/Scripts/Core/UI/BaseWidget.cs`

- [ ] 新增 `protected ResourceScope Scope { get; private set; }`
- [ ] 新增 `Initialize(Transform parent, ResourceScope scope)` 重载；未传 scope 时自行 `BeginScope()`
- [ ] `Destroy()` 里 `Scope?.Dispose()`
- [ ] 新增 `[Inject] protected IObjectResolver Container`
- [ ] 新增 `protected UniTask<T> CreateWidget<T>(ViewEntry entry, Transform parent)`（与 `BaseView.SetWidget` 对称）
- [ ] `SetWidget` 调用处改为把父 View 的 scope 传下去（Widget 资源随父界面一起回收）

### 3.5 `Assets/Scripts/Core/UI/UIManager.cs`

- [ ] `OpenView` 未命中缓存时 `var scope = ResourcesLoader.Instance.BeginScope();`
- [ ] 用 `scope.Load<GameObject>(entry.PrefabPath)` 替代 `LoadPrefab`
- [ ] prefab 为 null：`scope.Dispose()` 后返回
- [ ] `Container.Resolve` 失败：`Object.Destroy(viewGo)` + `scope.Dispose()` 后返回
- [ ] `await viewInstance.Initialize(...)` 用 try/catch 包裹：异常时 `Destroy(viewGo)` + `scope.Dispose()` 并重新抛出
- [ ] 成功路径把 scope 移交给 View（由 View 在 Destroy 时释放）
- [ ] 保持现有缓存、层级、`_viewStack` 逻辑不变

### 3.6 可选（Phase 1.5，本分支可后置）

- [ ] `BaseModel` 增加 `Scope/PreLoad/Dispose`（参考 2035），但**不改** `ModelManager` 的 EntryPoint 化
- [ ] `ModelManager` 只在 `GameLifetimeScope` 里保持现状

> 说明：`BaseModel` 的生命周期改造服务于后续 MVVM 的「数据预载」，不阻塞资源层验收，可放到本分支末尾或单独提交。

### 3.7 文档

- [ ] 本计划文件
- [ ] `Docs/README.md` 索引增加本文件链接
- [ ] 完成后补一节「验收记录」（日期、测试项、结果）

---

## 4. 兼容策略

| 变更 | 兼容做法 |
|---|---|
| `IView.Initialize` 需要传 scope | **新增重载**，旧签名保留；`UIManager` 内部切到新重载 |
| `BaseWidget.Initialize` 同理 | 新增重载，旧调用不改 |
| `ResourcesLoader` 新 API | 纯新增，不动旧方法 |
| `BaseView/BaseWidget` 加字段 | 不影响子类 |
| 命名空间 | 统一 `Endfield.Core`，不要直接复制 2035 的 `Core` 命名空间 |

预期：**现有 View 源码零改动**（`OperatorDisplayView`、`TopToolBarView`、`CombatHUDView` 等都不用动）。

---

## 5. 验收标准

### 编译

- [ ] Unity 编译 **0 error**；警告不新增（现有 CS0252 可顺手修）
- [ ] 现有界面代码未被修改仍能编译

### 运行（Play 模式）

- [ ] 启动游戏，打开 `OperatorDisplayView`，正常显示头像列表与 3D 预览
- [ ] 反复打开/关闭 10 次：不报错、不重复实例化
- [ ] 关闭界面后，其加载的 Sprite/图集被释放（引用计数归零）
- [ ] 退出 Play，无 Addressables handle 泄漏警告

### 资源计数验证

- [ ] 加一个仅 Editor/Development 可用的调试方法（如 `ResourcesLoader.DebugDumpRefCounts()`），打印当前 path 与引用计数
- [ ] 关闭所有界面后，计数表为空（或只剩全局预载项）

### 失败路径

- [ ] 故意把某个 `ViewEntry.PrefabPath` 写错：日志报错，场景中不残留空 GameObject，计数表无脏数据

---

## 6. 风险与回滚

| 风险 | 应对 |
|---|---|
| 引用计数多加/漏减导致资源不释放或过早释放 | 所有加载统一走 `ResourceScope`；`Dispose` 幂等；验收时用计数表检查 |
| `Initialize` 重载产生歧义（传 `null`） | 调用方只传强类型变量；禁止 `Initialize(root, null)` 写法 |
| 并发去重实现错误导致死等 | `UniTaskCompletionSource` 必须在 `finally` 中清理 `_inflight`；异常路径要 `TrySetException` |
| 与 2035 后续版本继续分叉 | 只移植「正确性」相关部分；差异记录在本文件，不追全量同步 |
| 改动波及所有界面 | 兼容重载 + 现有 View 零改动的策略；出问题可整体 `git revert` 本分支提交 |

回滚方式：

```powershell
# 丢弃整个分支（未合并时）
git switch main
git branch -D feature/resource-upgrade
```

---

## 7. 后续 MVVM 分支（预告，不在本分支做）

`feature/ui-mvvm` 的前置条件：本分支已合并、资源计数验收通过。

计划内容（详见后续文档）：

1. `ViewModelBase`（`OnActivated/OnDeactivated/Dispose`）
2. 绑定工具（文本 / 图片 / 按钮 / 列表）
3. `BaseView<TVm>`（VM 由容器解析，绑定在 `Destroy` 释放）
4. 试点迁移 `OperatorDisplayView`
5. 约定「数据驱动界面用 MVVM，表现驱动界面保持命令式」

**衔接点**：VM 不直接持有 `Sprite/GameObject`，只暴露资源路径/ID；View 用 `Scope.Load` 加载并绑定，资源随界面生命周期回收。

---

## 8. 参考文件对照

| 2035client | 本项目 | 处理 |
|---|---|---|
| `Assets/Scripts/Core/ResourceScope.cs` | 新增 `Assets/Scripts/Core/ResourceScope.cs` | 移植，改 namespace |
| `Assets/Scripts/Core/ResourceLoader.cs` | `Assets/Scripts/Core/ResourceLoader.cs` | 增量合并，不整体替换 |
| `Assets/Scripts/Core/UI/BaseView.cs` | `Assets/Scripts/Core/UI/BaseView.cs` | 只取 Scope 相关，不取 blur/Update |
| `Assets/Scripts/Core/UI/BaseWidget.cs` | `Assets/Scripts/Core/UI/BaseWidget.cs` | 取 Scope + Container + CreateWidget |
| `Assets/Scripts/Core/UI/UIManager.cs` | `Assets/Scripts/Core/UI/UIManager.cs` | 只取 scope 创建/移交/失败清理 |
| `Assets/Scripts/Core/Model/BaseModel.cs` | `Assets/Scripts/Core/Model/BaseModel.cs` | 可选 Phase 1.5 |

---

## 9. 进度记录

| 日期 | 事项 | 状态 |
|---|---|---|
| 2026-11-04 | 创建分支 `feature/resource-upgrade`，编写本计划 | ✅ |
| — | 3.1 `ResourceScope` | ⬜ |
| — | 3.2 `ResourcesLoader` | ⬜ |
| — | 3.3 `BaseView` | ⬜ |
| — | 3.4 `BaseWidget` | ⬜ |
| — | 3.5 `UIManager` | ⬜ |
| — | 3.6 `BaseModel`（可选） | ⬜ |
| — | 编译 + Play 验收 | ⬜ |
