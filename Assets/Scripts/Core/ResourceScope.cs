using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Endfield.Core
{
    /// <summary>
    /// 资源作用域：通过本作用域加载的资源在 Dispose 时按引用计数统一释放。
    /// 只释放本作用域加载的资源，不影响作用域外（含全局预载）的资产。
    /// </summary>
    public sealed class ResourceScope : IDisposable
    {
        private readonly ResourcesLoader _loader;
        private readonly List<string> _tracked = new();
        private bool _disposed;  // 当前作用域是否已释放
        public bool IsDisposed => _disposed;

        internal ResourceScope(ResourcesLoader loader)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }

        /// <summary>
        /// 异步加载单个资源
        /// </summary>
        public async UniTask<T> Load<T>(string path) where T : Object
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ResourceScope), "资源作用域已释放，不能继续加载");
            }
            var asset = await _loader.Load<T>(path);
            if (asset != null)
            {
                _tracked.Add(path);
            }
            return asset;
        }

        /// <summary>
        /// 异步加载多个资源，并返回资源数组
        /// </summary>
        public async UniTask<T[]> Loads<T>(params string[] paths) where T : Object
        {
            if (paths == null || paths.Length == 0) return Array.Empty<T>();

            var tasks = new List<UniTask<T>>(paths.Length);
            foreach (var path in paths)
                tasks.Add(Load<T>(path));
            return await UniTask.WhenAll(tasks);
        }

        /// <summary>
        /// 预加载多个资源，不返回加载结果
        /// </summary>

        public async UniTask PreLoad<T>(params string[] paths) where T : Object
        {
            if (paths == null || paths.Length == 0) return;

            var tasks = new List<UniTask>(paths.Length);
            foreach (var path in paths)
                tasks.Add(Load<T>(path));
            await UniTask.WhenAll(tasks);
        }
        /// <summary>
        /// 同步获取资源
        /// </summary>
        public T Get<T>(string path) where T : Object => _loader.Get<T>(path);

        /// <summary>
        /// 释放整个作用域
        /// </summary>
        public void Dispose()
        {
            if(_disposed)  return;
            _disposed = true;
            foreach(var path in _tracked)
                _loader.Release(path);
            _tracked.Clear();
        }
    }
}