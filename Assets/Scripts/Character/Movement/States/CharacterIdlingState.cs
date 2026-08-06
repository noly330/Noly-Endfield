using UnityEngine;
using Endfield.Tools;

namespace Endfield
{
    public class CharacterIdlingState : CharacterMovementState
    {
        public CharacterIdlingState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

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

            if (_character.GetMovementInput() == Vector3.zero)
                return;

            if (_character.GetShouldWalk())
                _movementStateMachine.ChangeState(CharacterMovementStateType.Walk);
            else
                _movementStateMachine.ChangeState(CharacterMovementStateType.Run);
        }
    }
}
