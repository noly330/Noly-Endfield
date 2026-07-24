using UnityEngine;

namespace Endfield
{
    public class OperatorIdlingState : OperatorMovementStateBase
    {
        public OperatorIdlingState(OperatorMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _reusableData.rotationTime = 0.15f;
            _reusableData.inputMult = 0f;
            _animator.SetFloat(AnimationID.MovementID, 0);
            _animator.SetBool(AnimationID.HasInputID, false);
        }

        public override void Update()
        {
            base.Update();

            if (_operator.GetMovementInput() == Vector3.zero)
                return;

            if (_operator.GetShouldWalk())
                _movementStateMachine.ChangeState(_movementStateMachine.walkingState);
            else
                _movementStateMachine.ChangeState(_movementStateMachine.runningState);
        }
    }
}
