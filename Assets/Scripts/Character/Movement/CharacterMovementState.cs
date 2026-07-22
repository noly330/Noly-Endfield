using UnityEngine;

namespace Endfield
{
    public class CharacterMovementState : IState
    {
        protected CharacterMovementStateMachine movementStateMachine { get; }
        protected Character character { get; }
        protected Animator animator { get; }
        protected CharacterStateReusableData reusableData { get; }

        public CharacterMovementState(CharacterMovementStateMachine stateMachine)
        {
            movementStateMachine = stateMachine;
            character = stateMachine.character;
            animator = character.animator;
            reusableData = stateMachine.reusableData;
        }

        public virtual void Enter()
        {
            Debug.Log(character.name + " 状态为 " + GetType().Name);
        }

        public virtual void Exit() { }

        public virtual void HandInput()
        {
            Vector3 moveDir = character.GetMovementInput();
            animator.SetFloat(AnimationID.MovementID,
                moveDir.magnitude * reusableData.inputMult, 0.35f, Time.deltaTime);
        }

        public virtual void Update()
        {
            CharacterRotation(character.GetMovementInput());
        }

        private float currentVelocity;
        protected void CharacterRotation(Vector3 movementDirection)
        {
            if (movementDirection == Vector3.zero) return;

            reusableData.targetAngle = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg;

            character.transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(
                character.transform.eulerAngles.y,
                reusableData.targetAngle,
                ref currentVelocity,
                reusableData.rotationTime);
        }

        public virtual void OnAnimationTranslateEvent(IState state)
        {
            movementStateMachine.ChangeState(state);
        }

        public virtual void OnAnimationExitEvent() { }
    }
}
