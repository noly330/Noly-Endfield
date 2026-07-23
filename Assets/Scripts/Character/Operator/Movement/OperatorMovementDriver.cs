using UnityEngine;

namespace Endfield
{
    public class OperatorMovementDriver
    {
        /// <summary>世界空间移动方向，Vector3.zero=停</summary>
        public Vector3 worldDirection;

        /// <summary>true=走，false=跑</summary>
        public bool shouldWalk;

        /// <summary>闪避触发，读一次后由 Operator.ResetDodge() 复位</summary>
        public bool shouldDodge;

        /// <summary>闪避冷却标记，false=冷却中不能闪避</summary>
        public bool canDodge = true;
    }
}
