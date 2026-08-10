using UnityEngine;

namespace Endfield.Module.VFX
{
    /// <summary>挂特效 prefab 根：播粒子 / 隐藏清粒子 / 暴露时长供回收。</summary>
    public class PooledVFX : MonoBehaviour
    {
        private ParticleSystem[] _particles;   // 存这个特效下所有的粒子系统组件
        private float _duration;  // 存这个特效播放完需要多少秒

        private void Awake()
        {
            _particles = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var p in _particles)
                _duration = Mathf.Max(_duration, p.main.duration);  //取所有粒子系统中耗时最长的
        }

        public float Duration => _duration;

        public void Play()
        {
            foreach (var p in _particles) p.Play();  // 播放所有粒子系统
        }

        private void OnDisable()
        {
            foreach (var p in _particles)
                p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);  // 停止所有粒子系统并清空粒子
        }
    }
}