using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Endfield.Core
{
    public class ResourcesLoader : Singleton<ResourcesLoader>
    {
        //AsyncOperationHandle:异步操作句柄，用于跟踪和管理异步资源加载操作的状态和结果
        private readonly Dictionary<string, AsyncOperationHandle> _cache = new();
        private readonly Dictionary<string, int> _refCounts = new();


        public async UniTask<T> Load<T>(string path) where T : Object
        {
            //如果已经加载，直接返回
            if (_cache.TryGetValue(path, out var cachedHandle))
            {
                //确保资源有效
                if (cachedHandle.IsValid() && cachedHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    //增加引用计数
                    if (!_refCounts.TryAdd(path, 1))
                    {
                        _refCounts[path]++;
                    }

                    return (T)cachedHandle.Result;
                }

                //缓存无效，移除并释放
                Addressables.Release(cachedHandle);
                _cache.Remove(path);
                _refCounts.Remove(path);
            }
            //首次加载
            var handle = Addressables.LoadAssetAsync<T>(path);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _cache[path] = handle;
                _refCounts[path] = 1;
                return handle.Result;
            }

            //加载失败。释放handle防止泄露
            Addressables.Release(handle);
            Debug.LogError($"Failed to load asset: {path}");
            return null;
        }

        /// <summary>
        /// 释放资源（减少引用计数）
        /// </summary>
        public void Release(string path)
        {
            if (!_refCounts.ContainsKey(path))
            {
                Debug.LogWarning($"Trying to release unloaded or already released asset: {path}");
                return;
            }
            _refCounts[path]--;
            if (_refCounts[path] <= 0)
            {
                //引用计数为0，真正释放
                if (_cache.TryGetValue(path, out var handle))
                {
                    Addressables.Release(handle);
                    _cache.Remove(path);
                }
                _refCounts.Remove(path);
            }
        }
        public void ReleaseAll()
        {
            foreach(var kvp in _cache)
            {
                Addressables.Release(kvp.Value);
            }
            _cache.Clear();
            _refCounts.Clear();
        }
        public async UniTask<T> LoadNoCache<T>(string path) where T : Object
        {
            AsyncOperationHandle<T> handle = default;
            try
            {
                handle = Addressables.LoadAssetAsync<T>(path);
                await handle.Task;
                return handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : null;
            }
            catch (Exception e)
            {
                Debug.LogError($"LoadNoCache failed for '{path}': {e}");
                return null;
            }
            finally
            {
                //不管成功还是失败，最后都释放handle
                if(handle.IsValid())
                    Addressables.Release(handle);
            }
        }

        public async UniTask<GameObject> LoadPrefab(ViewEntry entry) => await Load<GameObject>(entry.PrefabPath);
        public async UniTask<T[]> Loads<T>(string[] paths) where T : Object
        {
            var tasks = new List<UniTask<T>>();
            foreach(var path in paths)
            {
                tasks.Add(Load<T>(path));
            }
            return await UniTask.WhenAll(tasks);
        }
    }
}