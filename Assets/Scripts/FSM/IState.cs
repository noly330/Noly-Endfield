namespace Endfield
{


    public interface IState
    {
        /// <summary>进入状态时调用一次，用于初始化参数、播放动画、注册输入回调</summary>
        public void Enter();

        /// <summary>离开状态时调用一次，用于清理参数、注销输入回调</summary>
        public void Exit();

        /// <summary>每帧调用，用于处理非物理逻辑（旋转、动画参数更新、状态切换判断）</summary>
        public void Update();

        /// <summary>每帧调用，用于处理输入相关的逻辑</summary>
        public void HandInput();

        /// <summary>Animator 进入某个动画状态时由 StateMachineBehaviour 触发，用于动画驱动状态切换</summary>
        public void OnAnimationTranslateEvent(IState state);

        /// <summary>Animator 离开某个动画状态时由 StateMachineBehaviour 触发</summary>
        public void OnAnimationExitEvent();
    }

}
