# 输入系统升级笔记：IInputSource + InputService

> 记录时间：2026-11-04
> 对应提交：`7bc1cc5` 重构输入系统，添加输入抽象（IInputSource + InputService）
> 当前状态：项目**暂时采用这一版**；后续更激进的改造（门面化、UI 输入路由、模态栈）已回滚，方案保留在第 6 节。

---

## 0. 一句话总结

把「**输入从哪来**」和「**输入用来干什么**」分开：业务代码不再直接依赖 `PlayerInputSystem` 单例和 `UnityEngine.InputSystem`，改为依赖 `IInputSource` 接口，通过 `InputService` 获取当前输入源。

方向是**依赖倒置**，属于架构优化，不是性能优化。

---

## 1. 升级前的结构

```
PlayerInputSystem.Instance   (SingletonMono<PlayerInputSystem>，持有 PlayerInput)
 ├─ public InputAction DashAction / AttackAction / SwitchAction
 ├─ public InputAction Skill1 / Skill2 / Skill3 / Skill4 / LinkSkill
 ├─ public InputAction CloseView / OpenOperatorDisplay
 ├─ public Vector2 Move / Look / Scroll
 └─ public void SetPlayerInputEnabled(bool)

消费者直接抓到 Instance：
 ├─ CharacterPlayerController  → DashAction.performed / AttackAction.performed / Move
 ├─ TeamInputController        → SwitchAction / Skill1..4 / LinkSkill 的 performed
 ├─ TopToolBarView             → OpenOperatorDisplay.performed
 ├─ OperatorDisplayView        → CloseView.performed + SetPlayerInputEnabled(false/true)
 └─ ThirdPersonCamera          → Look / Scroll
```

特点：**能跑、直接、好调试**。在单机单人项目里，这套设计短期没有任何问题。

---

## 2. 升级前的问题

### 2.1 抽象泄漏：把 `InputAction` 暴露给业务代码（最根本）

`DashAction`、`Skill1` 返回的是 Input System 的内部句柄。后果：

- 每个消费者被迫 `using UnityEngine.InputSystem`，还要处理 `InputAction.CallbackContext` —— 而业务只关心「按没按」；
- 输入方案绑死在 Input System 上，换方案要改 5 个消费者；
- 无法在测试里伪造输入（得先造真实 `PlayerInput` 和场景单例）。

### 2.2 订阅生命周期散落，而且「一次性」

| 消费者 | 订阅时机 | 退订时机 | 额外状态 |
|---|---|---|---|
| `CharacterPlayerController` | `OnEnable` | `OnDisable` | 无 |
| `TeamInputController` | `OnEnable` | `OnDisable` | 无 |
| `TopToolBarView` | `OnInit` | `OnDestroy` | 无 |
| `OperatorDisplayView` | `OnShown` | `OnHidden` + `OnDestroy` | `_escSubscribed` |
| `ThirdPersonCamera` | 每帧轮询 | — | 无 |

两个真实隐患：

1. **漏退订 → 回调到已销毁对象**。`_escSubscribed` 这种手工标志就是模式不健壮的证据。
2. **初始化顺序依赖 + 不重试**：

```csharp
if (PlayerInputSystem.Instance == null) return;   // 之后没有任何重试
```

`SingletonMono.Instance` 首次访问走 `FindObjectOfType`。如果消费者比输入系统先初始化（跨场景、对象池、单独打开某个场景调试），订阅被直接跳过，**之后永远没有输入**，而且不报错，极难排查。

### 2.3 UI 层反过来控制设备层

```csharp
// OperatorDisplayView.OnShown
PlayerInputSystem.Instance?.SetPlayerInputEnabled(false);
```

- View 需要理解「Player map / UI map」这种硬件实现细节；
- 冻结是**成对调用**，没有栈也没有引用计数：A 打开冻结、B 打开又冻结（无效）、A 关闭就解冻，而 B 还开着 → 角色在 B 后面能移动；
- 任何一个界面漏调 `OnHidden` → 永久冻结；
- `PlayerInputSystem.OnEnable` 无条件 `Player.Enable()`，对象经历一次 Disable→Enable（切场景/池化）就会丢失冻结状态。

### 2.4 单例职责过载（SRP）

`PlayerInputSystem` 同时承担：单例管理、硬件读取、事件分发、UI 输入开关。四种变更原因挤在一个类里。

### 2.5 重复代码

4 个技能各有一个属性和一个几乎相同的 handler，加第 5 个技能要改 4 处。

### 2.6 没有「输入来源」的概念

默认「输入永远来自本机键盘鼠标」。一旦要做输入回放、AI 托管、联机、按键重绑定，消费者全部要改。

### 2.7 客观评价

