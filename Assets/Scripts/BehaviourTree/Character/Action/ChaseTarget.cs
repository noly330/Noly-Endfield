using BehaviorDesigner.Runtime.Tasks;
using Endfield;
using UnityEngine;

[TaskCategory("Character")]
public class ChaseTarget : Action
{
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

        if (Vector3.Distance(transform.position, target.position) <= _controller.AIData.stopDistance)
        {
            _controller.Stop();
            return TaskStatus.Success;
        }

        _controller.MoveTo(target.position);
        return TaskStatus.Running;
    }
}
