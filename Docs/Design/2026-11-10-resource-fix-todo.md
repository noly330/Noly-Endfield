# 资源管理问题修复 TODO（2026-11-10）

> 创建时间：2026-11-09
> 计划执行：2026-11-10
> 分支：`feature/resource-upgrade`
> 背景：`Docs/Notes/Unity/resource-management-upgrade.md`
> 目标：修掉「引用计数只增不减」和对象池的生命周期问题，不引入 MVVM / HybridCLR 相关改动。

---

## 0. 开工前（5 分钟）

- [ ] 确认在 `feature/resource-upgrade` 分支
- [ ] 先把当前资源升级的主体改动提交（**不要**带上 `GameLauncher.unity` 的用户改动）
- [ ] 进 Play 按 **P** 记录一次 baseline 计数（截图或复制日志）
- [ ] 决策：预览资源修复用 **方案 A（手工 Release）** 还是 **方案 B（接入 ResourceScope）**

> 推荐方案 B：`OperatorPreview` 接收视图的 `ResourceScope`，用 `Scope.Load` 代替手工 `Load/Release`，由 `BaseView.Destroy → Scope.Dispose` 兜底。代价是面板打开期间切换的干员引用会累积到关闭才释放，但不会漏。

---

## 1. P0：`OperatorPreview` 引用计数泄漏

**证据**：切换干员后按 P，`安比=4、可琳=3、比利=2`，只增不减。

**文件**：`Assets/Scripts/Module/UI/OperatorDisplay/OperatorPreview.cs`

- [ ] **1.1 切换时释放旧引用**
  `ClearCharacter()` 目前只 `Destroy(_character)`，不释放 `_loadedAddress`。
  - 方案 A：在 `ClearCharacter` 里 `Release(_loadedAddress); _loadedAddress = null;`
  - 方案 B：改用 `Scope.Load`，删除 `_loadedAddress` 与手工 `Release`

- [ ] **1.2 丢弃过期加载时释放**
  `Display()` 的 `if (version != _displayVersion) return;` 分支，刚 `Load` 的引用没有释放。
  - 方案 A：`ResourcesLoader.Instance.Release(so.prefabAddress);`
  - 方案 B：由 scope 跟踪，无需手动释放

- [ ] **1.3 关闭时作废在途加载**
  `Dispose()` 开头 `_displayVersion++;`，并在 `await` 之后再次检查版本 / disposed，避免在舞台已销毁后 `Instantiate` 出孤儿角色。

- [ ] **1.4 `Dispose` 幂等**
  重复调用不重复销毁舞台 / 不重复 Release。

- [ ] **1.5（方案 B）注入 scope**
  `OperatorPreview(ResourceScope scope)`，内部所有 prefab 加载走 `scope.Load<GameObject>`。

- [ ] **1.6 确认关闭策略**
  `OperatorDisplayView.OnEscape` 当前是 `CloseView(entry)`（Hide，不销毁），预览会随视图缓存常驻。
  - 决定：数据界面是否改成 `destroy: true`（推荐），让关闭即释放。

**验收**

- [ ] 打开面板，依次切 10 次干员 → 按 P，计数回到 baseline（允许最后一位预览角色 +1）
- [ ] Hierarchy 里 `[OperatorPreview]/CharMount` 始终只有 1 个角色
- [ ] 预览一个**非队伍**干员，关闭（`destroy: true`）后它的计数归零
- [ ] Console 无 `MissingReferenceException`、无重复 Release 警告

---

## 2. P1：`PrefabPoolManager` 并发建池

**证据**：其他 VFX 计数都是 1，唯独 `FX_slash_15 = 2`。

**原因**：`GetPoolAsync` 在 `await Load` 期间，第二次调用看不到 `_pools` 里的缓存，于是重复 `Load` + 重复建池；后写入的池覆盖字典，先建的池和它预热的实例泄漏。

**文件**：`Assets/Scripts/Core/ObjectPool/PrefabPoolManager.cs`

- [ ] **2.1 增加按 path 的在途任务去重**
  ```csharp
  private readonly Dictionary<string, UniTask<PrefabPoolBase>> _inflight = new();
  ```
  第一个调用者负责加载/建池，其余等待同一个任务。
- [ ] **2.2 保证「一个 path 只 Load 一次、只建一个池」**
- [ ] **2.3 清理 `_inflight`**（`finally`），失败时让后续调用可以重试

**验收**

- [ ] 进 Play 后立刻按 Y 生成怪 / 放技能（抢在预热完成前），再按 P
- [ ] `FX_slash_15` 保持 1，不再出现 2
- [ ] `[PoolRoot]` 下没有重复的池实例

