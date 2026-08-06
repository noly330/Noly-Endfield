using BehaviorDesigner.Runtime.Tasks;
using Endfield;
using UnityEngine;

[TaskCategory("Operator")]
public class ChaseTarget : Action
{
    //TODO: 停距先写死，之后配置化
    private const float StopDistance = 2.5f;

    private CharacterAIController _controller;

    public override void OnAwake() => _controller = GetComponent<CharacterAIController>();

    public override TaskStatus OnUpdate()
    {
        Transform target = _controller.CurrentTarget;
        if (target == null)
        {
            _controller.Stop();
            return TaskStatus.Failure;
        }

        if (Vector3.Distance(transform.position, target.position) <= StopDistance)
        {
            _controller.Stop();
            return TaskStatus.Success;
        }

        _controller.MoveTo(target.position);
        return TaskStatus.Running;
    }
}
