# MVVM 改造计划

> 2026-11-16 | `refactor/mvvm-ui` | 草稿

## 结论

UI 层引入 ViewModel：**View 只绑定显示、ViewModel 持状态与命令、Model 只留业务数据**。
分 5 步增量改造，每步可编译可运行；先用 `OperatorDisplay` 打通模式，再推广到其余界面。
战斗 / 角色逻辑（`Character/`、FSM）**不纳入**本次改造。

## 问题

| 位置 | 问题 |
|---|---|
| `Core/UI/BaseView.cs` | 组件靠字符串路径现场取，取完直接改 `text`；无绑定、无退订管理 |
| `OperatorDisplay/OperatorDisplayView.cs` | 一个类同时做取组件、读 SO、算显示值、处理输入、播动画 |
| `OperatorDisplay/OperatorDisplayModel.cs` | 名叫 Model，实为 UI 状态 + 查询，缺少 VM 层 |
| `Core/GameLifetimeScope.cs:23` | 所有 `BaseModel` 注册为 Singleton → `currentID` 全局共享，关界面不清状态 |
| `TopToolBar/TopToolBarView.cs` | 7 个按钮逐个 `AddListener`，加一个入口就要改 View |
| `CombatHUD/LinkQueueWidget.cs` | Widget 直接读 `TeamManager.Instance`，View 与业务耦合 |
| `Tools/BindableProperty.cs` | 已实现但零引用，缺订阅句柄 / 静默赋值能力 |

## 方案

| 层 | 落地物 | 生命周期 |
|---|---|---|
| Model | 现有 `UserDataService` / `TeamManager` / `OperatorCatalog` / `LinkSystem`，**不含 UI 状态** | Singleton |
| ViewModel | 新增 `XxxViewModel : BaseViewModel`，暴露 `BindableProperty<T>` + Command | Transient |
| View | 现有 `XxxView : BaseView<TVm>`，只缓存组件、订阅、转发命令 | Transient |

基础设施（新增 4 个小文件，不改现有契约）：

```csharp
public abstract class BaseViewModel : IDisposable
{
    protected readonly CompositeDisposable Bag = new();
    public virtual void Dispose() => Bag.Dispose();
}
public abstract class BaseView<TVm> : BaseView where TVm : BaseViewModel
{
    [Inject] protected TVm ViewModel { get; set; }
}
```

- `BindableProperty` 补 `SetWithoutNotify` + `Subscribe(handler, invokeNow)` 返回 `IDisposable`。
- `GameLifetimeScope` 增加 `BaseViewModel → Transient` 扫描；`OperatorDisplayModel` 从 Model 名单移出。

约束：

- View 内禁止出现 `XxxService.Instance` / `attr.maxHp` 这类业务取值。
- ViewModel 不引用 `UnityEngine.UI`，可脱离场景单测。
- 不引入 UniRx / UnityWeld，现有 `BindableProperty` + 20 行订阅管理即够。

## 任务

| # | 步骤 | 涉及文件 | 验收 |
|---|---|---|---|
| 1 | 基础设施 | `Core/MVVM/BaseViewModel.cs`、`Core/Reactive/{BindableProperty,RelayCommand,CompositeDisposable}.cs`、`Core/UI/BaseView.cs`、`Core/GameLifetimeScope.cs` | 编译通过；现有界面行为无变化 |
| 2 | 改造 OperatorDisplay | `OperatorDisplay/{View,ViewModel}.cs`、删除 `OperatorDisplayModel` | 选人 / 名字 / 三维 / 星级 / 预览与改造前一致；重开界面回到默认选中 |
| 3 | 改造 TopToolBar | `TopToolBar/{TopToolBarView,TopToolBarViewModel}.cs` | 7 个入口 + C 键走同一 Command；View 内无业务判断 |
| 4 | 改造 CombatHUD Widget | `CombatHUD/CombatHUDView.cs`、`Link{Banner,Queue}Widget.cs` + 两个 ViewModel | 连携横幅 / 队列表现不变；Widget 不直接访问 `TeamManager` |
| 5 | 收尾 | `Docs/README.md`、本文件 | 文档索引更新；遗留项登记 |

执行清单：

- [ ] 1. 提交基础设施，`BaseView` 增加订阅句柄列表，`Destroy` 时统一 Dispose
- [ ] 2. 新增 `IOperatorRepository` 包装 `OperatorCatalog`，VM 依赖接口而非静态类
- [ ] 3. `OperatorDisplay` 拆分为 View + VM，删除旧 Model 与 Singleton 注册
- [ ] 4. `TopToolBar` 改为 Command 驱动，`InputService.OnOpenOperatorDisplay` 接同一 Command
- [ ] 5. `LinkQueueWidget` / `LinkBannerWidget` 改为订阅 VM 属性，动画逻辑留在 View
- [ ] 6. 补 1 个 EditMode 测试：`OperatorDisplayViewModel.Select()` 后属性值正确

## 验证

- 每步完成后手动回归：主界面 → 干员展示 → 关闭重开 → 战斗中连携触发。
- 第 6 项测试跑通即证明 VM 可脱离 Unity 场景运行。
- 检查 `GameLifetimeScope` 容器日志，确认 ViewModel 为 Transient、Model 为 Singleton。

## 遗留

- `MainView` 仍是空壳，等实际 UI 制作时直接按本模式落地。
- 未做 UI 自动绑定（`[SerializeField]` 反射绑定），当前仍用路径 `GetComponent`。
- `EventCenter` 事件总线保留，但订阅方从 View 迁到 ViewModel。

## 相关文件

- `Assets/Scripts/Core/UI/BaseView.cs`、`BaseWidget.cs`、`UIManager.cs`
- `Assets/Scripts/Core/Model/{IModel,BaseModel,ModelManager}.cs`
- `Assets/Scripts/Core/GameLifetimeScope.cs`
- `Assets/Scripts/Core/Reactive/BindableProperty.cs`（命名空间约定见 [namespace-convention.md](../Notes/Unity/namespace-convention.md)）
- `Assets/Scripts/Module/UI/OperatorDisplay/`
- `Assets/Scripts/Module/UI/TopToolBar/`、`CombatHUD/`
