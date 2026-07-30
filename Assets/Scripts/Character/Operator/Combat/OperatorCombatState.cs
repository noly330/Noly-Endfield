using System.Collections;
using System.Collections.Generic;
using Endfield;
using UnityEngine;

namespace Endfield
{
    public abstract class OperatorCombatState : IState
    {
        protected Operator _operatorCharacter{get;}
        protected OperatorCombatStateMachine _combatStateMachine{get;}
        protected OperatorCombatResuableData _resuableData{get;}
        protected OperatorCombatData _combatData{get;}
        protected Animator _animator{get;}
        public OperatorCombatState(OperatorCombatStateMachine combatStateMachine)
        {
            _combatStateMachine = combatStateMachine;
            _operatorCharacter = combatStateMachine.operatorCharacter;
            _resuableData = combatStateMachine.resuableData;
            _combatData = _operatorCharacter.operatorSO.combatData;
            _animator = _operatorCharacter.animator;
        }
        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void HandInput()
        {
            if (_operatorCharacter.combatDriver.normalAttack)
            {

                _operatorCharacter.combatDriver.normalAttack = false;
                int index = _resuableData.nextComboIndex;
                Debug.Log("进行普攻第"+index+"段");
                _resuableData.comboIndex = index;
                _resuableData.nextComboIndex = (index+1)%_combatData.normalAttackData.TryGetCombatCount();
                _animator.CrossFadeInFixedTime(_combatData.normalAttackData.TryGetCombatName(index),0.2f);
            }
        }

        public void OnAnimationExitEvent()
        {
        }

        public void OnAnimationTranslateEvent(IState state)
        {
        }

        public void Update()
        {
        }
    }
}