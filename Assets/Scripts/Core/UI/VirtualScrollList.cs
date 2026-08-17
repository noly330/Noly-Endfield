using System;
using System.Collections.Generic;
using Endfield.Core.Pool;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endfield.Core.UI
{
    /// <summary>
    /// 通用虚拟滚动列表：只渲染可见区间内的格子，滚动时池化回收/补齐。
    /// 机制与内容解耦——调用方提供 setup(index, cell) 回调 + 具体格子组件 T。
    /// 可用于干员头像条、背包物品列表等任何大量数据的滚动（横向/纵向均可）。
    ///
    /// Content 布局要求（横向举例）：
    ///   - 锚定在 Viewport 左缘、pivot (0, 0.5)
    ///   - 这样 content.anchoredPosition.x 直接反映位移，可见区间公式才成立
    ///   - 宽由 SetCount 自动设置，纵向尺寸在 prefab 里调好
    /// </summary>
    public class VirtualScrollList<T> where T : MonoBehaviour
    {
        private readonly ScrollRect _scroll;
        private readonly RectTransform _content;
        private readonly float _itemStep;
        private readonly PrefabPool<T> _pool;
        private readonly Action<int, T> _setupCell;
        private readonly UnityAction<Vector2> _onValueChanged;
        private readonly Dictionary<int, T> _active = new();

        private int _count;

        public VirtualScrollList(ScrollRect scroll, float itemStep, PrefabPool<T> pool, Action<int, T> setupCell)
        {
            _scroll = scroll;
            _content = scroll.content;
            _itemStep = itemStep;
            _pool = pool;
            _setupCell = setupCell;

            _onValueChanged = _ => Refresh();
            _scroll.onValueChanged.AddListener(_onValueChanged);
        }

        /// <summary>设格子总数 → Content 定宽 + 立即刷新可见区。</summary>
        public void SetCount(int count)
        {
            _count = count;
            _content.sizeDelta = new Vector2(count * _itemStep, _content.sizeDelta.y);
            Refresh();
        }

        /// <summary>可见区间裁剪：回收越界的格子，补齐区间内缺失的格子。</summary>
        public void Refresh()
        {
            float viewportSize = _scroll.viewport != null ? _scroll.viewport.rect.width : ((RectTransform)_content.parent).rect.width;
            float x = -_content.anchoredPosition.x;
            int start = Mathf.FloorToInt(x / _itemStep) - 1;                 // ±1 缓冲，平滑滚动
            int end = Mathf.CeilToInt((x + viewportSize) / _itemStep) + 1;

            List<int> toRemove = null;
            foreach (var kv in _active)
            {
                if (kv.Key < start || kv.Key > end)
                {
                    _pool.Release(kv.Value);
                    (toRemove ??= new List<int>()).Add(kv.Key);
                }
            }
            if (toRemove != null)
                foreach (var i in toRemove)
                    _active.Remove(i);

            for (int i = start; i <= end; i++)
            {
                if (i < 0 || i >= _count || _active.ContainsKey(i)) continue;

                var cell = _pool.Get();  //从对象池取一个格子
                var rt = (RectTransform)cell.transform;
                rt.SetParent(_content, false);
                // 强制格子锚点/轴点对准 Content 左缘（横向列表），anchoredPosition = i*step 才从左边开始排
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = new Vector2(i * _itemStep, 0);
                _setupCell(i, cell);  //调用设置格子的委托
                _active[i] = cell;
            }
        }

        /// <summary>重跑所有可见格子的 setup 回调（如选中状态变化后刷新高亮）。</summary>
        public void RebindVisible()
        {
            foreach (var kv in _active)
                _setupCell(kv.Key, kv.Value);
        }

        /// <summary>视图销毁时调用：移除滚动监听，避免泄漏。</summary>
        public void Dispose()
        {
            if (_scroll != null)
                _scroll.onValueChanged.RemoveListener(_onValueChanged);
        }
    }
}
