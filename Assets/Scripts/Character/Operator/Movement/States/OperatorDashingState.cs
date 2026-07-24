using UnityEngine;

namespace Endfield
{
    public class OperatorDashingState : OperatorMovementStateBase
    {
        private float _cooldownTimer;

        public OperatorDashingState(OperatorMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _reusableData.rotationTime = _movementData.dashData.rotationTime;
            _operator.movementDriver.canDash = false;
            _cooldownTimer = _movementData.dashData.coldTime;
        }

        public override void Update()
        {
            base.Update();

            //TODO：计时器放在这里有严重问题，比如状态被打断后，计时器就不会更新
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0f)
                    _operator.ResetDash();
            }
        }

        public override void OnAnimationExitEvent()
        {
            if (_operator.GetMovementInput() != Vector3.zero)
                _movementStateMachine.ChangeState(_movementStateMachine.runningState);
            else
                _movementStateMachine.ChangeState(_movementStateMachine.idlingState);
        }
    }
}
