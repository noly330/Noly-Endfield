using UnityEngine;

namespace Endfield
{
    public class OperatorDashingState : OperatorMovementState
    {
        private readonly float _dashCooldown = 1.5f;
        private float _cooldownTimer;

        public OperatorDashingState(OperatorMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _operator.movementDriver.canDash = false;
            _cooldownTimer = _dashCooldown;

            Vector3 inputDir = _operator.GetMovementInput();
            if (inputDir != Vector3.zero)
                _animator.CrossFadeInFixedTime("Dash", 0.1f);
            else
                _animator.CrossFadeInFixedTime("DashBack", 0.1f);
            _animator.SetBool(AnimationID.HasInputID, true);
        }

        public override void Update()
        {
            base.Update();

            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0f)
                    _operator.ResetDash();
            }
        }

        public override void OnAnimationExitEvent()
        {
            if (_operator.GetMovementInput() != Vector3.zero)
                _movementStateMachine.ChangeState(_movementStateMachine.runningState);
            else
                _movementStateMachine.ChangeState(_movementStateMachine.idlingState);
        }
    }
}
