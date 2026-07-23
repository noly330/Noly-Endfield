using UnityEngine;

namespace Endfield
{
    public class OperatorMovementDriver
    {
        /// <summary>世界空间移动方向，Vector3.zero=停</summary>
        public Vector3 worldDirection;

        /// <summary>true=走，false=跑</summary>
        public bool shouldWalk;

        /// <summary>冲刺触发，读一次后由 Operator.ResetDash() 复位</summary>
        public bool shouldDash;

        /// <summary>冲刺冷却标记，false=冷却中不能冲刺</summary>
        public bool canDash = true;
    }
}
