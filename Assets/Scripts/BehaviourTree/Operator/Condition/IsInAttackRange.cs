using BehaviorDesigner.Runtime.Tasks;
using Endfield;
using UnityEngine;

[TaskCategory("Operator")]
public class IsInAttackRange : Conditional
{
    //TODO: 攻击范围写死，之后配置化
    private const float AttackRange = 2.5f;

    private CharacterAIController _controller;

    public override void OnAwake() => _controller = GetComponent<CharacterAIController>();

    public override TaskStatus OnUpdate()
    {
        Transform target = _controller.CurrentTarget;   // 目标由 AIController 搜索缓存
        if (target == null)
            return TaskStatus.Failure;

        return Vector3.Distance(transform.position, target.position) <= AttackRange
            ? TaskStatus.Success : TaskStatus.Failure;
    }
}
