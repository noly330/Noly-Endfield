using Endfield.Tools;
using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 正常攻击状态。覆写 CharacterCombatState.HandInput() 检测攻击输入。
    /// </summary>
    public class CharacterCombatNormalATKState : CharacterCombatState
    {
        private CombatConfigSO _currentCombatConfig;
        public CharacterCombatNormalATKState(CharacterCombatStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            //进入普攻
            CombatSetSO combatSet = _combatData.normalAttackData;
            _currentCombatConfig = combatSet.combatConfigs[_resuableData.combatIndex];
            _combatController.StartAttackDetection(combatSet, _resuableData.combatIndex);

            if (_comboResetTimer != null) TimerManager.Instance.UnregisterTimer(_comboResetTimer);
        }

        public override void Update()
        {
            base.Update();
            _combatController.UpdateAttackDetection();
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < _currentCombatConfig.rotationTime && _currentCombatConfig.isRotationToTarget)
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
        public override void Exit()
        {
            base.Exit();
            Debug.Log("退出普攻状态");
            RestartComboResetTimer(_combatData.normalAttackData.TryGetColdTime(_resuableData.combatIndex));
        }

        public override void OnAnimationTranslateEvent(IState state)
        {
            _combatStateMachine.ChangeState(state);
        }

        public override void OnAnimationExitEvent()
        {
        }
    }
}
