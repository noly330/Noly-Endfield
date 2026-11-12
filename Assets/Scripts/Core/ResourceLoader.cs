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
        // 同一 path 的并发加载去重：多个调用者共享同一次 Addressables 请求
        private readonly Dictionary<string, UniTaskCompletionSource<AsyncOperationHandle>> _inflight = new();

        /// <summary>开启一个资源作用域：作用域内加载的资源在 Dispose 时统一释放。</summary>
        public ResourceScope BeginScope() => new ResourceScope(this);

        public T Get<T>(string path) where T : Object
        {
            if (TryGetCached(path, out var handle))
                return (T)handle.Result;
            throw new InvalidOperationException($"[ResourcesLoader] 资源尚未加载(请先 Load):{path}");
        }
        public bool TryGet<T>(string path, out T asset) where T : Object
        {
            if (TryGetCached(path, out var handle))
            {
                asset = (T)handle.Result;
                return true;
            }
            asset = null;
            return false;
        }

        private bool TryGetCached(string path, out AsyncOperationHandle handle)
        {
            if (_cache.TryGetValue(path, out handle) && handle.IsValid() &&
                handle.Status == AsyncOperationStatus.Succeeded)
                return true;

            handle = default;
            return false;
        }

        public async UniTask<T> Load<T>(string path) where T : Object
        {
            var handle = await EnsureLoadedAsync<T>(path);
            if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
            {
                // 运行时引用 +1（缓存命中与首次加载都只在这里计数）
                if (!_refCounts.TryAdd(path, 1))
                    _refCounts[path]++;

                return (T)handle.Result;
            }

            return null;
        }
        /// <summary>
        /// 确保 path 对应的句柄已存在于缓存（并发去重）。不触碰引用计数。
        /// 失败时返回 default(AsyncOperationHandle)（IsValid() == false）。
        /// </summary>
        private async UniTask<AsyncOperationHandle> EnsureLoadedAsync<T>(string path) where T : Object
        {
            if (TryGetCached(path, out var cached))
                return cached;
            //已有相同的path的加载在途中：共享同一次底层加载
            if (_inflight.TryGetValue(path, out var pending))
                return await pending.Task;

            var completionSource = new UniTaskCompletionSource<AsyncOperationHandle>();
            _inflight[path] = completionSource;

            try
            {
                var handle = await StartAndCacheAsync<T>(path);
                completionSource.TrySetResult(handle);
                return handle;
            }
            catch (Exception ex)
            {
                completionSource.TrySetException(ex);
                throw;
            }
            finally
            {
                _inflight.Remove(path);
            }
        }
        private async UniTask<AsyncOperationHandle> StartAndCacheAsync<T>(string path) where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(path);
            try
            {
                await handle.Task;
            }
            catch
            {
                Addressables.Release(handle);
                Debug.LogError($"Failed to load asset: {path}");
                return default;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _cache[path] = handle;
                return handle;
            }

            // 加载失败。释放 handle 防止泄露
            Addressables.Release(handle);
            Debug.LogError($"Failed to load asset: {path}");
            return default;
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
            foreach (var kvp in _cache)
            {
                Addressables.Release(kvp.Value);
            }
            _cache.Clear();
            _refCounts.Clear();
            _inflight.Clear();
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
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
        }
        public async UniTask<T[]> Loads<T>(string[] paths) where T : Object
        {
            var tasks = new List<UniTask<T>>();
            foreach (var path in paths)
            {
                tasks.Add(Load<T>(path));
            }
            return await UniTask.WhenAll(tasks);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>调试用：打印当前所有资源的引用计数，便于验收「关闭界面后归零」。</summary>
        public string DebugDumpRefCounts()
        {
            if (_refCounts.Count == 0) return "(empty)";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[ResourcesLoader] 引用计数（{_refCounts.Count} 项）：");
            foreach (var kvp in _refCounts)
                sb.AppendLine($"{kvp.Value}  {kvp.Key}");
            return sb.ToString();
        }
#endif
    }
}