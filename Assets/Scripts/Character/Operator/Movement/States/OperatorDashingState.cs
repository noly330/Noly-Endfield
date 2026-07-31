using UnityEngine;
using Endfield.Tools;
namespace Endfield
{
    public class OperatorDashingState : OperatorMovementState
    {


        public OperatorDashingState(OperatorMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _reusableData.rotationTime = _movementData.dashData.rotationTime;
            _operator.movementDriver.canDash = false;
            _animator.SetBool(AnimationID.HasInputID,true);
            TimerManager.Instance.GetOneTimer(_movementData.dashData.coldTime, _operator.ResetDash);
        }

        public override void Update()
        {
            base.Update();

        }

        public override void OnAnimationExitEvent()
        {
            if (_operator.GetMovementInput() != Vector3.zero)
                _movementStateMachine.ChangeState(_movementStateMachine.sprintingState);
            else
                _movementStateMachine.ChangeState(_movementStateMachine.idlingState);
        }
    }
}
