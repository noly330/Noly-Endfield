using BehaviorDesigner.Runtime.Tasks;
using Endfield;

[TaskCategory("Operator")]
public class AttackTarget : Action
{
    private CharacterAIController _controller;

    public override void OnAwake() => _controller = GetComponent<CharacterAIController>();

    public override TaskStatus OnUpdate()
    {
        _controller.TryAttack();
        return TaskStatus.Success;
    }
}
