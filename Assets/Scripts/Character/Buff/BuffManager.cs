using System;
using System.Collections.Generic;

namespace Endfield
{
    /// <summary>
    /// buff 注册表（SRP：只负责增删叠层与到期清理；OCP：不感知任何具体效果）。
    /// 目标经 IBuffTarget 传给效果（LoD：只认识 owner 与 BuffRuntime）。
    /// </summary>
    public class BuffManager
    {
        private readonly IBuffTarget _owner;
        private readonly Dictionary<int, BuffRuntime> _buffs = new();
        private readonly List<int> _expired = new();

        public event Action<int> OnBuffAdded;
        public event Action<int> OnBuffRemoved;

        public BuffManager(IBuffTarget owner)
        {
            _owner = owner;
        }

        public void AddBuff(BuffDataSO data, int stackCount = 1)
        {
            if (data == null) return;
            if (_buffs.TryGetValue(data.buffId, out var buff))
            {
                int prev = buff.stackCount;
                int applied = buff.AddStack(stackCount);
                if (applied > 0)
                {
                    buff.FireStackChanged(_owner, prev, buff.stackCount);
                    OnBuffAdded?.Invoke(data.buffId);
                }
            }
            else
            {
                var newBuff = new BuffRuntime(data);
                _buffs[data.buffId] = newBuff;
                newBuff.FireApply(_owner);
                OnBuffAdded?.Invoke(data.buffId);
            }
        }

        public void RemoveBuff(int buffId)
        {
            if (_buffs.TryGetValue(buffId, out var buff))
            {
                _buffs.Remove(buffId);
                buff.FireRemove(_owner);
                OnBuffRemoved?.Invoke(buffId);
            }
        }

        public void RemoveAllBuffs()
        {
            if (_buffs.Count == 0) return;
            var ids = new List<int>(_buffs.Keys);
            foreach (var id in ids) RemoveBuff(id);
        }

        public bool HasBuff(int buffId) => _buffs.ContainsKey(buffId);
        public int GetStackCount(int buffId) => _buffs.TryGetValue(buffId, out var buff) ? buff.stackCount : 0;

        /// <summary>每帧驱动到期清理。</summary>
        public void Update()
        {
            if (_buffs.Count == 0) return;
            _expired.Clear();
            foreach (var kv in _buffs)
            {
                if (kv.Value.IsExpired) _expired.Add(kv.Key);
            }
            foreach (var id in _expired) RemoveBuff(id);
        }
    }
}
