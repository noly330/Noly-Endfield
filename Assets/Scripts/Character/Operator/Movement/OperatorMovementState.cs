using UnityEngine;

namespace Endfield
{
    public class OperatorMovementState : IState
    {
        protected OperatorMovementStateMachine _movementStateMachine { get; }
        protected Operator _operator { get; }
        protected Animator _animator { get; }
        protected OperatorStateReusableData _reusableData { get; }
        protected OperatorMovementData _movementData { get; }

        public OperatorMovementState(OperatorMovementStateMachine stateMachine)
        {
            _movementStateMachine = stateMachine;
            _operator = stateMachine.operatorCharacter;
            _animator = _operator.animator;
            _reusableData = stateMachine.reusableData;
            _movementData = _operator.operatorSO.movementData;
        }

        public virtual void Enter()
        {
            Debug.Log(_operator.name + " 状态为 " + GetType().Name);
        }

        public virtual void Exit() { }

        public virtual void HandInput()
        {
            Vector3 moveDir = _operator.GetMovementInput();
            _animator.SetFloat(AnimationID.MovementID,
                moveDir.magnitude * _reusableData.inputMult, 0.35f, Time.deltaTime);
        }

        public virtual void Update()
        {
            CharacterRotation(_operator.GetMovementInput());
        }

        protected void CharacterRotation(Vector3 movementDirection)
        {
            if (movementDirection == Vector3.zero) return;

            _reusableData.targetAngle = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg;

            float angle = Mathf.SmoothDampAngle(
                _operator.transform.eulerAngles.y,
                _reusableData.targetAngle,
                ref _reusableData.currentVelocity,
                _reusableData.rotationTime);

            _operator.transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        public virtual void OnAnimationTranslateEvent(IState state)
        {
            _movementStateMachine.ChangeState(state);
        }

        public virtual void OnAnimationExitEvent() { }
    }
}
