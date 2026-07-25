using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Endfield;
[TaskCategory("Operator")]
public class ClearFollowTarget : Action
{
    private OperatorAIController _controller;

    public override void OnAwake() => _controller = GetComponent<OperatorAIController>();
    public override TaskStatus OnUpdate()
    {
        _controller.followTarget = null;
        return TaskStatus.Success;
    }
}