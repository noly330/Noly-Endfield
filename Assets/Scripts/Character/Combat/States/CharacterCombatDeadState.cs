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
        }

        public override void Update()
        {
            base.Update();
            // 死亡动画播完（normalizedTime 到 1）才通知一次，供对象池回收。
            // 用 normalizedTime 而非 IsTag：IsTag("Dead") 在动画一开始就是 true，会提前回收打断死亡动画。
            if (!_deadNotified && !_animator.IsInTransition(0) &&
                _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f)
            {
                _deadNotified = true;
                _character.NotifyDeathAnimationEnd();
            }
        }
    }
}
