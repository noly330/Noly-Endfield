using UnityEngine;
using Endfield.Tools;
namespace Endfield
{
    //TODO:现在没有走路的动画，这个先闲置在这里，以后添加走路的动画
    public class OperatorWalkingState : OperatorMovementState
    {
        public OperatorWalkingState(OperatorMovementStateMachine stateMachine) : base(stateMachine) { }

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
