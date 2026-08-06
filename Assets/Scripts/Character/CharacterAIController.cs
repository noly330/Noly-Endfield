using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace Endfield
{
    /// <summary>
    /// 角色 AI 执行层：行为树负责决策（追击/攻击），本控制器把决策翻译成
    /// 移动/战斗状态机的输入。不内联任何 AI 行为逻辑。
    /// </summary>
    public class CharacterAIController : MonoBehaviour
    {
        private Character _character;
        private BehaviorTree _behaviorTree;
        private NavMeshAgent _navMeshAgent;

        /// <summary>行为树调用：请求一次攻击（战斗状态机会消费并执行）</summary>
        public void TryAttack() => _character.combatDriver.normalAttack = true;

        /// <summary>行为树调用（ChaseTarget）：朝目标位置追击</summary>
        public void MoveTo(Vector3 position)
        {
            if (_navMeshAgent == null || !_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
                return;
            _navMeshAgent.SetDestination(position);
            _character.movementDriver.worldDirection = _navMeshAgent.desiredVelocity.normalized;
        }

        /// <summary>行为树调用：停止移动</summary>
        public void Stop() => _character.movementDriver.worldDirection = Vector3.zero;

        private void Awake()
        {
            _behaviorTree = GetComponent<BehaviorTree>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _character = GetComponent<Character>();
        }

        private void OnEnable()
        {
            if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.updatePosition = false;
                _navMeshAgent.updateRotation = false;
            }
        }

        private void Update()
        {
            if (_navMeshAgent == null || !_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
                return;
            // 防漂移：每帧同步 NavMesh 内部位置（追击/停止由行为树调用 MoveTo/Stop）
            _navMeshAgent.nextPosition = transform.position;
        }
    }
}
