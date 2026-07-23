using UnityEngine;

namespace Endfield
{
    public class OperatorWalkingState : OperatorMovementState
    {
        public OperatorWalkingState(OperatorMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _reusableData.rotationTime = 0.1f;
            _reusableData.inputMult = 1f;
            _animator.CrossFadeInFixedTime("WalkStart", 0.14f);
            _animator.SetBool(AnimationID.HasInputID, true);
        }

        public override void Update()
        {
            base.Update();

            if (_operator.GetMovementInput() == Vector3.zero)
            {
                _movementStateMachine.ChangeState(_movementStateMachine.idlingState);
                return;
            }

            if (!_operator.GetShouldWalk())
                _movementStateMachine.ChangeState(_movementStateMachine.runningState);
        }
    }
}
