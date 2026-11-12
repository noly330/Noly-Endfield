# 输入系统升级：IInputSource + InputService

> 2026-11-04 ｜ 分支：`main` ｜ 状态：当前版本

## 结论

- 把「输入从哪来」和「输入用来干什么」分开：消费者依赖 `IInputSource`，通过 `InputService` 取当前源。
- 方向正确（依赖倒置）；但当前实现「换源」实际不生效，消费者绑定模板重复，UI 模态无栈。

## 问题（升级前）

| 现象 | 原因 |
|---|---|
| UI/玩法直接依赖 `UnityEngine.InputSystem` | `PlayerInputSystem` 暴露裸 `InputAction`，业务被迫处理 `CallbackContext` |
| 订阅散落、可能泄漏 | 每个类各自 `OnEnable/OnInit` 订阅，漏退订就回调已销毁对象 |
| 初始化顺序敏感 | `PlayerInputSystem.Instance == null` 时直接 return，之后不重试 |
| UI 兼职设备开关 | View 调 `SetPlayerInputEnabled`，成对调用、无栈 |
| 单例职责过载 | 单例 + 硬件读取 + 事件分发 + UI map 控制挤在一个类 |
| 无法替换来源 | 输入写死本机键鼠，回放/AI/联机无处接入 |

## 方案

```
InputAction.performed
  → PlayerInputSystem.XxxPerformed
  → IInputSource 的 C# 事件
  → 消费者
```

- 新增 `IInputSource`：轮询（Move/Look/Scroll）+ 事件（Dash/Attack/Switch/Skill/LinkSkill/UI）。
- 新增 `InputService.Current` 静态网关。
- `PlayerInputSystem` 实现接口，Awake 注册、OnDestroy 注销。
- 收益：UI/玩法不再引用 InputSystem；订阅集中到 `SubscribeActions`；`Skill1~4` 合并为 `Action<int> OnSkill`。

## 现状 / 验证

- 当前项目采用此版本；编译仅剩 `PlayerInputSystem.cs:42` 的 CS0252。
- `ThirdPersonCamera` 每帧读 `Current`，是唯一能正确处理源变化的消费者。
- 其余消费者用 `_input ??= InputService.Current`，`null` 后不重试。

## 遗留

- [ ] **换源不生效**：`??=` 缓存旧源；`TopToolBar/OperatorDisplay` 订阅与退订各读一次 `Current`，换源后会退订错对象。
- [ ] **绑定模板重复**：每个 View/Controller 20~40 行输入管道代码。
- [ ] **设备概念泄漏**：`SetPlayerInputEnabled` 应改为 `UIManager` 模态栈 + 引用计数。
- [ ] **静态定位与 VContainer 并存**。
- [ ] 推荐演进（未实施）：`InputService` 门面转发事件 → `UIInputRouter` 集中 UI 输入 → `UIManager` 管理模态。

## 相关文件

- `Assets/Scripts/Player/IInputSource.cs`
- `Assets/Scripts/Player/InputService.cs`
- `Assets/Scripts/Player/PlayerInputSystem.cs`
- `Assets/Scripts/Character/CharacterPlayerController.cs`
- `Assets/Scripts/Team/TeamInputController.cs`
- `Assets/Scripts/Module/UI/TopToolBar/TopToolBarView.cs`
- `Assets/Scripts/Module/UI/OperatorDisplay/OperatorDisplayView.cs`
- `Assets/Scripts/Player/ThirdPersonCamera.cs`

提交：`7bc1cc5`
