using Global;
using UnityEngine;

namespace Endfield
{
    public class Character : CharacterMovementControlBase
    {
        [SerializeField] private float moveSpeed = 5f;

        public CharacterMovementStateMachine movementStateMachine { get; private set; }

        public virtual Vector3 GetMovementInput()
        {
            return Vector3.zero;
        }

        protected override void Awake()
        {
            base.Awake();
            movementStateMachine = new CharacterMovementStateMachine(this);
        }

        protected override void Start()
        {
            base.Start();
            movementStateMachine.ChangeState(movementStateMachine.idlingState);
        }

        protected override void Update()
        {
            base.Update();
            movementStateMachine.HandInput();
            movementStateMachine.Update();
        }

        public void OnAnimationTranslateEvent(OnEnterAnimationPlayerState playerState)
        {
            switch (playerState)
            {
                case OnEnterAnimationPlayerState.Dash:
                    movementStateMachine.OnAnimationTranslateEvent(movementStateMachine.dashingState);
                    break;
            }
        }

        public void OnAnimationExitEvent()
        {
            movementStateMachine.OnAnimationExitEvent();
        }
    }
}
