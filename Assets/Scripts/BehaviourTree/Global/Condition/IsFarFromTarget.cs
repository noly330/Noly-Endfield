using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Operator")]
public class IsFarFromTarget : Conditional
{
    public SharedTransform target;
    public SharedFloat maxDistance = 3f;

    public override TaskStatus OnUpdate()
    { 
        if (target.Value == null)
            return TaskStatus.Failure;

        float dist = Vector3.Distance(transform.position, target.Value.position);
        return dist > maxDistance.Value ? TaskStatus.Success : TaskStatus.Failure;
    }
}