| 问题 | 性质 |
|---|---|
| 暴露 `InputAction`、UI 依赖硬件层 | **结构性**，决定后续扩展成本 |
| null 后不重试 | **潜在 bug**，当前加载顺序恰好没触发 |
| UI 模态成对调用 | **潜在 bug**，多 UI 叠加时必现 |
| 单例职责过载、重复代码 | 可维护性问题 |
| 性能 | **不是问题**，`ReadValue` 开销可忽略 |

---

## 3. 升级后的结构

```
IInputSource (接口)
 ├─ Vector2 Move / Look / Scroll        (轮询型)
 ├─ event Action OnDash / OnAttack
 ├─ event Action OnSwitch
 ├─ event Action<int> OnSkill
 ├─ event Action OnLinkSkill
 ├─ event Action OnCloseView / OnOpenOperatorDisplay
 └─ void SetPlayerInputEnabled(bool)

InputService (静态网关)
 └─ public static IInputSource Current { get; set; }

PlayerInputSystem : SingletonMono<PlayerInputSystem>, IInputSource
 └─ Awake 里 InputService.Current = this
```

事件流：

```
InputAction.performed
   → PlayerInputSystem 的 XxxPerformed(CallbackContext)
   → IInputSource 的 C# 事件（OnDash / OnSkill(int) / ...）
   → 消费者
```

### 升级带来的实际收益

1. **UI / 玩法不再依赖 InputSystem 包和具体单例**，只依赖接口；
2. 订阅集中在 `PlayerInputSystem.SubscribeActions()` 一处，消费者不再重复订阅 `InputAction`；
3. `Skill1~4` 合并成 `Action<int> OnSkill`，去掉 4 个重复 handler；
4. 为 AI / 回放 / 网络输入留了替换入口（但目前有坑，见 5.1）。

---

## 4. 当前版本的实际写法

```csharp
// PlayerInputSystem：Awake 注册、OnDestroy 注销
protected override void Awake()
{
    base.Awake();
    if (Instance != this) return;
    EnsureInput();
    SubscribeActions();
    InputService.Current = this;
}

private void OnDestroy()
{
    UnsubscribeActions();
    if (InputService.Current == this)   // ← CS0252：接口和 UnityEngine.Object 比较
        InputService.Current = null;
}
```

```csharp
// 消费者：注意这里缓存了源，且 null 时不会重试
private IInputSource _input;

private void OnEnable()
{
    _input ??= InputService.Current;
    if (_input == null) return;
    _input.OnDash += OnDashStart;
}

private void OnDisable()
{
    if (_input == null) return;
    _input.OnDash -= OnDashStart;
}
```

---

## 5. 当前版本的已知问题（技术债清单）

### 5.1 「替换 Current 即可换源」实际不成立

`InputService` 注释写着「将来联机/输入回放时替换 Current 即可，消费者零改动」，但消费者的写法互相不一致：

| 位置 | 写法 | 换源后 |
|---|---|---|
| `CharacterPlayerController` | `_input ??= InputService.Current` | 永远用旧源 |
| `TeamInputController` | 同上 | 永远用旧源 |
| `TopToolBarView` | 订阅时读 Current，**退订时再读一次** | 旧源泄漏订阅，退订打在新源上（无效） |
| `OperatorDisplayView` | 同上 | 同上 |
| `ThirdPersonCamera` | 每帧读 Current | ✅ 正常 |

结论：接口化只做了一半，**缺少「源变更通知」**。

### 5.2 消费者绑定模板膨胀

每个 View / Controller 都要自己写 `_input` 字段 + 判空 + 订阅 + 退订，两个 View 里各多出 20~40 行和界面无关的输入管道代码。**基础设施的复杂度摊到了使用者身上**，这是明确的设计缺陷。

### 5.3 设备层概念泄漏 + 模态无栈

- `SetPlayerInputEnabled(bool)` 属于硬件实现（Player map），网络/AI 源没有这个概念；
- UI 靠 `OnShown/OnHidden` 成对调用，多 UI 叠加会互相解冻；
- `PlayerInputSystem.OnEnable` 无条件启用 Player map，Disable→Enable 后冻结状态丢失。

### 5.4 编译器警告 CS0252

```
Assets\Scripts\Player\PlayerInputSystem.cs(42,13): warning CS0252
```

`IInputSource`（接口）和 `this`（`UnityEngine.Object` 派生）比较时，编译器选中 Unity 重载的 `operator ==`。功能上碰巧正确，但语义混搭。

一行修复：

```csharp
if (ReferenceEquals(InputService.Current, this))
    InputService.Current = null;
```

### 5.5 静态服务定位与 VContainer 并存

项目已经用 VContainer 管 UI/Model 的依赖，输入系统又开了一套静态全局入口，两套依赖获取方式并存。`IInputSource` 这种可替换服务其实是 DI 的典型场景。

当前没有改成 DI 的原因：玩法对象都是 `Instantiate` 出来的、场景组件是序列化引用，全部改注入需要动到 `TeamManager` 的生成点和场景对象，改动面大、运行时风险高。

