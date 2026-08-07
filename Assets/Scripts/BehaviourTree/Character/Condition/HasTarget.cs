using BehaviorDesigner.Runtime.Tasks;
using Endfield;
using UnityEngine;

[TaskCategory("Character")]

public class HasTarget : Conditional
{
    private CharacterAIController _controller;
    public override void OnAwake() => _controller = GetComponent<CharacterAIController>();

    public override TaskStatus OnUpdate()
    {
        return _controller.CurrentTarget != null
            ? TaskStatus.Success : TaskStatus.Failure;
    }

}
