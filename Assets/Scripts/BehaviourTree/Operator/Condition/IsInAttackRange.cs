using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Operator")]
public class IsInAttackRange : Conditional
{
    public SharedTransform target;
    public SharedFloat attackRange = 2.5f;

    public override TaskStatus OnUpdate()
    {
        if (target.Value == null)
            return TaskStatus.Failure;

        float dist = Vector3.Distance(transform.position, target.Value.position);
        return dist <= attackRange.Value ? TaskStatus.Success : TaskStatus.Failure;
    }
}
