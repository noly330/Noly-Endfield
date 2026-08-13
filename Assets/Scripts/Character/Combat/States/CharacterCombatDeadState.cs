using UnityEngine;

namespace Endfield
{
    public class CharacterCombatDeadState : CharacterCombatState
    {
        private bool _deadNotified;
        public CharacterCombatDeadState(CharacterCombatStateMachine combatStateMachine) : base(combatStateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _animator.CrossFadeInFixedTime("Dead", 0.1555f);
            _deadNotified = false;
        }

        public override void HandInput()
        {
            if (_character.combatDriver.normalAttack == true)
                _character.combatDriver.normalAttack = false;
            if (_character.combatDriver.skillAttack == true)
                _character.combatDriver.skillAttack = false;
            if (_character.combatDriver.linkAttack == true)
                _character.combatDriver.linkAttack = false;
        }

        public override void Update()
        {
            base.Update();
            
            if (!_deadNotified && !_animator.IsInTransition(0) &&
                _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f)
            {
                _deadNotified = true;
                _character.NotifyDeathAnimationEnd();
            }
        }
    }
}
