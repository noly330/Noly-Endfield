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
            // 连携优先：任何非 Hit/Dead 状态响应连携输入（连携/战技均可打断普攻）
            if (_character.combatDriver.linkAttack)
            {
                _character.combatDriver.linkAttack = false;
                var linkData = _character.LinkAttackData;
                if (linkData != null)
                {
                    _resuableData.currentSkillData = linkData;
                    _animator.CrossFadeInFixedTime(linkData.TryGetCombatName(0), 0f);
                }
                return;
            }

            // 技能优先：任何非 Hit/Dead 状态响应技能输入（技能打断普攻）
            if (_character.combatDriver.skillAttack)
            {
                _character.combatDriver.skillAttack = false;
                var skillData = _character.SkillAttackData;
                if (skillData != null)
                {
                    _resuableData.currentSkillData = skillData;
                    _animator.CrossFadeInFixedTime(skillData.TryGetCombatName(0), 0.1555f);
                }
                return;
            }

            //普攻
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
