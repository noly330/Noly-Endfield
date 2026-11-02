using UnityEngine;

namespace Endfield
{
    public class CharacterPlayerController : MonoBehaviour
    {
        private Transform _cameraTransform;
        private CharacterMovementData _movementData;

        private Character _character;
        private IInputSource _input;
        private void Awake()
        {
            _cameraTransform = Camera.main.transform;
            _character = GetComponent<Character>();
            _movementData = _character.MovementData;
        }

        private void OnEnable()
        {
            _input ??= InputService.Current;
            if (_input == null)
                return;

            _input.OnDash += OnDashStart;
            _input.OnAttack += OnAttackStart;
        }

        private void OnDisable()
        {
            if (_input == null)
                return;

            _input.OnDash -= OnDashStart;
            _input.OnAttack -= OnAttackStart;
        }

        private void Update()
        {
            UpdateMovementDriver();
        }

        private void OnDashStart()
        {
            if (!_character.movementDriver.canDash)
                return;

            if (_character.movementDriver.worldDirection == Vector3.zero)
                _character.animator.CrossFadeInFixedTime(_movementData.dashData.backDushAnimationName, _movementData.dashData.fadeTime);
            else
                _character.animator.CrossFadeInFixedTime(_movementData.dashData.frontDushAnimationName, _movementData.dashData.fadeTime);
        }
        private void OnAttackStart()
        {
            _character.combatDriver.normalAttack = true;
        }
        public void UpdateMovementDriver()
        {
            if (_input == null)
            {
                _character.movementDriver.worldDirection = Vector3.zero;
                return;
            }
            Vector2 input = _input.Move;

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
