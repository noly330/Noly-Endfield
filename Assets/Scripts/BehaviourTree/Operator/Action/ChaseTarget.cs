using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Endfield;
using UnityEngine;

[TaskCategory("Operator")]
public class ChaseTarget : Action
{
    //TODO: 停距先写死，之后配置化
    private const float StopDistance = 2.5f;

    public SharedTransform target;
    private OperatorAIController _controller;

    public override void OnAwake() => _controller = GetComponent<OperatorAIController>();

    public override TaskStatus OnUpdate()
    {
        if (target.Value == null)
            return TaskStatus.Failure;

        // 追到停距：停下并返回 Success，让 Selector 有机会评估攻击分支
        if (Vector3.Distance(transform.position, target.Value.position) <= StopDistance)
        {
            _controller.Stop();
            return TaskStatus.Success;
        }

        // 继续追击：持续驱动移动，返回 Running
        _controller.MoveTo(target.Value.position);
        return TaskStatus.Running;
    }
}
