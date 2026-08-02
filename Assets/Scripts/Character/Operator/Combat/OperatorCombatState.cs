using System.Collections;
using System.Collections.Generic;
using Endfield;
using UnityEngine;

namespace Endfield
{
    public abstract class OperatorCombatState : IState
    {
        protected Operator _operatorCharacter { get; }
        protected OperatorCombatStateMachine _combatStateMachine { get; }
        protected OperatorCombatController _combatController { get; }
        protected OperatorCombatResuableData _resuableData { get; }
        protected OperatorCombatData _combatData { get; }
        protected Animator _animator { get; }
        public OperatorCombatState(OperatorCombatStateMachine combatStateMachine)
        {
            _combatStateMachine = combatStateMachine;
            _combatController = combatStateMachine.combatController;
            _operatorCharacter = combatStateMachine.operatorCharacter;
            _resuableData = combatStateMachine.resuableData;
            _combatData = _operatorCharacter.operatorSO.combatData;
            _animator = _operatorCharacter.animator;
        }
        public virtual void Enter()
        {
            //Debug.Log(_operatorCharacter.name + " 战斗状态为 " + GetType().Name);
        }

        public virtual void Exit()
        {
        }

        public virtual void HandInput()
        {
            if (_operatorCharacter.combatDriver.normalAttack)
            {

                _operatorCharacter.combatDriver.normalAttack = false;

                if (!_combatStateMachine.combatController.canAttack)
                {
                    Debug.Log("攻击冷却中");
                    return;
                }
                _combatStateMachine.combatController.SetAttackColdTime();
                int index = _resuableData.nextCombatIndex;
                Debug.Log("进行普攻第" + index + "段");
                _resuableData.combatIndex = index;
                _resuableData.nextCombatIndex = (index + 1) % _combatData.normalAttackData.TryGetCombatCount();
                _animator.CrossFadeInFixedTime(_combatData.normalAttackData.TryGetCombatName(index), 0.1555f);
            }
        }

        public virtual void OnAnimationExitEvent()
        {
        }

        public virtual void OnAnimationTranslateEvent(IState state)
        {
        }

        public virtual void Update()
        {
        }
    }
}