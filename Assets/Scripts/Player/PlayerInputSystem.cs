using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    private static PlayerInputSystem _instance;
    public static PlayerInputSystem Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<PlayerInputSystem>();
            return _instance;
        }
    }

    [SerializeField] private PlayerInput inputActions;
    public Vector2 Move => EnsureInput().Player.MoveMent.ReadValue<Vector2>();
    public Vector2 Look => EnsureInput().Player.Look.ReadValue<Vector2>();
    public Vector2 Scroll => EnsureInput().Player.Scroll.ReadValue<Vector2>();
    public InputAction DashAction => EnsureInput().Player.Dash;
    public InputAction AttackAction => EnsureInput().Player.Attack;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        EnsureInput();
        DontDestroyOnLoad(gameObject);
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
