using UnityEngine;

namespace Endfield
{
    public class OperatorCombatStateMachine : StateMachine
    {
        public Operator operatorCharacter { get; }
        public OperatorCombatResuableData resuableData { get; }

        public OperatorCombatNullState nullState { get; private set; }

        public OperatorCombatStateMachine(Operator operatorCharacter)
        {
            resuableData = new OperatorCombatResuableData();
            this.operatorCharacter = operatorCharacter;

            nullState = new OperatorCombatNullState(this);
        }
    }
}