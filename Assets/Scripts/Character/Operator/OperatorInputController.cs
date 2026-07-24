using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endfield
{
    public class OperatorInputController : Operator
    {
        public bool isMainPlayer;

        private Transform _cameraTransform;
        private OperatorMovementData _movementData;

        protected override void Awake()
        {
            base.Awake();
            _cameraTransform = Camera.main.transform;
            _movementData = operatorSO.movementData;
        }

        private void OnEnable()
        {
            PlayerInputSystem.Instance.DashAction.performed += OnDashStart;
        }

        private void OnDashStart(InputAction.CallbackContext context)
        {
            if(!isMainPlayer)
                return;

            //TODO:后面加上计时器，搞冲刺冷却
            // if (!movementDriver.canDash)
            //     return;
            if(movementDriver.worldDirection == Vector3.zero)
                _animator.CrossFadeInFixedTime(_movementData.dashData.backDushAnimationName, _movementData.dashData.fadeTime);
            else
                _animator.CrossFadeInFixedTime(_movementData.dashData.frontDushAnimationName, _movementData.dashData.fadeTime);
        }
        public override void UpdateMovementDriver()
        {
            if (!isMainPlayer)
                return;

            Vector2 input = PlayerInputSystem.Instance.Move;

            if (input.sqrMagnitude < 0.01f)
            {
                movementDriver.worldDirection = Vector3.zero;
                return;
            }

            Vector3 dir = new Vector3(input.x, 0, input.y);
            Vector3 worldDir = _cameraTransform.TransformDirection(dir);
            worldDir.y = 0;
            movementDriver.worldDirection = worldDir.normalized;
        }

    }
}
