using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Endfield.Core
{
    /// <summary>
    /// 全局时间控制（时间减速）：连携慢动作、完美闪避共用。
    /// SlowTo(scale, duration)：把 timeScale 缩到 scale，持续 duration 真实秒后恢复 1。
    /// 用 unscaled 计时，减速本身不影响时长。
    /// </summary>
    public static class TimeDirector
    {
        public static void SlowTo(float scale, float duration) => SlowToAsync(scale, duration).Forget();

        private static async UniTaskVoid SlowToAsync(float scale, float duration)
        {
            Time.timeScale = scale;
            await UniTask.Delay((int)(duration * 1000f), DelayType.UnscaledDeltaTime);
            Time.timeScale = 1f;
        }
    }
}
