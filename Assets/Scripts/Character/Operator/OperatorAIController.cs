using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace Endfield
{
    public class OperatorAIController : MonoBehaviour
    {
        private Operator _operator;
        private BehaviorTree _behaviorTree;
        private NavMeshAgent _navMeshAgent;

        //TODO: 暂时这样写，先测试AI能否进行移动操作
        private float _stopDistance = 2.5f;
        public Transform followTarget;

        private void Awake()
        {
            _behaviorTree = GetComponent<BehaviorTree>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _operator = GetComponent<Operator>();
        }

        private void OnEnable()
        {
            if(_navMeshAgent == null && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.updatePosition = false;
                _navMeshAgent.updateRotation = false;
            }
        }

        private void Update()
        {
            if(_navMeshAgent == null || !_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
                return;
            //行为树负责设置followTarget
            if(followTarget != null)
                _navMeshAgent.SetDestination(followTarget.position);

            if(_navMeshAgent.hasPath && _navMeshAgent.remainingDistance > _stopDistance)
            {
                Vector3 vel = _navMeshAgent.desiredVelocity;
                _operator.movementDriver.worldDirection = vel.normalized;
            }
            else
            {
                _operator.movementDriver.worldDirection = Vector3.zero;
            }

            // 防漂移：每帧同步 NavMesh 内部位置
            _navMeshAgent.nextPosition = transform.position;
        }

    }
}
