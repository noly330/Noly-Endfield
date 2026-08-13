using Cysharp.Threading.Tasks;
using Endfield.Core;
using Endfield.Core.Pool;
using UnityEngine;

namespace Endfield.Module.Audio
{
    /// <summary>
    /// 音频服务（纯类单例）：从池借出音效实例播放。
    /// 不关心声音属于干员还是怪物——调用方传入 SoundData + 位置即可（DIP）。
    /// </summary>
    public class AudioService : Singleton<AudioService>
    {
        private const string SoundPrefabPath = "Assets/Res/Prefab/Audio/SoundItem.prefab";

        private PrefabPool<SoundItem> _pool;

        /// <summary>GameLauncher启动时初始化</summary>
        public async UniTask InitializeAsync()
        {
            _pool = await PrefabPoolManager.Instance.GetPoolAsync<SoundItem>(SoundPrefabPath, 15, 50);
        }

        /// <summary>在指定位置播放一个音效</summary>
        public void Play(SoundStyle style, SoundData soundData, Vector3 position, int index = -1)
        {
            if (_pool == null) return;
            var clip = index >= 0 ? soundData?.GetClip(style, index) : soundData?.GetClip(style);
            if (clip == null) return;

            var item = _pool.Get();
            item.BindPool(_pool);  //给这个物品绑定对象池
            item.Play(clip, position);
        }
    }
}
