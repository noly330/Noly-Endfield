using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Endfield;

[TaskCategory("Operator")]
public class SetFollowTarget : Action
{
    public SharedTransform target;
    private OperatorAIController _controller;

    public override void OnAwake() => _controller = GetComponent<OperatorAIController>();
    public override TaskStatus OnUpdate()
    {
        _controller.followTarget = target.Value;
        return TaskStatus.Success;
    }
}
