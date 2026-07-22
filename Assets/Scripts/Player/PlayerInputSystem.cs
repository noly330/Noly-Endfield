using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    public static PlayerInputSystem Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActions;

    private InputAction moveAction;
    public Vector2 Move { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        moveAction = inputActions.FindAction("MoveMent");
    }

    private void OnEnable()
    {
        moveAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
    }

    private void Update()
    {
        if (moveAction != null)
            Move = moveAction.ReadValue<Vector2>();
    }
}
