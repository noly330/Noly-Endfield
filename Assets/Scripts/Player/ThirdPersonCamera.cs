using UnityEngine;

/// <summary>
/// 第三人称相机控制器。基于 Cinemachine VirtualCamera，用 SmoothDampAngle 驱动旋转，
/// 支持 FOV 缩放。相机位置、距离、碰撞避让由 Cinemachine Body 组件自动处理。
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("目标")]
    [SerializeField] private Transform _cameraTarget;

    [Header("灵敏度")]
    [SerializeField] private float _mouseSensitivity = 0.3f;

    /// <summary>/// 垂直灵敏度倍率（相对水平灵敏度的比例）。/// </summary>
    [SerializeField] private float _verticalSensitivity = 0.4f;

    /// <summary>滚轮缩放灵敏度</summary>
    [SerializeField] private float _scrollSensitivity = 10f;

    [Header("限制")]
    [SerializeField] private float _topClamp = 70f;
    [SerializeField] private float _bottomClamp = -30f;
    [SerializeField] private float _fovMin = 30f;
    [SerializeField] private float _fovMax = 70f;

    [Header("平滑")]
    /// <summary>
    /// 旋转平滑时间（秒）。值越大旋转越"软"，越小响应越快。
    /// 传给 SmoothDampAngle 的第三个参数。
    /// </summary>
    [SerializeField] private float _rotationSmoothTime = 0.12f;

    private Cinemachine.CinemachineVirtualCamera _virtualCamera;

    /// <summary>SmoothDampAngle 的水平角速度缓存</summary>
    private float _horizontalVelocity;

    /// <summary>SmoothDampAngle 的垂直角速度缓存</summary>
    private float _verticalVelocity;

    /// <summary>当前水平旋转角度</summary>
    private float _horizontalAngle;

    /// <summary>当前垂直旋转角度</summary>
    private float _verticalAngle;

    /// <summary>目标 FOV，每帧 Lerp 逼近</summary>
    private float _targetFov;

    /// <summary>输入死区，小于此值的鼠标/滚轮输入视为噪声</summary>
    private const float _threshold = 0.01f;

    private void Awake()
    {
        _virtualCamera = GetComponent<Cinemachine.CinemachineVirtualCamera>();
    }

    private void Start()
    {
        _horizontalAngle = _cameraTarget.eulerAngles.y;
        _verticalAngle   = _cameraTarget.eulerAngles.x;
        _targetFov       = _virtualCamera.m_Lens.FieldOfView;

        Cursor.visible   = false;  // 隐藏鼠标光标
        Cursor.lockState = CursorLockMode.Locked;  // 锁定鼠标光标
    }

    /// <summary>
    /// 在 LateUpdate 中更新相机，确保在角色 Animator 之后执行，避免抖动。
    /// 只控制 cameraTarget 的旋转，位置/距离/碰撞由 Cinemachine Body 自动处理。
    /// </summary>
    private void LateUpdate()
    {
        Vector2 look   = PlayerInputSystem.Instance.Look;
        Vector2 scroll = PlayerInputSystem.Instance.Scroll;

        //旋转
        float targetHorizontal = _horizontalAngle + look.x * _mouseSensitivity;
        float targetVertical   = _verticalAngle   - look.y * _mouseSensitivity * _verticalSensitivity;

        targetVertical = ClampAngle(targetVertical, _bottomClamp, _topClamp);  //把垂直旋转角度限制在 _bottomClamp 到 _topClamp 之间

        _horizontalAngle = Mathf.SmoothDampAngle(
            _horizontalAngle, targetHorizontal, ref _horizontalVelocity, _rotationSmoothTime);
        _verticalAngle = Mathf.SmoothDampAngle(
            _verticalAngle,   targetVertical,   ref _verticalVelocity,   _rotationSmoothTime);

        _cameraTarget.rotation = Quaternion.Euler(_verticalAngle, _horizontalAngle, 0f);

        // ---- FOV 缩放 ----
        HandleFovZoom(scroll.y);
    }

    /// <summary>
    /// 处理滚轮 FOV 缩放。目标 FOV 受 _fovMin / _fovMax 限制，
    /// 当前 FOV 每帧 Lerp 逼近目标值。
    /// </summary>
    private void HandleFovZoom(float scroll)
    {
        if (_virtualCamera == null) return;

        _targetFov -= scroll * _scrollSensitivity * Time.unscaledDeltaTime;
        _targetFov  = Mathf.Clamp(_targetFov, _fovMin, _fovMax);

        float current = _virtualCamera.m_Lens.FieldOfView;
        if (Mathf.Abs(current - _targetFov) > _threshold)
        {
            _virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(
                current, _targetFov, 7f * Time.unscaledDeltaTime);
        }
    }
    /// <summary>
    /// 角度限制，正确处理 ±360 之外的缠绕值。
    /// </summary>
    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle >  360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
