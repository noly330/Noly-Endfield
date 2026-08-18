using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using Endfield.Module.Audio;

namespace Endfield
{
    /// <summary>
    /// 干员：Character 的具体子类，提供干员专属的 OperatorSO 数据。
    /// </summary>
    public class Operator : Character
    {
        [SerializeField] private OperatorSO operatorSO;

        /// <summary>干员数据（队伍/编队等按 OperatorSO 标识干员）。</summary>
        public OperatorSO OperatorData => operatorSO;

        public override CharacterMovementData MovementData => operatorSO.movementData;
        public override CharacterCombatData CombatData => operatorSO.combatData;
        protected override CharacterAttributeData AttributeData => operatorSO.attributeData;
        public override CharacterAIData AIData => operatorSO.AIData;
        public override SoundData SoundData => operatorSO.soundData;
        public override CombatSetSO SkillAttackData => operatorSO.combatData != null ? operatorSO.combatData.skillAttackData : null;
        public override CombatSetSO LinkAttackData => operatorSO.combatData != null ? operatorSO.combatData.linkAttackData : null;
        public override float LinkCooldown => operatorSO.combatData != null ? operatorSO.combatData.linkCooldown : 0f;

        /// <summary>切换玩家/AI 控制：切自己的控制组件（playerCtrl / aiCtrl / BehaviorTree / NavMeshAgent）。</summary>
        public void SetPlayerControl(bool isPlayer)
        {
            var playerCtrl = GetComponent<CharacterPlayerController>();
            var aiCtrl = GetComponent<CharacterAIController>();
            var behaviorTree = GetComponent<BehaviorTree>();
            var navMeshAgent = GetComponent<NavMeshAgent>();

            if (isPlayer)
            {
                if (navMeshAgent != null) navMeshAgent.enabled = false;
                if (behaviorTree != null) behaviorTree.enabled = false;
                if (aiCtrl != null) aiCtrl.enabled = false;
                if (playerCtrl != null) playerCtrl.enabled = true;
            }
            else
            {
                if (playerCtrl != null) playerCtrl.enabled = false;
                if (navMeshAgent != null)
                {
                    navMeshAgent.enabled = true;
                    navMeshAgent.Warp(transform.position);
                    navMeshAgent.ResetPath();
                }
                if (aiCtrl != null) aiCtrl.enabled = true;
                if (behaviorTree != null) behaviorTree.enabled = true;
            }
        }
    }
}
