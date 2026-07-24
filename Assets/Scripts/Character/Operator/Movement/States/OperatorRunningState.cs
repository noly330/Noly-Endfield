namespace Endfield
{
    public class OperatorRunningState : OperatorMovementStateBase
    {
        public OperatorRunningState(OperatorMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _reusableData.rotationTime = _movementData.runData.rotationTime;
            _reusableData.inputMult = _movementData.runData.inputMult;
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
