using Endfield.Core.Pool;
using UnityEngine;

namespace Endfield.Module.Audio
{
    /// <summary>音效实例,也就是通用的音频播放器，配合对象池管理。</summary>
    public class SoundItem : MonoBehaviour
    {
        private AudioSource _audioSource;
        private PrefabPool<SoundItem> _pool;
        private bool _startedPlaying;

        private void Awake() => _audioSource = GetComponent<AudioSource>();

        /// <summary>绑定所属池。</summary>
        public void BindPool(PrefabPool<SoundItem> pool) => _pool = pool;

        /// <summary>播放指定音效并定位。</summary>
        public void Play(AudioClip clip, Vector3 position)
        {
            transform.position = position;
            _audioSource.clip = clip;
            _audioSource.Play();
            _startedPlaying = true;
        }

        private void Update()
        {
            // 播完自回归；_startedPlaying 防止"激活未播"被误回收
            if (_startedPlaying && _audioSource != null && !_audioSource.isPlaying)
            {
                _startedPlaying = false;
                _pool?.Release(this);
            }
        }
    }
}