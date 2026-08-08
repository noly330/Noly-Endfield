using UnityEngine;
using Endfield.Tools;

namespace Endfield
{
    public class CharacterDashingState : CharacterMovementState
    {
        public CharacterDashingState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _reusableData.rotationTime = _movementData.dashData.rotationTime;
            _character.movementDriver.canDash = false;
            _animator.SetBool(AnimationID.HasInputID, true);
            //TimerManager.Instance.GetOneTimer(_movementData.dashData.coldTime, _character.ResetDash);
        }

        public override void Update()
        {
            base.Update();
            // 冲刺动画真正结束后才退出。不依赖 OnAnimationExitEvent：
            // 该事件由任意带 behaviour 的状态退出时触发，冲刺打断跑步/攻击等动画时会被源状态的退出事件误触发提前退出。
            if (_animator.IsInTransition(0)) return;
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(_movementData.dashData.frontDushAnimationName)) return;
            if (info.IsName(_movementData.dashData.backDushAnimationName)) return;

            if (_character.GetMovementInput() != Vector3.zero)
                _movementStateMachine.ChangeState(CharacterMovementStateType.Sprint);
            else
                _movementStateMachine.ChangeState(CharacterMovementStateType.Idle);
        }

        public override void Exit()
        {
            base.Exit();
            _character.movementDriver.canDash = true;
        }
    }
}
