using System.Collections;
using System.Collections.Generic;
using Endfield.Tools;
using UnityEngine;

namespace Endfield
{
    public class CharacterReturnRunState : CharacterMovementState
    {
        public CharacterReturnRunState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            //进入到这个状态之后关闭TurnBack，因为只触发一次，注意，这个代码是在OnAnimationTranslate中触发的
            _animator.SetBool(AnimationID.TurnBackID, false);
            _animator.SetBool(AnimationID.HasInputID, true);
            _reusableData.inputMult = _movementData.returnRunData.inputMult;
            _reusableData.rotationTime = _movementData.returnRunData.rotationTime;
            _canReturnBack = false;
        }

        public override void Exit()
        {
            base.Exit();
            //保险起见，再加个关闭的方式
            _animator.SetBool(AnimationID.TurnBackID, false);
            _canReturnBack = true;
        }

        public override void OnAnimationExitEvent()
        {
            _canReturnBack = true;
            if (_character.movementDriver.worldDirection == Vector3.zero)
            {
                _movementStateMachine.ChangeState(CharacterMovementStateType.Idle);
            }
            else
            {
                _movementStateMachine.ChangeState(CharacterMovementStateType.Sprint);
            }
        }
    }
}
