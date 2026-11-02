using System;
using Endfield.Core;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 本机硬件输入源：包装生成的 PlayerInput actions，把 InputAction.performed 转发为
/// IInputSource 的 C# 事件。Awake 时注册到 InputService.Current。
/// </summary>
public class PlayerInputSystem : SingletonMono<PlayerInputSystem>, IInputSource
{
    [SerializeField] private PlayerInput inputActions;

    #region IInputSource 轮询型
    public Vector2 Move => EnsureInput().Player.MoveMent.ReadValue<Vector2>();
    public Vector2 Look => EnsureInput().Player.Look.ReadValue<Vector2>();
    public Vector2 Scroll => EnsureInput().Player.Scroll.ReadValue<Vector2>();
    #endregion

    #region IInputSource 事件
    public event Action OnDash;
    public event Action OnAttack;
    public event Action OnSwitch;
    public event Action<int> OnSkill;
    public event Action OnLinkSkill;
    public event Action OnCloseView;
    public event Action OnOpenOperatorDisplay;
    #endregion

    protected override void Awake()
    {
        base.Awake();   // 设 _instance + DontDestroyOnLoad + 去重
        if (Instance != this) return;   // 重复实例已被销毁流程接管，不抢输入源注册
        EnsureInput();
        SubscribeActions();
        InputService.Current = this;
    }

    private void OnDestroy()
    {
        UnsubscribeActions();
        if (InputService.Current == this)
            InputService.Current = null;
    }

    /// <summary>Awake 订阅一次（map 的 Enable/Disable 控制事件是否触发，不影响订阅关系）。</summary>
    private void SubscribeActions()
    {
        var player = EnsureInput().Player;
        player.Dash.performed += DashPerformed;
        player.Attack.performed += AttackPerformed;
        player.Switch.performed += SwitchPerformed;
        player.Skill1.performed += Skill1Performed;
        player.Skill2.performed += Skill2Performed;
        player.Skill3.performed += Skill3Performed;
        player.Skill4.performed += Skill4Performed;
        player.LinkSkill.performed += LinkSkillPerformed;

        var ui = EnsureInput().UI;
        ui.CloseView.performed += CloseViewPerformed;
        ui.OperatorDisplay.performed += OpenOperatorDisplayPerformed;
    }

    private void UnsubscribeActions()
    {
        if (inputActions == null) return;

        var player = inputActions.Player;
        player.Dash.performed -= DashPerformed;
        player.Attack.performed -= AttackPerformed;
        player.Switch.performed -= SwitchPerformed;
        player.Skill1.performed -= Skill1Performed;
        player.Skill2.performed -= Skill2Performed;
        player.Skill3.performed -= Skill3Performed;
        player.Skill4.performed -= Skill4Performed;
        player.LinkSkill.performed -= LinkSkillPerformed;

        var ui = inputActions.UI;
        ui.CloseView.performed -= CloseViewPerformed;
        ui.OperatorDisplay.performed -= OpenOperatorDisplayPerformed;
    }

    private void DashPerformed(InputAction.CallbackContext _) => OnDash?.Invoke();
    private void AttackPerformed(InputAction.CallbackContext _) => OnAttack?.Invoke();
    private void SwitchPerformed(InputAction.CallbackContext _) => OnSwitch?.Invoke();
    private void Skill1Performed(InputAction.CallbackContext _) => OnSkill?.Invoke(1);
    private void Skill2Performed(InputAction.CallbackContext _) => OnSkill?.Invoke(2);
    private void Skill3Performed(InputAction.CallbackContext _) => OnSkill?.Invoke(3);
    private void Skill4Performed(InputAction.CallbackContext _) => OnSkill?.Invoke(4);
    private void LinkSkillPerformed(InputAction.CallbackContext _) => OnLinkSkill?.Invoke();
    private void CloseViewPerformed(InputAction.CallbackContext _) => OnCloseView?.Invoke();
    private void OpenOperatorDisplayPerformed(InputAction.CallbackContext _) => OnOpenOperatorDisplay?.Invoke();

    private PlayerInput EnsureInput()
    {
        if (inputActions == null)
            inputActions = new PlayerInput();
        return inputActions;
    }

    private void OnEnable()
    {
        EnsureInput().Player.Enable();
        EnsureInput().UI.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();
        inputActions?.UI.Disable();
    }

    /// <summary>
    /// 冻结/解冻角色输入（UI 打开时禁用 Player map，UI 的 C/ESC 在 UI map 不受影响）。
    /// </summary>
    public void SetPlayerInputEnabled(bool enabled)
    {
        if (enabled) EnsureInput().Player.Enable();
        else         EnsureInput().Player.Disable();
    }

}
