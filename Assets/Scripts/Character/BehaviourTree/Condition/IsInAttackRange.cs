using BehaviorDesigner.Runtime.Tasks;
using Endfield;
using UnityEngine;

[TaskCategory("Character")]
public class IsInAttackRange : Conditional
{
    private CharacterAIController _controller;

    public override void OnAwake() => _controller = GetComponent<CharacterAIController>();

    public override TaskStatus OnUpdate()
    {
        Transform target = _controller.CurrentTarget;   // 目标由 AIController 搜索缓存
        if (target == null)
            return TaskStatus.Failure;

        return Vector3.Distance(transform.position, target.position) <= _controller.AIData.attackRange
            ? TaskStatus.Success : TaskStatus.Failure;
    }
}