### 5.6 其他

- `EnsureInput()` 在引用缺失时静默 `new PlayerInput()`，会掩盖 Inspector 配置错误；
- 静态状态在「关闭域重载」模式下会跨播放残留（当前 `m_EnterPlayModeOptionsEnabled: 0`，暂时安全）；
- `Current` 是公开 setter，任何代码都能改，没有所有权约束。

---

## 6. 推荐的演进方案（未实施，留档）

目标：**消费者只认识稳定事件，换源逻辑只存在于输入基础设施里，UI 不出现输入代码。**

### 6.1 `InputService` 门面化

```csharp
public static class InputService
{
    public static IInputSource Current { get; private set; }
    public static event Action<IInputSource> Changed;   // 保留给需要源对象的场景

    public static event Action OnDash;
    public static event Action OnAttack;
    public static event Action OnSwitch;
    public static event Action<int> OnSkill;
    public static event Action OnLinkSkill;
    public static event Action OnCloseView;
    public static event Action OnOpenOperatorDisplay;

    public static Vector2 Move => Current != null ? Current.Move : Vector2.zero;
    public static Vector2 Look => Current != null ? Current.Look : Vector2.zero;
    public static Vector2 Scroll => Current != null ? Current.Scroll : Vector2.zero;

    public static void Register(IInputSource source)   // 内部 attach 转发
    public static void Unregister(IInputSource source) // 内部 detach 转发

    // 模态：引用计数 + 幂等
    public static void PushGameplayBlock(object owner);
    public static void PopGameplayBlock(object owner);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { /* 清空静态状态 */ }
}
```

消费者：

```csharp
InputService.OnDash += OnDashStart;          // OnEnable
InputService.OnDash -= OnDashStart;          // OnDisable
Vector2 move = InputService.Move;            // 不用判空
```

### 6.2 UI 输入集中路由

新增 `UIInputRouter`（由 `GameLauncher.Awake` 创建），全局只在这里订阅 UI 输入：

```csharp
private void OnEnable()
{
    InputService.OnCloseView += CloseTopModal;
    InputService.OnOpenOperatorDisplay += OpenOperatorDisplay;
}
```

- `TopToolBarView`：删除全部输入代码，只留按钮 `onClick`；
- `OperatorDisplayView`：删除 ESC 绑定。

### 6.3 模态冻结交给 UIManager

```csharp
// IView / BaseView
bool BlocksGameplayInput { get; }   // 默认 false

// OperatorDisplayView 只需要一行
protected override bool BlocksGameplayInput => true;
```

`UIManager.OpenView / CloseView / CloseAllViews` 内部维护 `_modalStack` 并统一 `Push/PopGameplayBlock`：

- 多 UI 叠加天然支持；
- 重复 Show 不会重复冻结（Set 幂等）；
- `CloseAllViews` 统一解冻，不会「UI 全关还不能动」；
- 附带 `CloseTopModal()`，ESC 变成「关闭最上层弹窗」，以后加背包/商店不用改输入逻辑。

### 6.4 优先级

| 优先级 | 事项 |
|---|---|
| P0 | 修 CS0252；给消费者加「源变更」重绑；或直接做 6.1 门面 |
| P1 | 门面化 + UIInputRouter + UIManager 模态栈 |
| P2 | `EnsureInput` 缺失引用时 `Debug.LogWarning`；`Current` 收窄可见性 |

---

## 7. 经验总结

1. **抽象要看消费者，不看实现**。消费者只该知道「冲刺被按下」，不该知道 `InputAction`、`CallbackContext`、Player map。
2. **复杂度应该放在基础设施，不要摊给每个使用者**。让 5 个消费者各自处理换源绑定，等于把架构成本转嫁给业务代码。
3. **事件订阅必须有统一的生命周期和唯一的归属地**；`OnEnable/OnDisable` 成对写在多个类里，早晚会漏。
4. **UI 不要直接操作设备层**。模态是 UI 栈的属性，不是「禁用某个 action map」。
5. **静态网关可以存在，但必须有 Register/Unregister、变更通知、域重载重置**，否则就是隐藏的全局可变状态。

---

## 附录

### 涉及文件

```
Assets/Scripts/Player/IInputSource.cs
Assets/Scripts/Player/InputService.cs
Assets/Scripts/Player/PlayerInputSystem.cs
Assets/Scripts/Character/CharacterPlayerController.cs
Assets/Scripts/Team/TeamInputController.cs
Assets/Scripts/Module/UI/TopToolBar/TopToolBarView.cs
Assets/Scripts/Module/UI/OperatorDisplay/OperatorDisplayView.cs
Assets/Scripts/Player/ThirdPersonCamera.cs
```

### 相关提交

- `7bc1cc5` 重构输入系统，添加输入抽象（IInputSource + InputService）
