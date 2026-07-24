using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    public static PlayerInputSystem Instance { get; private set; }

    [SerializeField] private PlayerInput inputActions;
    public Vector2 Move => inputActions.Player.MoveMent.ReadValue<Vector2>();
    public Vector2 Look => inputActions.Player.Look.ReadValue<Vector2>();
    public Vector2 Scroll => inputActions.Player.Scroll.ReadValue<Vector2>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if(inputActions == null)
            inputActions = new PlayerInput();
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

}
