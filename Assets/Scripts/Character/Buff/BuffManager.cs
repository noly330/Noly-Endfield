using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    /// <summary>
    /// buff 管理器（挂在目标身上）：挂配置 → 建实例 → 按策略叠层 → 每帧 tick → 到期移除。
    /// 不感知任何具体效果（OCP）；目标经 IBuffTarget 传给效果（LoD）。
    /// </summary>
    public class BuffManager
    {
        private readonly IBuffTarget _owner;
        private readonly Dictionary<int, BuffInstance> _buffs = new();
        private readonly List<int> _expired = new();

        public event Action<int> OnBuffAdded;    // 供 UI / 元素反应等监听
        public event Action<int> OnBuffRemoved;

        public BuffManager(IBuffTarget owner)
        {
            _owner = owner;
        }

        /// <summary>挂 buff（可指定初始层数，碎甲按消耗破防层数挂时用）。</summary>
        public void Apply(BuffConfig config, Character caster = null, int layer = 1)
        {
            if (config == null) return;
            //如果当前身上有这个 buff，根据策略叠层
            if (_buffs.TryGetValue(config.buffId, out var existing))
            {
                int oldStackCount = existing.stackCount;
                switch (existing.config.stackingMode)
                {
                    case BuffStackingMode.Ignore:
                        return;
                    case BuffStackingMode.Refresh:
                        existing.remainingTime = config.duration;
                        break;
                    case BuffStackingMode.Replace:
                        existing.stackCount = Mathf.Clamp(layer, 1, config.maxStacks);
                        existing.remainingTime = config.duration;
                        break;
                    case BuffStackingMode.Stack:
                    default:
                        existing.stackCount = Mathf.Min(existing.stackCount + layer, config.maxStacks);
                        existing.remainingTime = config.duration;
                        break;
                }
                if (oldStackCount != existing.stackCount)
                {
                    existing.StackChangedEffects(_owner, existing.stackCount - oldStackCount);
                }
            }
            else  //否则创建buff实例，添加到字典
            {
                var instance = new BuffInstance
                {
                    config = config,
                    remainingTime = config.duration,
                    stackCount = Mathf.Clamp(layer, 1, config.maxStacks),
                    caster = caster,
                };
                _buffs[config.buffId] = instance;
                instance.ApplyEffects(_owner);
            }
            OnBuffAdded?.Invoke(config.buffId);
        }

        public void Remove(int buffId)
        {
            if (_buffs.TryGetValue(buffId, out var instance))
            {
                _buffs.Remove(buffId);
                instance.RemoveEffects(_owner);
                OnBuffRemoved?.Invoke(buffId);
            }
        }

        public void RemoveAll()
        {
            if (_buffs.Count == 0) return;
            var ids = new List<int>(_buffs.Keys);
            foreach (var id in ids) Remove(id);
        }

        public bool Has(int buffId) => _buffs.ContainsKey(buffId);
        public int GetStack(int buffId) => _buffs.TryGetValue(buffId, out var i) ? i.stackCount : 0;

        /// <summary>每帧：倒计时 + 效果 tick + 到期移除。</summary>
        public void Update()
        {
            if (_buffs.Count == 0) return;
            _expired.Clear();
            foreach (var kv in _buffs)
            {
                var instance = kv.Value;
                if (!instance.IsPermanent)
                {
                    instance.remainingTime -= Time.deltaTime;
                    if (instance.remainingTime < 0f) instance.remainingTime = 0f;
                }
                instance.TickEffects(_owner, Time.deltaTime);
                if (instance.IsExpired) _expired.Add(kv.Key);
            }
            foreach (var id in _expired) Remove(id);
        }
    }
}
