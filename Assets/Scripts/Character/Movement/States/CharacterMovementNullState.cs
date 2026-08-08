using UnityEngine;
using Endfield.Tools;

namespace Endfield
{
    /// <summary>
    /// 移动空状态：攻击等动作期间锁定移动（不响应输入做 locomotion），
    /// 仅攻击前段允许朝输入方向转向（攻击目标吸附 FaceTarget 优先级更高）。
    /// 参考 ZZZ demo 的 PlayerMovementNullState。
    /// </summary>
    public class CharacterMovementNullState : CharacterMovementState
    {
        public CharacterMovementNullState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _reusableData.rotationTime = _movementData.comboRotationTime;
        }

        public override void HandInput()
        {
            // 锁定移动：攻击期间不响应输入驱动 Movement 动画参数
            _animator.SetFloat(AnimationID.MovementID, 0f, 0.35f, Time.deltaTime);
        }

        public override void Update()
        {
            //啥也不做，算是停止父方法的转向了
        }

        public override void OnAnimationExitEvent()
        {
            //TimerManager.Instance.GetTimer(0.2f, CheckStateExit);
            CheckStateExit();
        }

        private void CheckStateExit()
        {
            // 已经切走（如闪避/回移动）则跳过，避免打断
            if (_movementStateMachine.currentState.Value != this) return;
            if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("ATK")) return;
            if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit")) return;

            if (_character.GetMovementInput() != Vector3.zero)
                _movementStateMachine.ChangeState(CharacterMovementStateType.Run);
            else
                _movementStateMachine.ChangeState(CharacterMovementStateType.Idle);
        }
    }
}
