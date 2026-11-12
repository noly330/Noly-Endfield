# 资源管理升级：ResourceScope

> 2026-11-05 ｜ 分支：`feature/resource-upgrade` ｜ 状态：已完成（未提交）

## 结论

- 资源所有权从「调用方手工 Release」改为「`ResourceScope` 管作用域，`ResourcesLoader` 管引用计数」。
- 界面销毁时，Prefab 与 `OnInit` 加载的资源一起释放；同 path 并发加载只发一次请求；失败路径不残留。
- 预览角色的引用计数仍有泄漏，见 [TODO](../../Design/2026-11-10-resource-fix-todo.md)。

## 问题

| 现象 | 原因 |
|---|---|
| 关界面后资源常驻 | 每处 Destroy 手工 Release，漏一处就泄漏 |
| 同一资源重复加载 | `Load` 没有并发去重 |
| 失败路径残留 | Resolve / Initialize 异常时 viewGo 和 handle 都不释放 |
| 预载后无法同步取用 | 只有 `await Load` |

## 方案

### 1. ResourceScope

- 只能由 `ResourcesLoader.BeginScope()` 创建（`internal` 构造）。
- 只记 `path`，不记 handle；`Dispose` 时逐个 `Release`。
- 加载失败不纳入跟踪；`Dispose` 幂等。

```csharp
var scope = ResourcesLoader.Instance.BeginScope();
var prefab = await scope.Load<GameObject>(path);
// 离开作用域：scope.Dispose() → Release(path)
```

### 2. ResourcesLoader

- 新增 `BeginScope` / `Get` / `TryGet`。
- `_inflight` 去重：同一 path 并发只发一次 Addressables 请求。
- 引用计数只在 `Load<T>` 中 +1；`EnsureLoadedAsync` 只负责缓存。

```csharp
if (_inflight.TryGetValue(path, out var pending))
    return await pending.Task;
```

### 3. UI 接线

- `IView.Initialize(Transform, ResourceScope = null)` 统一签名。
- `BaseView` 持有 scope，`Destroy` 时 `Dispose`。
- `BaseWidget` 默认自建 scope；`BaseView` 记录子 Widget，销毁时先销毁 Widget 再释放自己的 scope。
- **Widget 不共享父 scope**：否则子对象提前销毁会带走父资源。
- `UIManager`：创建 scope → 加载 prefab → 所有权移交 View；失败路径销毁 viewGo + Dispose。

### 4. 双释放

`BaseView.Destroy` 会 `Scope.Dispose()`，所以 UIManager 里四处「销毁后再 Release」必须删除：

```
CloseView(entry, destroy: true)
CloseView(view, destroy: true)
DestroyView(entry)   → 改为 CloseView(entry, destroy: true)
CloseAllViews()
```

原则：**资源释放只走 `ResourceScope.Dispose`。**

## 验证

- [x] 编译 0 error；Play 20s 0 error；退出 Play 无释放警告。
- [x] 反复开关面板 5 次：`OperatorDisplayView.prefab` 始终为 1，无累积。
- [x] `安比=2`：队伍 1 + 预览 1。
- [x] `AvatarItem=1`：对象池持有，不随界面释放（预期）。

注意：`OnHidden` 的打印发生在 `Scope.Dispose` 之前，看到的计数尚未减少。

## 遗留

- [ ] 预览角色切换泄漏 → [TODO](../../Design/2026-11-10-resource-fix-todo.md)
- [ ] 对象池并发建池 / ClearAll 不释放 → 同上
- [ ] `BaseModel` 生命周期、CloseView 策略、OpenView 并发去重 → 延后

## 相关文件

- `Assets/Scripts/Core/ResourceScope.cs`
- `Assets/Scripts/Core/ResourceLoader.cs`
- `Assets/Scripts/Core/UI/IView.cs`
- `Assets/Scripts/Core/UI/BaseView.cs`
- `Assets/Scripts/Core/UI/BaseWidget.cs`
- `Assets/Scripts/Core/UI/UIManager.cs`

参考实现：`2035client/Assets/Scripts/Core/ResourceScope.cs` 等。
