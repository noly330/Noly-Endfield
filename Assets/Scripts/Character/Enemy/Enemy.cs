using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace Endfield
{

    public class Enemy : Character
    {
        public EnemySO enemySO;

        public override CharacterMovementData MovementData => enemySO.movementData;
        public override CharacterCombatData CombatData => enemySO.combatData;
        protected override CharacterAttributeData AttributeData => enemySO.attributeData;
        public override CharacterAIData AIData => enemySO.AIData;

        /// <summary>
        /// 对象池复用重置：回满血、状态机回初始、清动画、恢复 AI。
        /// 放 OnEnable（pool.Get() = SetActive(true) 自动触发），spawner 不用手动调。
        /// 只在 Enemy 上（只有敌人池化），不污染 Character 基类。
        /// </summary>
        private void OnEnable()
        {
            GetComponent<CharacterHealth>().Reset();
            combatStateMachine.ChangeState(CharacterCombatStateType.Null);
            movementStateMachine.ChangeState(CharacterMovementStateType.Idle);
            animator.Rebind();   // 清掉 Dead 状态，回初始 Idle
            Buffs.RemoveAllBuffs();   // 清破防/碎甲等，对象池复用不残留

            var aiCtrl = GetComponent<CharacterAIController>();
            if (aiCtrl != null) aiCtrl.enabled = true;
            var behaviorTree = GetComponent<BehaviorTree>();
            if (behaviorTree != null) behaviorTree.enabled = true;
            var navMeshAgent = GetComponent<NavMeshAgent>();
            if (navMeshAgent != null) navMeshAgent.enabled = true;
        }
    }
}
