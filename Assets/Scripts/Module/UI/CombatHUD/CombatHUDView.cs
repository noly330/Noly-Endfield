using Cysharp.Threading.Tasks;
using Endfield.Core;
using UnityEngine;

namespace Endfield.Module.UI
{
    /// <summary>
    /// 战斗 HUD 界面（BaseView，常驻，Top/Middle 层）。
    /// 容器：现在托管连携技照片横幅（LinkBannerWidget）；
    /// 以后血量条 / 能量条 / 干员状态卡等战斗 HUD 元素都作为它的子元素或 Widget 扩展。
    /// </summary>
    public class CombatHUDView : BaseView
    {
        protected override async UniTask OnInit(Transform root)
        {
            await base.OnInit(root);

            var bannerContainer = GetComponent<RectTransform>("Main/LinkBannerContainer");
            if (bannerContainer != null)
                await SetWidget<LinkBannerWidget>(UIRegister.LinkBannerWidget, bannerContainer);

            var queueContainer = GetComponent<RectTransform>("Main/LinkQueueContainer");
            if (queueContainer != null)
                await SetWidget<LinkQueueWidget>(UIRegister.LinkQueueWidget, queueContainer);
        }
    }
}
