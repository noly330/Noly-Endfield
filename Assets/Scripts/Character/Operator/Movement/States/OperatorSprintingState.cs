using Endfield.Tools;
using UnityEngine;


namespace Endfield
{
    public class OperatorSprintingState : OperatorMovementStateBase
    {
        private Vector3 _targetDirection;
        private float _turnDeltaAngle;
        public OperatorSprintingState(OperatorMovementStateMachine stateMachine) : base(stateMachine){}

        public override void Enter()
        {
            base.Enter();
            _reusableData.rotationTime = _movementData.sprintData.rotationTime;
            _reusableData.inputMult = _movementData.sprintData.inputMult;

            _animator.SetBool(AnimationID.HasInputID, true);
        }

        public override void Update()
        {
            base.Update();
            _targetDirection = Quaternion.Euler(0,_reusableData.targetAngle,0) * Vector3.forward;
            _turnDeltaAngle = TransformUtility.GetDeltaAngle(_operator.transform, _targetDirection);

            if (Mathf.Abs(_turnDeltaAngle) > _movementData.sprintData.turnBackAngle)
            {
                _animator.SetBool(AnimationID.TurnBackID, true);
              
            }


            if (_operator.GetMovementInput() == UnityEngine.Vector3.zero)
            {
                OnBufferToIdle();
                return;
            }
            _bufferTimer = _bufferTime;
        
        }

        //TODO:未来加上计时器，这个先用硬编码缓冲，等有时间再优化
        private float _bufferTime = 0.1f;
        private float _bufferTimer;
        private void OnBufferToIdle()
        {
            _bufferTimer -= Time.deltaTime;
            if (_bufferTimer <= 0)
            {
                _movementStateMachine.ChangeState(_movementStateMachine.idlingState);
            }
        }

    }
}