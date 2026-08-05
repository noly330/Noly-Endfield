using BehaviorDesigner.Runtime.Tasks;
using Endfield;

[TaskCategory("Operator")]
public class AttackTarget : Action
{
    private OperatorAIController _controller;

    public override void OnAwake() => _controller = GetComponent<OperatorAIController>();

    public override TaskStatus OnUpdate()
    {
        _controller.TryAttack();
        return TaskStatus.Success;
    }
}
