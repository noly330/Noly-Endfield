using UnityEngine;

namespace Endfield
{
    public class Operator : Character
    {
        private Transform cameraTransform;

        protected override void Awake()
        {
            base.Awake();
            cameraTransform = Camera.main.transform;
        }

        public override Vector3 GetMovementInput()
        {
            Vector2 input = PlayerInputSystem.Instance.Move;

            if (input.sqrMagnitude < 0.01f)
                return Vector3.zero;

            Vector3 dir = new Vector3(input.x, 0, input.y);
            Vector3 worldDir = cameraTransform.TransformDirection(dir);
            worldDir.y = 0;
            return worldDir.normalized;
        }
    }
}
