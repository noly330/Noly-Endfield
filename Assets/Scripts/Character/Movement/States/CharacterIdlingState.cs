using UnityEngine;

namespace Endfield
{
    public class CharacterIdlingState : CharacterMovementState
    {
        public CharacterIdlingState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            reusableData.rotationTime = 0.15f;
            reusableData.inputMult = 0f;
            animator.SetFloat(AnimationID.MovementID, 0);
            animator.SetBool(AnimationID.HasInputID, false);
        }

        public override void Update()
        {
            base.Update();

            if (character.GetMovementInput() == UnityEngine.Vector3.zero)
                return;

            if (reusableData.shouldWalk)
                movementStateMachine.ChangeState(movementStateMachine.walkingState);
            else
                movementStateMachine.ChangeState(movementStateMachine.runningState);
        }
    }
}
