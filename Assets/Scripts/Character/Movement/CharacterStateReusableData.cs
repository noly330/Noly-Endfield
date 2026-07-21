namespace Endfield
{
    /// <summary>
    /// 所有移动状态的共享数据容器。只 new 一次，各状态通过引用读写同一份数据。
    /// </summary>
    public class CharacterStateReusableData
    {
        /// <summary>
        /// 输入倍率。Idle 设低值（≈0），降低摇杆微推时的误触；Walk/Run 设高值（≈1-3），
        /// 控制 Animator BlendTree 的 Movement 参数响应幅度。
        /// </summary>
        public float inputMult { get; set; }

        /// <summary>
        /// 行走/跑步切换标记。true=走路，false=跑步。由 Walk 按键翻转。
        /// </summary>
        public bool shouldWalk { get; set; }

        /// <summary>
        /// 是否可以闪避。进入 Dash 状态时设为 false，冷却结束后复位 true，
        /// 用来防止连按闪避。
        /// </summary>
        public bool canDash { get; set; } = true;

        /// <summary>
        /// 转向所需时间（秒），传给 Mathf.SmoothDampAngle。
        /// Idle 设大值（缓慢转向），Run 设小值（快速转向）。
        /// </summary>
        public float rotationTime { get; set; }

        /// <summary>
        /// 目标朝向角度。由 CharacterMovementState.CharacterRotation() 每帧根据
        /// 输入方向和相机朝向计算：Atan2(input.x, input.y)*Rad2Deg + camera.eulerAngles.y
        /// </summary>
        public float targetAngle { get; set; }
    }
}