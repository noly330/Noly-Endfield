using Endfield.Core;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : SingletonMono<PlayerInputSystem>
{
    [SerializeField] private PlayerInput inputActions;
    public Vector2 Move => EnsureInput().Player.MoveMent.ReadValue<Vector2>();
    public Vector2 Look => EnsureInput().Player.Look.ReadValue<Vector2>();
    public Vector2 Scroll => EnsureInput().Player.Scroll.ReadValue<Vector2>();
    public InputAction DashAction => EnsureInput().Player.Dash;
    public InputAction AttackAction => EnsureInput().Player.Attack;
    public InputAction SwitchAction => EnsureInput().Player.Switch;
    public InputAction Skill1 => EnsureInput().Player.Skill1;
    public InputAction Skill2 => EnsureInput().Player.Skill2;
    public InputAction Skill3 => EnsureInput().Player.Skill3;
    public InputAction Skill4 => EnsureInput().Player.Skill4;
    public InputAction LinkSkill => EnsureInput().Player.LinkSkill;

    protected override void Awake()
    {
        base.Awake();   // 设 _instance + DontDestroyOnLoad + 去重
        EnsureInput();
    }

    private PlayerInput EnsureInput()
    {
        if (inputActions == null)
            inputActions = new PlayerInput();
        return inputActions;
    }

    private void OnEnable()
    {
        EnsureInput().Player.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();
    }

}
