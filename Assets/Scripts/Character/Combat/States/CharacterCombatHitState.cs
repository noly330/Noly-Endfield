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
            // 受击中不能出招，吞掉攻击输入
            if (_character.combatDriver.normalAttack)
                _character.combatDriver.normalAttack = false;
        }

        public override void Update()
        {
            base.Update();
            // 受击动画真正结束后才回 Null。
            // 用 tag 判断而非 OnAnimationExitEvent：退出事件由任意带 behaviour 的状态退出时触发，
            // 受击打断攻击/闪避时会提前退出（参考 NormalATKState 的 IsTag("ATK") 退出写法）。
            if (!_animator.IsInTransition(0) && !_animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit"))
            {
                _combatStateMachine.ChangeState(CharacterCombatStateType.Null);
            }
        }
    }
}
