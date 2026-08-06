using Endfield.Tools;
using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 正常攻击状态。覆写 CharacterCombatState.HandInput() 检测攻击输入。
    /// </summary>
    public class CharacterCombatNormalATKState : CharacterCombatState
    {
        public CharacterCombatNormalATKState(CharacterCombatStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            CombatSetSO combatSet = _combatData.normalAttackData;
            _combatController.StartAttackDetection(combatSet, _resuableData.combatIndex);
        }

        public override void Update()
        {
            base.Update();
            _combatController.UpdateAttackDetection();
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.3f)
            {
                Transform target = _combatController.GetCurrentTarget();
                if (target)
                {
                    _combatController.FaceTarget(target);
                }
            }

            // 攻击动画真正结束后才回 null（不用定时器，避免连段过渡期间误切导致丢检测）
            // 过渡中、当前动画是 ATK、或正在切入 ATK 动画时都保持攻击状态
            if (!_animator.IsInTransition(0) &&
                !_animator.GetCurrentAnimatorStateInfo(0).IsTag("ATK") &&
                !_animator.GetNextAnimatorStateInfo(0).IsTag("ATK"))
            {
                _combatStateMachine.ChangeState(CharacterCombatStateType.Null);
            }
        }

        public override void OnAnimationTranslateEvent(IState state)
        {
            _combatStateMachine.ChangeState(state);
        }

        public override void OnAnimationExitEvent()
        {
            RestartComboResetTimer(_combatData.normalAttackData.TryGetColdTime(_resuableData.combatIndex));
        }
    }
}
