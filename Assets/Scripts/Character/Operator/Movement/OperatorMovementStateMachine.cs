namespace Endfield
{
    public class OperatorMovementStateMachine : StateMachine
    {
        public Operator operatorCharacter { get; }
        public OperatorStateReusableData reusableData { get; }

        public OperatorIdlingState idlingState { get; private set; }
        public OperatorWalkingState walkingState { get; private set; }
        public OperatorRunningState runningState { get; private set; }
        public OperatorDodgingState dodgingState { get; private set; }

        public OperatorMovementStateMachine(Operator operatorCharacter)
        {
            this.operatorCharacter = operatorCharacter;
            reusableData = new OperatorStateReusableData();

            idlingState = new OperatorIdlingState(this);
            walkingState = new OperatorWalkingState(this);
            runningState = new OperatorRunningState(this);
            dodgingState = new OperatorDodgingState(this);
        }
    }
}
