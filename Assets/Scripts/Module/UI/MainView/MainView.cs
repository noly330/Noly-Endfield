using Endfield.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Endfield.Module.UI
{
    /// <summary>
    /// 主界面占位视图。
    /// 当前 UI 尚未制作，仅用于让 UIRegister 编译通过。
    /// UI 制作完成后在 OnInit 中缓存组件、绑定事件。
    /// </summary>
    public class MainView : BaseView
    {
        protected override UniTask OnInit(Transform root)
        {
            base.OnInit(root);
            return UniTask.CompletedTask;
        }
    }
}
