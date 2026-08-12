using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{

    public class OnAnimationTranslation : StateMachineBehaviour
    {
        [SerializeField] private OnEnterAnimationState _enterAnimationState;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_enterAnimationState == OnEnterAnimationState.None)
                return;
            if (animator.TryGetComponent<Character>(out Character characterComponent))
            {
                characterComponent.OnAnimationTranslate(_enterAnimationState);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_enterAnimationState == OnEnterAnimationState.None)
                return;
            if (animator.TryGetComponent<Character>(out Character characterComponent))
            {
                characterComponent.OnAnimationEixt();
            }
        }
    }

    public enum OnEnterAnimationState
    {
        None,
        Idle,
        Walk,
        Run,
        Dash,
        Sprint,
        ReturnRun,
        ATK,
        Skill,
        Hit,

    }
}
