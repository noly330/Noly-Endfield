using UnityEngine;
using Endfield.Tools;

namespace Endfield
{
    //TODO:现在没有走路的动画，这个先闲置在这里，以后添加走路的动画
    public class CharacterWalkingState : CharacterMovementState
    {
        public CharacterWalkingState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _reusableData.rotationTime = _movementData.walkData.rotationTime;
            _reusableData.inputMult = _movementData.walkData.inputMult;
            _animator.CrossFadeInFixedTime("WalkStart", 0.14f);
            _animator.SetBool(AnimationID.HasInputID, true);
        }

        public override void Update()
        {
            base.Update();

            if (_character.GetMovementInput() == Vector3.zero)
            {
                _movementStateMachine.ChangeState(CharacterMovementStateType.Idle);
                return;
            }

            if (!_character.GetShouldWalk())
                _movementStateMachine.ChangeState(CharacterMovementStateType.Run);
        }
    }
}
