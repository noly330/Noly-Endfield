using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Endfield.Core.Pool
{
    /// <summary>
    /// 非泛型基类：解决"泛型池放进 Dictionary 后拿不到 Clear()/Count"的问题。
    /// PrefabPoolManager 只认识这个基类，就能统一清池。
    /// </summary>
    public abstract class PrefabPoolBase
    {
        public abstract void Clear();
        public abstract int Count { get; }
    }

    /// <summary>
    /// GameObject prefab 对象池：池化任何挂有 T 组件的 prefab 实例
    /// </summary>
    public class PrefabPool<T> : PrefabPoolBase where T : Component
    {
        private readonly ObjectPool<T> _pool;

        public PrefabPool(GameObject prefab,Transform parent = null, int defaultCapacity = 10, int maxSize = 100,
            bool collectionCheck = true)
        {
            if (prefab == null)
                throw new ArgumentException("prefab 不能为 null");

            _pool = new ObjectPool<T>(
                // 造新实例
                createFunc: () => Object.Instantiate(prefab, parent).GetComponent<T>(),
                // 借出时激活
                actionOnGet: t => t.gameObject.SetActive(true),
                // 归还时隐藏
                actionOnRelease: t => t.gameObject.SetActive(false),
                // 超容量销毁
                actionOnDestroy: t => Object.Destroy(t.gameObject),
                collectionCheck: collectionCheck,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        public T Get() => _pool.Get();
        public void Release(T item) => _pool.Release(item);
        public override void Clear() => _pool.Clear();
        public override int Count => _pool.Count;
    }
}
