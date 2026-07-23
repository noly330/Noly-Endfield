using UnityEngine;

namespace Endfield
{
    public class OperatorInputController : Operator
    {
        public bool isMainPlayer;

        private Transform _cameraTransform;

        protected override void Awake()
        {
            base.Awake();
            _cameraTransform = Camera.main.transform;
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
