namespace Endfield
{
    public class CharacterMovementStateMachine : StateMachine
    {
        public Character character { get; }
        public CharacterStateReusableData reusableData { get; }

        public CharacterIdlingState idlingState { get; private set; }
        public CharacterWalkingState walkingState { get; private set; }
        public CharacterRunningState runningState { get; private set; }
        public CharacterDashingState dashingState { get; private set; }

        public CharacterMovementStateMachine(Character character)
        {
            this.character = character;
            reusableData = new CharacterStateReusableData();

            idlingState = new CharacterIdlingState(this);
            walkingState = new CharacterWalkingState(this);
            runningState = new CharacterRunningState(this);
            dashingState = new CharacterDashingState(this);
        }
    }
}
