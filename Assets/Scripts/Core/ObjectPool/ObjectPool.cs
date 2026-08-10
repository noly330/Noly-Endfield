using System;
using System.Collections.Generic;

namespace Endfield.Core.Pool
{
    /// <summary>
    /// 通用对象池核心（纯 C#，无 Unity 依赖）。
    /// - Stack LIFO 存取
    /// - 构造函数注入回调（组合优于继承）
    /// - maxSize 只管"池保留多少"，不管"已创建多少"
    /// - collectionCheck 用 HashSet 防双重 Get / 双重 Release
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _stack = new();
        private readonly Func<T> _createFunc;
        private readonly Action<T> _actionOnGet;
        private readonly Action<T> _actionOnRelease;
        private readonly Action<T> _actionOnDestroy;
        private readonly int _maxSize;
        private readonly bool _collectionCheck;
        private readonly HashSet<T> _active;   // 只在 collectionCheck 开启时才用

        public ObjectPool(Func<T> createFunc,
                Action<T> actionOnGet = null,
                Action<T> actionOnRelease = null,
                Action<T> actionOnDestroy = null,
                bool collectionCheck = true,
                int defaultCapacity = 10,
                int maxSize = 10000)
        {
            if (createFunc == null)
                throw new ArgumentException("createFunc 不能为 null");
            if (maxSize <= 0)
                throw new ArgumentException("maxSize 必须大于 0");

            _createFunc = createFunc;
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
            _actionOnDestroy = actionOnDestroy;
            _maxSize = maxSize;
            _collectionCheck = collectionCheck;
            if (_collectionCheck)
                _active = new HashSet<T>();

            //保护，预热容量不能大于最大容量
            defaultCapacity = Math.Min(defaultCapacity,_maxSize);
            // 预热：提前把 defaultCapacity 个对象放进池，首次 Get 不卡顿。
            // 注意：预热实例也要走 actionOnRelease（PrefabPool 里是 SetActive(false)），
            // 否则池里"空闲"的对象是激活的，会被误当成在场上（按一次生成几十只的 bug 根源）。
            for (int i = 0; i < defaultCapacity; i++)
            {
                var item = _createFunc();
                _actionOnRelease?.Invoke(item);
                _stack.Push(item);
            }
        }

        /// <summary>借出对象。池空则新造（maxSize 不管创建数量）。</summary>
        public T Get()
        {
            T item = _stack.Count > 0 ? _stack.Pop() : _createFunc();

            _actionOnGet?.Invoke(item);

            // 双重 Get 检测：如果它已经在外面，说明 createFunc 造出重复引用，是 bug
            if (_collectionCheck && !_active.Add(item))
                throw new InvalidOperationException($"对象 {item} 已在池外（重复 Get）");

            return item;
        }

        /// <summary>归还对象。池满了则销毁，否则放回栈。</summary>
        public void Release(T item)
        {
            // 双重 Release 检测：它不在外面 = 重复归还，当场报错（官方池最值钱的设计）
            if (_collectionCheck && !_active.Remove(item))
                throw new InvalidOperationException($"对象 {item} 已被释放或从未借出（重复 Release）");

            if (_stack.Count >= _maxSize)
            {
                _actionOnDestroy?.Invoke(item);
                return;
            }

            _actionOnRelease?.Invoke(item);
            _stack.Push(item);
        }

        /// <summary>清空池：销毁所有空闲对象，重置借出跟踪。</summary>
        public void Clear()
        {
            while (_stack.Count > 0)
            {
                var item = _stack.Pop();
                _actionOnDestroy?.Invoke(item);
            }

            // 同步清空 active，让池处于"全新"状态（Clear 之后任何 Release 都会被判定为非法归还）
            if (_collectionCheck)
                _active.Clear();
        }

        /// <summary>当前池里的空闲对象数量。</summary>
        public int Count => _stack.Count;
    }
}
