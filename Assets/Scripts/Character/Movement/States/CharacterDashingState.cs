using UnityEngine;
using Endfield.Tools;

namespace Endfield
{
    public class CharacterDashingState : CharacterMovementState
    {
        public CharacterDashingState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _reusableData.rotationTime = _movementData.dashData.rotationTime;
            _character.movementDriver.canDash = false;
            _animator.SetBool(AnimationID.HasInputID, true);
            TimerManager.Instance.GetOneTimer(_movementData.dashData.coldTime, _character.ResetDash);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnAnimationExitEvent()
        {
            if (_character.GetMovementInput() != Vector3.zero)
                _movementStateMachine.ChangeState(CharacterMovementStateType.Sprint);
            else
                _movementStateMachine.ChangeState(CharacterMovementStateType.Idle);
        }
    }
}
