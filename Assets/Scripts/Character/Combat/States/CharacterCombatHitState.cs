using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 受击状态：由受击动画进入（OnAnimationTranslate(Hit)）。
    /// 受击中不响应攻击输入；受击动画播放结束后（脱离 Hit tag）回到 Null。
    /// </summary>
    public class CharacterCombatHitState : CharacterCombatState
    {
        public CharacterCombatHitState(CharacterCombatStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void HandInput()
        {
            // 受击中不能出招，吞掉攻击/技能/连携输入
            if (_character.combatDriver.normalAttack)
                _character.combatDriver.normalAttack = false;
            if (_character.combatDriver.skillAttack)
                _character.combatDriver.skillAttack = false;
            if (_character.combatDriver.linkAttack)
                _character.combatDriver.linkAttack = false;
        }

        public override void Update()
        {
            base.Update();
            if (!_animator.IsInTransition(0) && !_animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit"))
            {
                _combatStateMachine.ChangeState(CharacterCombatStateType.Null);
            }
        }

        public override void Exit()
        {
            base.Exit();
            _character.movementStateMachine.ChangeState(CharacterMovementStateType.Idle);
        }

    }
}
