using Endfield;
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

    /// <summary>连携镜头：施法干员 + 剩余时长 + 连携前的水平角 + 定格目标（开始瞬间固定）</summary>
    private Transform _linkTarget;
    private float _linkTimer;
    private float _preLinkHorizontalAngle;
    private float _linkTargetYaw;
    private Vector3 _linkTargetOffset;

    /// <summary>连携镜头转向速度（度/秒）与位移速度（单位/秒）；转向比例（0~1，越大越靠近干员正前方）</summary>
    private const float _linkRotateSpeed = 500f;
    private const float _linkMoveSpeed = 30f;
    private const float _linkTurnFraction = 0.35f;   // 只转干员方向的 35%，轻微转向不甩镜

    /// <summary>世界空间位移扩展（连携镜头平移用）</summary>
    private CameraPositionOffset _positionOffset;

    /// <summary>输入死区，小于此值的鼠标/滚轮输入视为噪声</summary>
    private const float _threshold = 0.01f;

    private void Awake()
    {
        _virtualCamera = GetComponent<Cinemachine.CinemachineVirtualCamera>();
        _positionOffset = GetComponent<CameraPositionOffset>() ?? gameObject.AddComponent<CameraPositionOffset>();
    }

    private void Start()
    {
        
        // _horizontalAngle = _cameraTarget.eulerAngles.y;
        // _verticalAngle   = _cameraTarget.eulerAngles.x;
        // _targetFov       = _virtualCamera.m_Lens.FieldOfView;

        Cursor.visible   = false;  // 隐藏鼠标光标
        Cursor.lockState = CursorLockMode.Locked;  // 锁定鼠标光标
    }

    /// <summary>
    /// 切人时统一重指相机：同时更新旋转参考 + Cinemachine 位置跟随。
    /// TeamManager 只调这一个入口，不需要知道 Cinemachine 内部。
    /// </summary>
    public void FollowTarget(Transform newTarget)
    {
        _cameraTarget = newTarget;
        if (_virtualCamera != null)
            _virtualCamera.Follow = newTarget;
    }

    /// <summary>
    /// 直接跟随目标（启动/切人）：找角色上的 CameraBasePoint 锚点并 FollowTarget。
    /// 连携技镜头走 LinkFocusOn（单独方法）。
    /// </summary>
    public void FocusOn(Transform target)
    {
        if (target == null) return;
        FollowTarget(FindCameraBasePoint(target));
    }

    /// <summary>
    /// 连携技镜头：开始瞬间定格目标方向/偏移，镜头快速转向固定值后停住（不追移动的目标），
    /// 时缓窗口结束立刻回到主控原来的相机位置。由 TeamManager 在连携打出时调用。
    /// </summary>
    public void LinkFocusOn(Transform target, float duration)
    {
        _linkTarget = target;
        _linkTimer = duration;
        _preLinkHorizontalAngle = _horizontalAngle;

        // 定格：只取开始瞬间干员的方向/偏移，之后镜头转向这个固定值并停住
        if (target != null && _cameraTarget != null)
        {
            Vector3 dir = target.position - _cameraTarget.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.0001f)
            {
                float casterYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                // 轻微转向：只朝干员方向转一部分，不平移太近（保持距离）
                _linkTargetYaw = _horizontalAngle + Mathf.DeltaAngle(_horizontalAngle, casterYaw) * _linkTurnFraction;
                _linkTargetOffset = dir.normalized * Mathf.Min(dir.magnitude * 0.5f, 2f);
            }
            else
            {
                _linkTargetYaw = _horizontalAngle;
                _linkTargetOffset = Vector3.zero;
            }
        }
    }

    /// <summary>找角色上的相机锚点（CameraBasePoint），没有就用角色根。</summary>
    private static Transform FindCameraBasePoint(Transform root)
    {
        foreach (Transform child in root)
        {
            if (child.name == "CameraBasePoint")
                return child;
        }
        return root;
    }

    /// <summary>
    /// 在 LateUpdate 中更新相机，确保在角色 Animator 之后执行，避免抖动。
    /// </summary>
    private void LateUpdate()
    {
        if (_cameraTarget == null) return;   // 防御：目标被销毁/未赋值时跳过
        Vector2 look   = PlayerInputSystem.Instance.Look;
        Vector2 scroll = PlayerInputSystem.Instance.Scroll;

        //旋转
        float targetHorizontal = _horizontalAngle + look.x * _mouseSensitivity;
        float targetVertical   = _verticalAngle   - look.y * _mouseSensitivity * _verticalSensitivity;

        // 连携镜头：快速转向定格目标（~0.1s 到位并固定），窗口结束立刻回主控原位
        if (_linkTimer > 0f)
        {
            _linkTimer -= Time.unscaledDeltaTime;
            _horizontalAngle = Mathf.MoveTowardsAngle(_horizontalAngle, _linkTargetYaw, _linkRotateSpeed * Time.unscaledDeltaTime);
            targetHorizontal = _horizontalAngle;   // 已直接驱动角度，防止 SmoothDamp 往回拉
            _positionOffset.Offset = Vector3.MoveTowards(_positionOffset.Offset, _linkTargetOffset, _linkMoveSpeed * Time.unscaledDeltaTime);
        }
        else if (_linkTarget != null)
        {
            // 时缓结束：立刻回到主控原来的相机位置（清偏移 + 还原角度）
            _positionOffset.Offset = Vector3.zero;
            _horizontalAngle = _preLinkHorizontalAngle;
            targetHorizontal = _horizontalAngle;
            _linkTarget = null;
        }

        targetVertical = ClampAngle(targetVertical, _bottomClamp, _topClamp);  //把垂直旋转角度限制在 _bottomClamp 到 _topClamp 之间

        _horizontalAngle = Mathf.SmoothDampAngle(
            _horizontalAngle, targetHorizontal, ref _horizontalVelocity, _rotationSmoothTime);
        _verticalAngle = Mathf.SmoothDampAngle(
            _verticalAngle,   targetVertical,   ref _verticalVelocity,   _rotationSmoothTime);

        _cameraTarget.rotation = Quaternion.Euler(_verticalAngle, _horizontalAngle, 0f);

        // ---- FOV 缩放 ----
        HandleFovZoom(scroll.y);
    }

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
    /// 角度限制，处理 ±360 之外的缠绕值。
    /// </summary>
    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle >  360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
