using System.Collections;
using System.Collections.Generic;
using Endfield.Tools;
using UnityEngine;

namespace Endfield
{
    public class OperatorReturnRunState : OperatorMovementStateBase
    {
        public OperatorReturnRunState(OperatorMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            //进入到这个状态之后关闭TurenBack，因为只触发一次，注意，这个代码是在OnAnimationTranslate中触发的
            _animator.SetBool(AnimationID.TurnBackID,false);
            _reusableData.inputMult = _movementData.returnRunData.inputMult;
            _reusableData.rotationTime = _movementData.returnRunData.rotationTime;
            _canReturnBack = false;
        }

        public override void Exit()
        {
            base.Exit();
            //保险起见，再加个关闭的方法
            _animator.SetBool(AnimationID.TurnBackID,false);
            _canReturnBack = true;
        }

        public override void OnAnimationExitEvent()
        {
            _canReturnBack = true;
            if(_operator.movementDriver.worldDirection == Vector3.zero)
            {
                _movementStateMachine.ChangeState(_movementStateMachine.idlingState);
            }
            else
            {
                _movementStateMachine.ChangeState(_movementStateMachine.sprintingState);
            }
        }
    }
}