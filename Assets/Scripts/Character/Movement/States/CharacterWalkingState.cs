using UnityEngine;

namespace Endfield
{
    public class CharacterWalkingState : CharacterMovementState
    {
        public CharacterWalkingState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            reusableData.rotationTime = 0.1f;
            reusableData.inputMult = 1f;
            animator.CrossFadeInFixedTime("WalkStart", 0.14f);
            animator.SetFloat(AnimationID.MovementID, reusableData.inputMult);
            animator.SetBool(AnimationID.HasInputID, true);
        }

        public override void Update()
        {
            base.Update();

            if (character.GetMovementInput() == UnityEngine.Vector3.zero)
            {
                movementStateMachine.ChangeState(movementStateMachine.idlingState);
                return;
            }

            if (!reusableData.shouldWalk)
                movementStateMachine.ChangeState(movementStateMachine.runningState);
        }
    }
}
