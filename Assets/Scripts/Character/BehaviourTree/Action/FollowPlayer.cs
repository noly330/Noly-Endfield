using BehaviorDesigner.Runtime.Tasks;
using Endfield;
using UnityEngine;

[TaskCategory("Character")]
public class FollowPlayer : Action
{
    [UnityEngine.Tooltip("跟随停止距离：干员与站位点距离小于该值视为到位并停止")]
    [SerializeField] private float _stopDistance = 1.5f;

    [UnityEngine.Tooltip("站位方向数量（围绕主控一周，默认 8 个方向）")]
    [SerializeField] private int _directionCount = 8;

    [UnityEngine.Tooltip("站位最小半径（离主控的距离）")]
    [SerializeField] private float _minRadius = 2.5f;

    [UnityEngine.Tooltip("站位最大半径")]
    [SerializeField] private float _maxRadius = 4.5f;

    private CharacterAIController _controller;
    private int _dirIndex = -1;   // 本次追随选定的方向索引，-1 = 未选

    public override void OnAwake() => _controller = GetComponent<CharacterAIController>();

    /// <summary>每次 FollowPlayer 被激活时重置，重新随机一个方向，避免追随过程每帧重摇抖动。</summary>
    public override void OnStart() => _dirIndex = -1;

    public override TaskStatus OnUpdate()
    {
        var player = TeamManager.Instance != null ? TeamManager.Instance.ActiveOperator : null;
        if (player == null)
        {
            _controller.Stop();
            return TaskStatus.Failure;
        }

        Vector3 anchor = GetFormationAnchor(player.transform);
        float distance = Vector3.Distance(transform.position, anchor);
        if (distance <= _stopDistance)
        {
            _controller.Stop();
            return TaskStatus.Success;
        }

        _controller.MoveTo(anchor);
        return TaskStatus.Running;
    }

    /// <summary>在八个方向中随机选一个，加轻微角度/半径扰动，返回主控周围该方向的落点。</summary>
    private Vector3 GetFormationAnchor(Transform playerTransform)
    {
        if (_dirIndex < 0)
            _dirIndex = Random.Range(0, _directionCount);

        float angle = 360f / _directionCount * (_dirIndex + Random.Range(-0.3f, 0.3f));   // 轻微扰动，避免并排
        float radius = Random.Range(_minRadius, _maxRadius);

        Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        dir.y = 0f;

        return playerTransform.position + dir * radius;
    }
}
