using Endfield.Module.Timer;
using UnityEngine;

namespace Endfield
{
    public abstract class CharacterCombatState : IState
    {
        protected Character _character { get; }
        protected CharacterCombatStateMachine _combatStateMachine { get; }
        protected CharacterCombatController _combatController { get; }
        protected CharacterCombatReusableData _resuableData { get; }
        protected CharacterCombatData _combatData { get; }
        protected Animator _animator { get; }
        protected GameTimer _comboResetTimer;

        public CharacterCombatState(CharacterCombatStateMachine combatStateMachine)
        {
            _combatStateMachine = combatStateMachine;
            _combatController = combatStateMachine.combatController;
            _character = combatStateMachine.character;
            _resuableData = combatStateMachine.resuableData;
            _combatData = _character.CombatData;
            _animator = _character.animator;
        }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void HandInput()
        {
            if (_character.combatDriver.normalAttack)
            {
                _character.combatDriver.normalAttack = false;

                if (!_combatStateMachine.combatController.canAttack)
                {
                    return;
                }
                _combatStateMachine.combatController.SetAttackColdTime();
                int index = _resuableData.nextCombatIndex;
                //Debug.Log("进行普攻第" + (index +1) + "段");
                _resuableData.combatIndex = index;
                _resuableData.nextCombatIndex = (index + 1) % _combatData.normalAttackData.TryGetCombatCount();
                
                _animator.CrossFadeInFixedTime(_combatData.normalAttackData.TryGetCombatName(index), 0.1555f);
            }
        }

        protected void RestartComboResetTimer(float coldTime)
        {
            if (_comboResetTimer != null) TimerManager.Instance.UnregisterTimer(_comboResetTimer);
            _comboResetTimer = TimerManager.Instance.GetTimer(coldTime, OnComboReset);
        }

        private void OnComboReset()
        {
            _resuableData.nextCombatIndex = 0;
            _comboResetTimer = null;
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
