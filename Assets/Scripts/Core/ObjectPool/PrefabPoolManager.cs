using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Endfield.Core.Pool
{
    /// <summary>
    /// 多池注册表：按 prefab 路径缓存对象池，每种 prefab 只建一个池。
    /// 异步加载一次 + 同步池化无数次：
    /// GetPoolAsync 只在首次 await ResourcesLoader 加载 prefab，之后全部同步返回。
    /// </summary>
    public class PrefabPoolManager : Singleton<PrefabPoolManager>
    {
        private readonly Dictionary<string, PrefabPoolBase> _pools = new();

        /// <summary>获取（或创建）指定 prefab 的对象池。首次会异步加载 prefab。</summary>
        public async UniTask<PrefabPool<T>> GetPoolAsync<T>(string prefabPath,
            int defaultCapacity = 10, int maxSize = 30, bool collectionCheck = true)
            where T : Component
        {
            //如果已经预热过，直接从里面取
            if (_pools.TryGetValue(prefabPath, out var cached))
            {
                // 同一路径只能用同一个 T；用错 T 会得到 null（注册表按路径索引的固有约束）
                return cached as PrefabPool<T>;
            }

            var prefab = await ResourcesLoader.Instance.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[PrefabPoolManager] 加载 prefab 失败: {prefabPath}");
                return null;
            }

            var pool = new PrefabPool<T>(prefab, defaultCapacity, maxSize, collectionCheck);
            _pools[prefabPath] = pool;
            return pool;
        }

        /// <summary>清空所有池（销毁所有实例）。场景切换时调用。</summary>
        public void ClearAll()
        {
            foreach (var pool in _pools.Values)
                pool.Clear();
            _pools.Clear();
        }
    }
}
