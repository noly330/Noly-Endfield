namespace Endfield
{
    public class CharacterRunningState : CharacterMovementState
    {
        public CharacterRunningState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            reusableData.rotationTime = 0.05f;
            reusableData.inputMult = 2f;
            animator.CrossFadeInFixedTime("WalkStart", 0.14f);
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

            if (reusableData.shouldWalk)
                movementStateMachine.ChangeState(movementStateMachine.walkingState);
        }
    }
}
