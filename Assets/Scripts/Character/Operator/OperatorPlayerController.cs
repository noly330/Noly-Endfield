using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endfield
{
    public class OperatorPlayerController : MonoBehaviour
    {
        public bool isMainPlayer;
        private Transform _cameraTransform;
        private OperatorMovementData _movementData;

        private Operator _operator;
        private void Awake()
        {
            _cameraTransform = Camera.main.transform;
            _operator = GetComponent<Operator>();
            _movementData = _operator.operatorSO.movementData;
        }

        private void OnEnable()
        {
            PlayerInputSystem.Instance.DashAction.performed += OnDashStart;
            PlayerInputSystem.Instance.AttackAction.performed += OnAttackStart;
            Debug.Log("角色名字："+name+"，玩家输入系统已启用");
        }



        private void OnDisable()
        {
            PlayerInputSystem.Instance.DashAction.performed -= OnDashStart;
            PlayerInputSystem.Instance.AttackAction.performed -= OnAttackStart;
        }

        private void Update()
        {
            UpdateMovementDriver();
        }

        private void OnDashStart(InputAction.CallbackContext context)
        {
            if(!isMainPlayer)
                return;

            if (!_operator.movementDriver.canDash)
                return;

            if(_operator.movementDriver.worldDirection == Vector3.zero)
                _operator.animator.CrossFadeInFixedTime(_movementData.dashData.backDushAnimationName, _movementData.dashData.fadeTime);
            else
                _operator.animator.CrossFadeInFixedTime(_movementData.dashData.frontDushAnimationName, _movementData.dashData.fadeTime);
        }
        private void OnAttackStart(InputAction.CallbackContext context)
        {
            if(!isMainPlayer)
                return;
            _operator.combatDriver.normalAttack = true;
        }
        public  void UpdateMovementDriver()
        {
            if (!isMainPlayer)
                return;

            Vector2 input = PlayerInputSystem.Instance.Move;

            if (input.sqrMagnitude < 0.01f)
            {
                _operator.movementDriver.worldDirection = Vector3.zero;
                return;
            }

            Vector3 dir = new Vector3(input.x, 0, input.y);
            Vector3 worldDir = _cameraTransform.TransformDirection(dir);
            worldDir.y = 0;
            _operator.movementDriver.worldDirection = worldDir.normalized;
        }

    }
}
