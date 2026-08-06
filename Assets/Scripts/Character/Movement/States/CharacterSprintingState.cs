using Endfield.Tools;
using UnityEngine;

namespace Endfield
{
    public class CharacterSprintingState : CharacterMovementState
    {
        private Vector3 _targetDirection;
        private float _turnDeltaAngle;
        private GameTimer _gameTimer;

        public CharacterSprintingState(CharacterMovementStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            _reusableData.rotationTime = _movementData.sprintData.rotationTime;
            _reusableData.inputMult = _movementData.sprintData.inputMult;

            _animator.SetBool(AnimationID.HasInputID, true);
        }

        public override void Exit()
        {
            base.Exit();
            if (_gameTimer != null)
            {
                TimerManager.Instance.UnregisterTimer(_gameTimer);
                _gameTimer = null;
            }
        }

        public override void Update()
        {
            base.Update();
            _targetDirection = Quaternion.Euler(0, _reusableData.targetAngle, 0) * Vector3.forward;
            _turnDeltaAngle = TransformUtility.GetDeltaAngle(_character.transform, _targetDirection);

            if (Mathf.Abs(_turnDeltaAngle) > _movementData.sprintData.turnBackAngle)
            {
                _animator.SetBool(AnimationID.TurnBackID, true);
            }

            if (_gameTimer == null && _character.GetMovementInput() == UnityEngine.Vector3.zero)
            {
                _gameTimer = TimerManager.Instance.GetTimer(0.1f, OnBufferToIdle);
            }
            else if (_gameTimer != null && _character.GetMovementInput() != UnityEngine.Vector3.zero)
            {
                TimerManager.Instance.UnregisterTimer(_gameTimer);
                _gameTimer = null;
            }
        }

        private void OnBufferToIdle()
        {
            _movementStateMachine.ChangeState(CharacterMovementStateType.Idle);
        }
    }
}
