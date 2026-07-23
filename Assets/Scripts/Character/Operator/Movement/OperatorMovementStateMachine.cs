namespace Endfield
{
    public class OperatorMovementStateMachine : StateMachine
    {
        public Operator operatorCharacter { get; }
        public OperatorStateReusableData reusableData { get; }

        public OperatorIdlingState idlingState { get; private set; }
        public OperatorWalkingState walkingState { get; private set; }
        public OperatorRunningState runningState { get; private set; }
        public OperatorDashingState dashingState { get; private set; }

        public OperatorMovementStateMachine(Operator operatorCharacter)
        {
            this.operatorCharacter = operatorCharacter;
            reusableData = new OperatorStateReusableData();

            idlingState = new OperatorIdlingState(this);
            walkingState = new OperatorWalkingState(this);
            runningState = new OperatorRunningState(this);
            dashingState = new OperatorDashingState(this);
        }
    }
}
