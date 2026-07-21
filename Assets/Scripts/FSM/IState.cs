using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{


    public interface IState
    {
        public void Enter();
        public void Exit();
        public void Update();
        public void HandInput();
        public void OnAnimationTranslateEvent(IState state);
        public void OnAnimationExitEvent();
    }

}
