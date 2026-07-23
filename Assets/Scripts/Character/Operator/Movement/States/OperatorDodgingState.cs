using UnityEngine;

namespace Endfield
{
    public class OperatorDodgingState : OperatorMovementState
    {
        private readonly float _dodgeCooldown = 1.5f;
        private float _cooldownTimer;

        public OperatorDodgingState(OperatorMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _operator.movementDriver.canDodge = false;
            _cooldownTimer = _dodgeCooldown;

            Vector3 inputDir = _operator.GetMovementInput();
            if (inputDir != Vector3.zero)
                _animator.CrossFadeInFixedTime("Dodge", 0.1f);
            else
                _animator.CrossFadeInFixedTime("DodgeBack", 0.1f);
            _animator.SetBool(AnimationID.HasInputID, true);
        }

        public override void Update()
        {
            base.Update();

            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0f)
                    _operator.ResetDodge();
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
