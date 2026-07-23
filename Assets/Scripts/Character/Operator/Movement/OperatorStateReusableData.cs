namespace Endfield
{
    /// <summary>
    /// 所有移动状态的共享数据容器。只 new 一次，各状态通过引用读写同一份数据。
    /// </summary>
    public class OperatorStateReusableData
    {
        /// <summary>
        /// 输入倍率。Idle 设低值（≈0），降低摇杆微推时的误触；Walk/Run 设高值（≈1-3），
        /// 控制 Animator BlendTree 的 Movement 参数响应幅度。
        /// </summary>
        public float inputMult { get; set; }

        /// <summary>
        /// 转向所需时间（秒），传给 Mathf.SmoothDampAngle。
        /// Idle 设大值（缓慢转向），Run 设小值（快速转向）。
        /// </summary>
        public float rotationTime { get; set; }

        /// <summary>
        /// 目标朝向角度。由 OperatorMovementState.CharacterRotation() 每帧根据
        /// 输入方向计算：Atan2(input.x, input.z)*Rad2Deg
        /// </summary>
        public float targetAngle { get; set; }

        /// <summary>
        /// SmoothDampAngle 的速度缓存。移到共享数据后状态切换不再归零，旋转不会抖动。
        /// </summary>
        public float currentVelocity;
    }
}
