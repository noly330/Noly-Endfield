using BehaviorDesigner.Runtime.Tasks;
using Endfield;
using UnityEngine;

[TaskCategory("Character")]
public class PlayerDistance : Conditional
{
    public enum CompareMode
    {
        Less,
        Greater,
    }

    [BehaviorDesigner.Runtime.Tasks.Tooltip("与主控的距离比较模式")]
    [SerializeField] private CompareMode _mode = CompareMode.Greater;

    [BehaviorDesigner.Runtime.Tasks.Tooltip("距离阈值")]
    [SerializeField] private float _distance = 5f;

    private CharacterAIController _controller;

    public override void OnAwake() => _controller = GetComponent<CharacterAIController>();

    public override TaskStatus OnUpdate()
    {
        var player = TeamManager.Instance != null ? TeamManager.Instance.ActiveOperator : null;
        if (player == null)
            return TaskStatus.Failure;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool pass = _mode == CompareMode.Less ? distance < _distance : distance > _distance;
        return pass ? TaskStatus.Success : TaskStatus.Failure;
    }
}
