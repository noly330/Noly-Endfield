using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Endfield.Core.Pool;
using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// 完美闪避表现：蓝白轮廓（贴体 rim ghost 每帧刷新）+ 残影（间隔生成、淡出回池）。
    /// 由 CharacterPerfectDodgeState 触发 Play(duration)；SRP：只管表现，不碰角色状态/伤害。
    /// </summary>
    public class PerfectDodgeVisual : MonoBehaviour
    {
        [Header("素材")]
        [SerializeField] private string _ghostPrefabAddress;   // ghost prefab 的 Addressable 地址（空则用默认路径）

        /// <summary>默认 ghost prefab 地址（没填 _ghostPrefabAddress 时用）。</summary>
        private const string _defaultGhostPrefabAddress = "Assets/Res/VFX/PerfectDodgeGhost/PerfectDodgeGhost.prefab";
        private string GhostPrefabAddress =>
            string.IsNullOrEmpty(_ghostPrefabAddress) ? _defaultGhostPrefabAddress : _ghostPrefabAddress;

        [Header("轮廓")]
        [SerializeField] private float _rimAlpha = 0.5f;

        [Header("残影")]
        [SerializeField] private float _afterimageAlpha = 0.35f;
        [SerializeField] private float _afterimageInterval = 0.1f;
        [SerializeField] private float _afterimageFade = 0.2f;   // 快速消失

        private SkinnedMeshRenderer[] _renderers;
        private PrefabPool<PerfectDodgeGhost> _ghostPool;
        private readonly List<PerfectDodgeGhost> _rimGhosts = new();
        private readonly List<AfterimageGroup> _afterimages = new();
        private bool _active;
        private float _timer;
        private float _afterimageTimer;

        private class AfterimageGroup
        {
            public readonly List<PerfectDodgeGhost> ghosts = new();
            public float remaining;
        }

        private void Awake()
        {
            _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        /// <summary>开始完美闪避表现：轮廓持续 duration，期间按节奏生成残影。</summary>
        public void Play(float duration)
        {
            if (_renderers.Length == 0) return;
            _active = true;
            _timer = duration;
            _afterimageTimer = 0f;
            EnsurePoolAndRimAsync().Forget();
        }

        private async UniTask EnsurePoolAndRimAsync()
        {
            if (_ghostPool == null)
            {
                var pool = await PrefabPoolManager.Instance.GetPoolAsync<PerfectDodgeGhost>(GhostPrefabAddress);
                if (pool == null) { _active = false; return; }
                _ghostPool = pool;
            }
            // 轮廓 ghost：每个 renderer 一个（贴体，每帧刷新网格）
            _rimGhosts.Clear();
            for (int i = 0; i < _renderers.Length; i++)
                _rimGhosts.Add(_ghostPool.Get());
        }

        private void Update()
        {
            if (_active)
            {
                _timer -= Time.unscaledDeltaTime;

                if (_rimGhosts.Count == _renderers.Length)
                    RefreshRim();

                _afterimageTimer -= Time.unscaledDeltaTime;
                if (_afterimageTimer <= 0f && _ghostPool != null)
                {
                    SpawnAfterimage();
                    _afterimageTimer = _afterimageInterval;
                }

                if (_timer <= 0f) Stop();
            }

            // 残影持续淡出回池，即使完美闪避窗口已结束（否则残影卡住停留）
            FadeAfterimages();
        }

        /// <summary>轮廓：每帧把当前姿势 BakeMesh 到贴体 ghost，保持蓝白 rim 覆盖角色。</summary>
        private void RefreshRim()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].BakeMesh(_rimGhosts[i].GetMesh(), true);
                _rimGhosts[i].Show(_renderers[i].transform.position, _renderers[i].transform.rotation, _rimAlpha);
            }
        }

        private void SpawnAfterimage()
        {
            var group = new AfterimageGroup { remaining = _afterimageFade };
            for (int i = 0; i < _renderers.Length; i++)
            {
                var ghost = _ghostPool.Get();
                _renderers[i].BakeMesh(ghost.GetMesh(), true);
                ghost.Show(_renderers[i].transform.position, _renderers[i].transform.rotation, _afterimageAlpha);
                group.ghosts.Add(ghost);
            }
            _afterimages.Add(group);
        }

        private void FadeAfterimages()
        {
            for (int g = _afterimages.Count - 1; g >= 0; g--)
            {
                var group = _afterimages[g];
                group.remaining -= Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(group.remaining / _afterimageFade) * _afterimageAlpha;
                foreach (var ghost in group.ghosts) ghost.SetAlpha(alpha);
                if (group.remaining <= 0f)
                {
                    foreach (var ghost in group.ghosts) _ghostPool.Release(ghost);
                    _afterimages.RemoveAt(g);
                }
            }
        }

        private void Stop()
        {
            _active = false;
            foreach (var ghost in _rimGhosts) _ghostPool.Release(ghost);
            _rimGhosts.Clear();
        }
    }
}
