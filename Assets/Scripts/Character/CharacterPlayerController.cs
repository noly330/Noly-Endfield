using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endfield
{
    public class CharacterPlayerController : MonoBehaviour
    {
        private Transform _cameraTransform;
        private CharacterMovementData _movementData;

        private Character _character;
        private void Awake()
        {
            _cameraTransform = Camera.main.transform;
            _character = GetComponent<Character>();
            _movementData = _character.MovementData;
        }

        private void OnEnable()
        {
            if (PlayerInputSystem.Instance == null)
                return;

            PlayerInputSystem.Instance.DashAction.performed += OnDashStart;
            PlayerInputSystem.Instance.AttackAction.performed += OnAttackStart;
        }

        private void OnDisable()
        {
            if (PlayerInputSystem.Instance == null)
                return;

            PlayerInputSystem.Instance.DashAction.performed -= OnDashStart;
            PlayerInputSystem.Instance.AttackAction.performed -= OnAttackStart;
        }

        private void Update()
        {
            UpdateMovementDriver();
        }

        private void OnDashStart(InputAction.CallbackContext context)
        {
            if (!_character.movementDriver.canDash)
                return;

            if (_character.movementDriver.worldDirection == Vector3.zero)
                _character.animator.CrossFadeInFixedTime(_movementData.dashData.backDushAnimationName, _movementData.dashData.fadeTime);
            else
                _character.animator.CrossFadeInFixedTime(_movementData.dashData.frontDushAnimationName, _movementData.dashData.fadeTime);
        }
        private void OnAttackStart(InputAction.CallbackContext context)
        {
            _character.combatDriver.normalAttack = true;
        }
        public void UpdateMovementDriver()
        {
            Vector2 input = PlayerInputSystem.Instance.Move;

            if (input.sqrMagnitude < 0.01f)
            {
                _character.movementDriver.worldDirection = Vector3.zero;
                return;
            }

            Vector3 dir = new Vector3(input.x, 0, input.y);
            Vector3 worldDir = _cameraTransform.TransformDirection(dir);
            worldDir.y = 0;
            _character.movementDriver.worldDirection = worldDir.normalized;
        }
    }
}
