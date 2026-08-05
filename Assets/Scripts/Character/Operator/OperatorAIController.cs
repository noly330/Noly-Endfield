using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace Endfield
{
    /// <summary>
    /// 干员 AI 执行层：行为树负责决策（追击/攻击），本控制器把决策翻译成
    /// 移动/战斗状态机的输入。不内联任何 AI 行为逻辑。
    /// </summary>
    public class OperatorAIController : MonoBehaviour
    {
        private Operator _operator;
        private BehaviorTree _behaviorTree;
        private NavMeshAgent _navMeshAgent;

        // 旧跟随测试遗留：仅被 SetFollowTarget / ClearFollowTarget 任务写入，移动已改由行为树驱动
        public Transform followTarget;


        private void Awake()
        {
            _behaviorTree = GetComponent<BehaviorTree>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _operator = GetComponent<Operator>();
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
            // 防漂移：每帧同步 NavMesh 内部位置
            _navMeshAgent.nextPosition = transform.position;
        }
        #region 行为树调用方法
        public void TryAttack() => _operator.combatDriver.normalAttack = true;

        public void MoveTo(Vector3 position)
        {
            if (_navMeshAgent == null || !_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
                return;
            _navMeshAgent.SetDestination(position);
            _operator.movementDriver.worldDirection = _navMeshAgent.desiredVelocity.normalized;
        }

        public void Stop() => _operator.movementDriver.worldDirection = Vector3.zero;
        #endregion
    }
}
