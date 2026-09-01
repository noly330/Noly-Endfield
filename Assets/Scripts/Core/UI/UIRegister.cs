using System.Collections.Generic;
using Endfield.Module.UI;

namespace Endfield.Core
{
    public static class UIRegister
    {
        public static readonly ViewEntry MainView = new(typeof(MainView), "Assets/Res/Prefab/UI/MainView/MainView.prefab");
		public static readonly ViewEntry TopToolBarView = new(typeof(TopToolBarView), "Assets/Res/Prefab/UI/TopToolBar/TopToolBarView.prefab");
		public static readonly ViewEntry OperatorDisplayView = new(typeof(OperatorDisplayView), "Assets/Res/Prefab/UI/OperatorDisplay/OperatorDisplayView.prefab");
		public static readonly ViewEntry CombatHUDView = new(typeof(CombatHUDView), "Assets/Res/Prefab/UI/CombatHUDView/CombatHUDView.prefab");
		public static readonly ViewEntry LinkBannerWidget = new(typeof(LinkBannerWidget), "Assets/Res/Prefab/UI/CombatHUDView/LinkBannerWidget.prefab");
		public static readonly ViewEntry LinkQueueWidget = new(typeof(LinkQueueWidget), "Assets/Res/Prefab/UI/CombatHUDView/LinkQueueWidget.prefab");

        public static IReadOnlyList<ViewEntry> All{get;} = new[]
        {
            MainView,
			TopToolBarView,
			OperatorDisplayView,
			CombatHUDView,
			LinkBannerWidget,
			LinkQueueWidget,
        };
    }
}
