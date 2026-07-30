namespace Endfield
{
    public class OperatorMovementStateMachine : StateMachine
    {
        public Operator operatorCharacter { get; }
        public OperatorMovementReusableData reusableData { get; }

        public OperatorIdlingState idlingState { get; private set; }
        public OperatorWalkingState walkingState { get; private set; }
        public OperatorRunningState runningState { get; private set; }
        public OperatorDashingState dashingState { get; private set; }
        public OperatorSprintingState sprintingState { get; private set; }
        public OperatorReturnRunState returnRunState { get; private set; }

        public OperatorMovementStateMachine(Operator operatorCharacter)
        {
            this.operatorCharacter = operatorCharacter;
            reusableData = new OperatorMovementReusableData();

            idlingState = new OperatorIdlingState(this);
            walkingState = new OperatorWalkingState(this);
            runningState = new OperatorRunningState(this);
            dashingState = new OperatorDashingState(this);
            sprintingState = new OperatorSprintingState(this);
            returnRunState = new OperatorReturnRunState(this);
        }
    }
}
