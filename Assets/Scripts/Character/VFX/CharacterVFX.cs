using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Endfield.Core.Pool;
using Endfield.Module.VFX;
using Unity.VisualScripting;
using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 角色身上的特效入口：动画剪辑的帧事件直接调 PlayVFX("名字")。
    /// Awake 时把 name → {prefabPath, 容器} 一次性解析缓存，播放时 O(1) 查找、直接拿容器 Transform。
    /// 池操作内联（目前唯一调用方，避免为一个消费者抽服务）。
    /// </summary>
    public class CharacterVFX : MonoBehaviour
    {
        [SerializeField] private VFXConfigSO _config;
        [SerializeField] private string _hitVFXName;
        private readonly Dictionary<string, (string prefabPath, Transform anchor)> _vfxMap = new();

        private void Awake()
        {
            if (_config == null) return;
            foreach (var data in _config.vfxDatas)
                _vfxMap[data.name] = (data.prefabPath, FindAnchor(transform, data.anchorName));
        }
        private void OnEnable()
        {
            var health = GetComponent<CharacterHealth>();
            if(health != null)
            {
                health.OnDamaged += OnDamaged;
            }
        }
        private void OnDisable() {
            var health = GetComponent<CharacterHealth>();
            if(health != null)
            {
                health.OnDamaged -= OnDamaged;
            }
        }

        private void OnDamaged(DamageInfo info)
        {
            if(string.IsNullOrEmpty(_hitVFXName) == false)
            {
                PlayVFX(_hitVFXName);
            }
        }


        public void PlayVFX(string vfxName)
        {
            if (!_vfxMap.TryGetValue(vfxName, out var entry) || entry.anchor == null)
            {
                Debug.LogWarning($"[CharacterVFX] 找不到特效容器: {vfxName}");
                return;
            }
            PlayVFXAsync(entry.prefabPath, entry.anchor).Forget();
        }

        private async UniTask PlayVFXAsync(string prefabPath, Transform anchor)
        {
            var pool = await PrefabPoolManager.Instance.GetPoolAsync<PooledVFX>(prefabPath);
            var vfx = pool.Get();                       // SetActive(true)
            vfx.transform.SetParent(anchor, false);     // 挂到容器，跟随容器（位置/角度已由容器决定）
            vfx.transform.localPosition = Vector3.zero;
            vfx.transform.localRotation = Quaternion.identity;
            vfx.transform.localScale = Vector3.one;
            vfx.Play();

            await UniTask.Delay(TimeSpan.FromSeconds(vfx.Duration));
            pool.Release(vfx);                          // 播完回池（SetActive false）
        }

        /// <summary>含 "/" → 层级路径找；否则递归按名字找（对骨骼层级更稳）。</summary>
        private static Transform FindAnchor(Transform root, string anchorName) =>
            anchorName.Contains('/') ? root.Find(anchorName) : FindDeep(root, anchorName);

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform result = FindDeep(child, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
