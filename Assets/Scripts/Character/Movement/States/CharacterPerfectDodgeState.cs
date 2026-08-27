using UnityEngine;
using Endfield.Core;
namespace Endfield
{
    /// <summary>
    /// 完美闪避状态：闪避（Dodging）中被直接攻击触发。进入时切角色状态为 PerfectDodging + 全局减速 0.2×0.3s。
    /// 用真实时间（unscaled）计时，减速不影响时长；结束后回 Idle/Run 并切回 Normal。
    /// TODO: 完美闪避动画/特效（需要时加）。
    /// </summary>
    public class CharacterPerfectDodgeState : CharacterMovementState
    {
        private float _timer;
        private const float _duration = 0.8f;

        public CharacterPerfectDodgeState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _timer = _duration;
            _character.SetState(CharacterState.PerfectDodging);
            TimeDirector.SlowTo(0.2f, _duration);   // 全局减速到 0.2，持续 0.3s
            _character.PlayPerfectDodgeVisual(_duration);   // 蓝白轮廓 + 残影
        }

        public override void Update()
        {
            base.Update();
            _timer -= Time.unscaledDeltaTime;
            if (_timer <= 0f)
            {
                _movementStateMachine.ChangeState(
                    _character.GetMovementInput() != Vector3.zero
                        ? CharacterMovementStateType.Run
                        : CharacterMovementStateType.Idle);
            }
        }

        public override void Exit()
        {
            base.Exit();
            _character.SetState(CharacterState.Normal);
        }
    }
}
