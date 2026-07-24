using Global;
using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 干员的状态机宿主。主控干员与队友干员共用此移动系统。
    /// </summary>
    public class Operator : CharacterMovementControlBase
    {
        /// <summary>
        /// 当前干员的移动状态机。
        /// </summary>
        public OperatorMovementStateMachine movementStateMachine { get; private set; }
        /// <summary>
        /// 当前干员的移动驱动器。
        /// 从外部(玩家输入或者ai行为树）读取输入，向移动状态机写入数据。
        /// </summary>
        public OperatorMovementDriver movementDriver { get; private set; }
        public OperatorSO operatorSO;


        public virtual Vector3 GetMovementInput()
        {
            return movementDriver.worldDirection;
        }

        public bool GetShouldWalk()
        {
            return movementDriver.shouldWalk;
        }

        /// <summary>消耗型：读取冲刺触发标记后立刻复位。</summary>
        public bool GetShouldDash()
        {
            bool result = movementDriver.shouldDash;
            movementDriver.shouldDash = false;
            return result;
        }

        /// <summary>
        /// 子类覆写此方法，每帧向 movementDriver 写入数据。
        /// OperatorInputController 从玩家输入写入；队友 AI/行为树直接写入。
        /// </summary>
        public virtual void UpdateMovementDriver() { }

        /// <summary>冲刺冷却结束回调，由 OperatorDashingState 调用。</summary>
        public void ResetDash()
        {
            movementDriver.canDash = true;
        }

        protected override void Awake()
        {
            base.Awake();
            movementDriver = new OperatorMovementDriver();
            movementStateMachine = new OperatorMovementStateMachine(this);
        }

        protected override void Start()
        {
            base.Start();
            movementStateMachine.ChangeState(movementStateMachine.idlingState);
        }

        protected override void Update()
        {
            base.Update();
            UpdateMovementDriver();
            movementStateMachine.HandInput();
            movementStateMachine.Update();
        }

        public void OnAnimationTranslate(OnEnterAnimationState state)
        {
            switch (state)
            {
                case OnEnterAnimationState.Dash:
                    movementStateMachine.ChangeState(movementStateMachine.dashingState);
                    break;
            }
        }
    }
}
