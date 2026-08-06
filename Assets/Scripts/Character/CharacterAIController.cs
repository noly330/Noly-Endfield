using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace Endfield
{
    public class CharacterAIController : MonoBehaviour
    {
        [SerializeField] private TargetSearchStrategySO _searchStrategy;   // Inspector 选择搜索模式
        //TODO: 搜索半径写死，之后进 AI 配置
        private const float SearchRadius = 10f;
        private float _searchTimer;

        private Character _character;
        private BehaviorTree _behaviorTree;
        private NavMeshAgent _navMeshAgent;
        public Transform CurrentTarget { get; private set; }


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
            _searchTimer -= Time.deltaTime;
            if (_searchTimer <= 0f)
            {
                _searchTimer = 0.2f;
                CurrentTarget = (_searchStrategy != null && _character != null)
                    ? _searchStrategy.FindTarget(_character, SearchRadius)
                    : null;
            }

            if (_navMeshAgent == null || !_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
                return;
            // 防漂移：每帧同步 NavMesh 内部位置
            _navMeshAgent.nextPosition = transform.position;
        }
        
        #region 行为树调用方法
        public void TryAttack() => _character.combatDriver.normalAttack = true;

        public void MoveTo(Vector3 position)
        {
            if (_navMeshAgent == null || !_navMeshAgent.enabled || !_navMeshAgent.isOnNavMesh)
                return;
            _navMeshAgent.SetDestination(position);
            _character.movementDriver.worldDirection = _navMeshAgent.desiredVelocity.normalized;
        }

        public void Stop() => _character.movementDriver.worldDirection = Vector3.zero;
        #endregion
    }
}