---

## 3. P1：池的生命周期与所有权

**文件**：`PrefabPool.cs`、`PrefabPoolManager.cs`、`ObjectPool.cs`

- [ ] **3.1 `PrefabPool` 记录 `PrefabPath`**（用于释放资源）
- [ ] **3.2 `ClearAll()` 释放 prefab 引用**
  ```csharp
  foreach (var kvp in _pools)
  {
      kvp.Value.Clear();
      ResourcesLoader.Instance.Release(kvp.Key);
  }
  _pools.Clear();
  ```
- [ ] **3.3 `ObjectPool.Clear()` 处理已借出对象**
  当前只销毁空闲实例并 `_active.Clear()`，已借出的对象会留在场景里，之后 Release 还会抛「重复 Release」。
  - 方案：`Clear` 时遍历 `_active` 逐个 `actionOnDestroy`，再清空
- [ ] **3.4 明确 `ClearAll` 调用时机**
  当前没有调用点（注释写着「场景切换时调用」），决定是在场景切换、退出 Play 还是暂不调用，并写注释

**验收**

- [ ] 手动调用一次 `ClearAll`：对应 prefab 计数下降
- [ ] 重新建池后计数不累积
- [ ] 无重复 Release 异常、无残留实例

---

## 4. P2：`TeamManager` 重载编队（预防性）

**文件**：`Assets/Scripts/Team/TeamManager.cs`（`LoadTeamAsync`）

目前 `LoadTeamAsync` 只在启动调用一次，所以不发作。但代码是：

```csharp
_operators.Clear();          // 只清字典
// 旧 Operator 实例没 Destroy，旧 prefab 引用没 Release
```

- [ ] **4.1 重载前销毁旧实例、释放旧 prefab**
- [ ] **4.2 或者加注释明确「只允许启动时调用一次」**

**验收**

- [ ] 连续调用两次 `LoadTeamAsync`，队伍干员计数不增长、场景里没有重复角色

---

## 5. P2：池容量 review

- [ ] `DamageTextSystem` 预热 **100 个 TMP 实例**（`GetPoolAsync<TextMeshProUGUI>(..., 100, 200)`），评估是否降到 20~30 或按需扩容
- [ ] 检查其他池的 `defaultCapacity / maxSize` 是否合理（怪 10/30、slash 5/20、hit 10/40）

---

## 6. P3：清理与收尾

- [ ] 保留 `ResourceDebugHotkey`（编辑器 P 键），不加进构建
- [ ] 移除所有临时 `Debug.Log(DebugDumpRefCounts())`
- [ ] （可选）修 `PlayerInputSystem.cs:42` 的 CS0252：`ReferenceEquals`
- [ ] 更新 `Docs/README.md` 索引（本篇 + 资源升级笔记）
- [ ] 更新 `2026-11-04-resource-upgrade-plan.md` 的进度与验收记录
- [ ] 合并 `feature/resource-upgrade` → `main`
- [ ] 开 `feature/ui-mvvm`

---

## 7. 每次修改后的通用验证

- [ ] Unity 编译 **0 error**（只允许已知的 CS0252 warning）
- [ ] Play 冒烟 20 秒，无 error
- [ ] 按 P 对比修改前后的计数
- [ ] Console 无 `Trying to release unloaded or already released asset`
- [ ] 退出 Play 无 Addressables handle 泄漏警告

---

## 8. 不在本次范围内

- MVVM / ViewModel / 绑定层
- HybridCLR / 程序集拆分
- 把预览角色改成对象池（性能优化，不是泄漏修复）
- 重做 UI 框架

---

## 9. 关键文件索引

| 功能 | 文件 |
|---|---|
| 预览角色 | `Assets/Scripts/Module/UI/OperatorDisplay/OperatorPreview.cs` |
| 预览视图 | `Assets/Scripts/Module/UI/OperatorDisplay/OperatorDisplayView.cs` |
| 对象池管理 | `Assets/Scripts/Core/ObjectPool/PrefabPoolManager.cs` |
| 对象池实现 | `Assets/Scripts/Core/ObjectPool/PrefabPool.cs`、`ObjectPool.cs` |
| 队伍加载 | `Assets/Scripts/Team/TeamManager.cs` |
| 资源加载 | `Assets/Scripts/Core/ResourceLoader.cs`、`ResourceScope.cs` |
| 调试热键 | `Assets/Scripts/Debug/ResourceDebugHotkey.cs` |
