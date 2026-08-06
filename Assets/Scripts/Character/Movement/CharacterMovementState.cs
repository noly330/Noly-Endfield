using UnityEngine;
using Endfield.Tools;

namespace Endfield
{
    public abstract class CharacterMovementState : IState
    {
        protected CharacterMovementStateMachine _movementStateMachine { get; }
        protected Character _character { get; }
        protected Animator _animator { get; }
        protected CharacterMovementReusableData _reusableData { get; }
        protected CharacterMovementData _movementData { get; }
        protected bool _canReturnBack = true;

        public CharacterMovementState(CharacterMovementStateMachine stateMachine)
        {
            _movementStateMachine = stateMachine;
            _character = stateMachine.character;
            _animator = _character.animator;
            _reusableData = stateMachine.reusableData;
            _movementData = _character.MovementData;
        }

        public virtual void Enter()
        {
        }

        public virtual void Exit() { }

        public virtual void HandInput()
        {
            Vector3 moveDir = _character.GetMovementInput();
            _animator.SetFloat(AnimationID.MovementID,
                moveDir.magnitude * _reusableData.inputMult, 0.35f, Time.deltaTime);
        }

        public virtual void Update()
        {
            CharacterRotation(_character.GetMovementInput());
        }

        protected void CharacterRotation(Vector3 movementDirection)
        {
            if (movementDirection == Vector3.zero) return;
            if (!_canReturnBack) return;

            _reusableData.targetAngle = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg;

            float angle = Mathf.SmoothDampAngle(
                _character.transform.eulerAngles.y,
                _reusableData.targetAngle,
                ref _reusableData.currentVelocity,
                _reusableData.rotationTime);

            _character.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        public virtual void OnAnimationTranslateEvent(IState state)
        {
            _movementStateMachine.ChangeState(state);
        }

        public virtual void OnAnimationExitEvent() { }
    }
}
