using UnityEngine;

namespace Endfield
{
    public class CharacterDashingState : CharacterMovementState
    {
        private readonly float dashCooldown = 1.5f;
        private float cooldownTimer;

        public CharacterDashingState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            reusableData.canDash = false;
            cooldownTimer = dashCooldown;

            Vector3 inputDir = character.GetMovementInput();
            if (inputDir != Vector3.zero)
                animator.CrossFadeInFixedTime("Dash", 0.1f);
            else
                animator.CrossFadeInFixedTime("DashBack", 0.1f);
            animator.SetBool(AnimationID.HasInputID, true);
        }

        public override void Update()
        {
            base.Update();

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                    reusableData.canDash = true;
            }
        }

        public override void OnAnimationExitEvent()
        {
            if (character.GetMovementInput() != Vector3.zero)
                movementStateMachine.ChangeState(movementStateMachine.runningState);
            else
                movementStateMachine.ChangeState(movementStateMachine.idlingState);
        }
    }
}
