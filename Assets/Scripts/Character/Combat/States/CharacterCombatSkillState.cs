using UnityEngine;
using Endfield.Module.Audio;
namespace Endfield
{
    /// <summary>
    /// 技能状态：单段，由 skillAttack 输入直接进入（不走动画事件，避免与普攻 ATK 路由冲突）。
    /// 播技能动画 + 复用攻击检测；技能动画播完回 Null 并恢复移动。
    /// </summary>
    public class CharacterCombatSkillState : CharacterCombatState
    {
        private CombatConfigSO _currentCombatConfig;
        public CharacterCombatSkillState(CharacterCombatStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            var skillData = _resuableData.currentSkillData ?? _character.SkillAttackData;
            if (skillData == null || skillData.combatConfigs == null || skillData.combatConfigs.Length == 0)
                return;
            _character.PlayVoice(SoundStyle.SkillVoice);
            _currentCombatConfig = skillData.combatConfigs[0];
            //TODO:现在就是霸体，以后要改成技能体，霸体，金刚体等复杂状态
            _character.attribute.superArmor = true;
            _combatController.StartAttackDetection(skillData, 0);
        }

        public override void HandInput()
        {
            // 技能/连携中：普攻/战技/连携都不能打断（战技↔连携互不打断）
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
            _combatController.UpdateAttackDetection();
            if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < _currentCombatConfig.rotationTime && _currentCombatConfig.isRotationToTarget)
            {
                Transform target = _combatController.GetCurrentTarget();
                if (target)
                {
                    _combatController.FaceTarget(target);
                }
            }

            // 技能动画播完 → 回 Null + 恢复移动（直接进状态不走动画事件，移动要自己恢复）
            if (!_animator.IsInTransition(0) &&
                !_animator.GetCurrentAnimatorStateInfo(0).IsTag("Skill"))
            {
                _combatStateMachine.ChangeState(CharacterCombatStateType.Null);
                _character.movementStateMachine.ChangeState(
                    _character.GetMovementInput() != Vector3.zero
                        ? CharacterMovementStateType.Run
                        : CharacterMovementStateType.Idle);
            }
        }

        public override void Exit()
        {
            base.Exit();
            _character.attribute.superArmor = false;
        }
    }
}
