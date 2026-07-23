using Global;
using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 干员的状态机宿主。主控干员与队友干员共用此移动系统。
    /// </summary>
    public class Operator : CharacterMovementControlBase
    {
        [SerializeField] private float _moveSpeed = 5f;

        public OperatorMovementStateMachine movementStateMachine { get; private set; }
        public OperatorMovementDriver movementDriver { get; private set; }

        public virtual Vector3 GetMovementInput()
        {
            return movementDriver.worldDirection;
        }

        public bool GetShouldWalk()
        {
            return movementDriver.shouldWalk;
        }

        /// <summary>消耗型：读取闪避触发标记后立刻复位。</summary>
        public bool GetShouldDodge()
        {
            bool result = movementDriver.shouldDodge;
            movementDriver.shouldDodge = false;
            return result;
        }

        /// <summary>
        /// 子类覆写此方法，每帧向 movementDriver 写入数据。
        /// OperatorInputController 从玩家输入写入；队友 AI/行为树直接写入。
        /// </summary>
        public virtual void UpdateMovementDriver() { }

        /// <summary>闪避冷却结束回调，由 OperatorDodgingState 调用。</summary>
        public void ResetDodge()
        {
            movementDriver.canDodge = true;
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

            Vector3 input = GetMovementInput();
            if (input != Vector3.zero)
            {
                _characterController.Move(input * _moveSpeed * Time.deltaTime);
            }
        }
    }
}
