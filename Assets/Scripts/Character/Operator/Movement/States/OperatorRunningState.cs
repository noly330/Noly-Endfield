namespace Endfield
{
    public class OperatorRunningState : OperatorMovementState
    {
        public OperatorRunningState(OperatorMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _reusableData.rotationTime = 0.05f;
            _reusableData.inputMult = 2f;
            _animator.CrossFadeInFixedTime("WalkStart", 0.14f);
            _animator.SetBool(AnimationID.HasInputID, true);
        }

        public override void Update()
        {
            base.Update();

            if (_operator.GetMovementInput() == UnityEngine.Vector3.zero)
            {
                _movementStateMachine.ChangeState(_movementStateMachine.idlingState);
                return;
            }

            if (_operator.GetShouldWalk())
                _movementStateMachine.ChangeState(_movementStateMachine.walkingState);
        }
    }
}
