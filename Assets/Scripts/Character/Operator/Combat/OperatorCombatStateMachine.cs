using UnityEngine;

namespace Endfield
{
    public class OperatorCombatStateMachine : StateMachine
    {
        public Operator operatorCharacter { get; }
        public OperatorCombatResuableData resuableData { get; }
        public OperatorCombatController combatController { get; }
        public OperatorCombatNullState nullState { get; private set; }
        public OperatorCombatNormalATKState normalATKState { get; private set; }

        public OperatorCombatStateMachine(Operator operatorCharacter)
        {
            resuableData = new OperatorCombatResuableData();
            this.operatorCharacter = operatorCharacter;
            combatController = operatorCharacter.combatController;
            nullState = new OperatorCombatNullState(this);
            normalATKState = new OperatorCombatNormalATKState(this);
        }
    }
}