using BehaviorDesigner.Runtime.Tasks;
using Endfield;

[TaskCategory("Operator")]
public class Idle : Action
{
    private CharacterAIController _controller;

    public override void OnAwake() => _controller = GetComponent<CharacterAIController>();

    public override TaskStatus OnUpdate()
    {
        _controller.Stop();

        return TaskStatus.Running;
    }
}
