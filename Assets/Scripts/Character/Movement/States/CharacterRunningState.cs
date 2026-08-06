using Endfield.Tools;

namespace Endfield
{
    public class CharacterRunningState : CharacterMovementState
    {
        public CharacterRunningState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

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

            if (_character.GetMovementInput() == UnityEngine.Vector3.zero)
            {
                //这里就不用加缓冲了，就当转身动画了
                _movementStateMachine.ChangeState(CharacterMovementStateType.Idle);
                return;
            }

            if (_character.GetShouldWalk())
                _movementStateMachine.ChangeState(CharacterMovementStateType.Walk);
        }
    }
}
