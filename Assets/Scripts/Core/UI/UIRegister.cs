using System.Collections.Generic;
using Endfield.Module;

namespace Endfield.Core
{
    public static class UIRegister
    {
        public static readonly ViewEntry MainView = new(typeof(MainView), "Assets/Res/Prefab/MainView/MainView.prefab");

        public static IReadOnlyList<ViewEntry> All{get;} = new[]
        {
            MainView,
        };
    }
}