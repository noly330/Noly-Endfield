using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using System;
using Object = UnityEngine.Object;

namespace Endfield.Core
{
    public enum UILayer
    {
        Bottom = 0,
        Middle = 2000,
        Top = 5000,
        Pop = 9000,
    }

    public class UIManager : Singleton<UIManager>
    {
        private readonly Stack<IView> _viewStack = new();
        private readonly Dictionary<string, IView> _viewCache = new();
        [Inject]
        private IObjectResolver Container { get; set; }
        private Canvas _rootCanvas;
        private readonly Dictionary<UILayer, Transform> _canvasCache = new();

        public void Initialize(Canvas rootCanvas)
        {
            _rootCanvas = rootCanvas;
            InitCanvasLayers();
        }
        private void InitCanvasLayers()
        {
            _canvasCache.Clear();
            _canvasCache[UILayer.Bottom] = _rootCanvas.transform.Find("Bottom");
            _canvasCache[UILayer.Middle] = _rootCanvas.transform.Find("Middle");
            _canvasCache[UILayer.Top] = _rootCanvas.transform.Find("Top");
            _canvasCache[UILayer.Pop] = _rootCanvas.transform.Find("Pop");
        }

        /// <summary>
        /// 打开一个视图。
        /// 缓存命中：复用已有实例，只重新 Show（触发 OnShown），不会再次执行 OnInit。
        /// 未命中：创建资源作用域 → 加载预制体 → 实例化 → VContainer 创建视图并注入
        ///         → Initialize（作用域移交给 View，内部 await OnInit）→ Show。
        /// 失败路径会销毁已实例化的 GameObject 并释放作用域，不残留资源。
        /// </summary>
        public async UniTask OpenView(ViewEntry entry, object data = null, UILayer layer = UILayer.Bottom)
        {
            // 已缓存：只走 Show → OnShown
            if (_viewCache.TryGetValue(entry.PrefabPath, out var cachedView))
            {
                cachedView.Show(data);
                if (layer == UILayer.Pop) _viewStack.Push(cachedView);
                return;
            }

            // 视图级资源作用域：Prefab 与 OnInit 中加载的资源都随视图销毁统一释放
            var scope = ResourcesLoader.Instance.BeginScope();
            var viewPrefab = await scope.Load<GameObject>(entry.PrefabPath);
            if (viewPrefab == null)
            {
                Debug.LogError($"[UIManager] Failed to load view prefab: {entry.PrefabPath}");
                scope.Dispose();
                return;
            }

            var canvasTrs = GetLayerTransform(layer);
            var viewGo = Object.Instantiate(viewPrefab, canvasTrs, false);
            viewGo.name = entry.Name;

            // 用 VContainer 创建视图对象并注入依赖。
            // 注意：Resolve 本身可能抛异常（类型未注册 / ViewType 为 null），必须放在 try 里
            IView viewInstance;
            try
            {
                viewInstance = Container.Resolve(entry.ViewType) as IView;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Object.Destroy(viewGo);
                scope.Dispose();
                throw;
            }

            if (viewInstance == null)
            {
                Debug.LogError($"[UIManager] Failed to resolve view instance: {entry.ViewType}");
                Object.Destroy(viewGo);
                scope.Dispose();
                return;
            }

            // 作用域所有权在这里移交给 View：成功时由 View.Destroy 释放，
            // 失败时由 View 自己的销毁流程负责释放（含子 Widget 的 scope）
            try
            {
                await viewInstance.Initialize(viewGo.transform, scope);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);

                // View 可能已经创建了子 Widget，走它自己的销毁流程，
                // 保证 Widget 的 scope 也被释放；再兜底清理根节点和 scope（Dispose 幂等）
                try
                {
                    viewInstance.Destroy();
                }
                catch (Exception destroyEx)
                {
                    Debug.LogException(destroyEx);
                }

                if (viewGo != null) Object.Destroy(viewGo);
                scope.Dispose();
                throw;
            }

            viewInstance.Show(data);

            _viewCache[entry.PrefabPath] = viewInstance;
            if (layer == UILayer.Pop) _viewStack.Push(viewInstance);
        }
        /// <summary>
        /// 关闭指定视图
        /// </summary>
        /// <param name="entry">Register</param>
        /// <param name="destroy">如果为true,则同时销毁视图并从缓存中移除</param>
        public void CloseView(ViewEntry entry, bool destroy = false)
        {
            if (!_viewCache.TryGetValue(entry.PrefabPath, out var view)) return;
            if (destroy)
            {
                view.Destroy();
                _viewCache.Remove(entry.PrefabPath);
            }
            else
            {
                view.Hide();
            }
            //从栈中移除(如果存在)
            var tempStack = new Stack<IView>();
            while (_viewStack.Count > 0)
            {
                var v = _viewStack.Pop();
                if (v != view) tempStack.Push(v);
            }
            while (tempStack.Count > 0)
            {
                _viewStack.Push(tempStack.Pop());
            }
        }

        /// <summary>
        /// 关闭指定视图实例
        /// </summary>
        /// <param name="view">视图实例</param>
        /// <param name="destroy">如果为 true，则同时销毁视图并从缓存中移除所有对应的缓存条目</param>
        public void CloseView(IView view, bool destroy = false)
        {
            if (view == null) return;
            if (destroy)
            {
                //Where(kv => kv.Value == view)：遍历字典，找出所有值为 view 的条目
                var keysToRemove = _viewCache.Where(kv => kv.Value == view).Select(kv => kv.Key).ToList();
                foreach (var key in keysToRemove)
                {
                    _viewCache.Remove(key);
                }
                view.Destroy();
            }
            else
            {
                view.Hide();
            }
            /// 从栈中移除（如果存在）
            var tempStack = new Stack<IView>();
            while (_viewStack.Count > 0)
            {
                var v = _viewStack.Pop();
                if (v != view) tempStack.Push(v);
            }

            while (tempStack.Count > 0) _viewStack.Push(tempStack.Pop());
        }

        /// <summary>
        /// 销毁视图（释放资源）
        /// </summary>
        public void DestroyView(ViewEntry entry)
        {
            CloseView(entry, destroy: true);
        }

        public void CloseAllViews()
        {
            while (_viewStack.Count > 0)
            {
                _viewStack.Pop().Hide();
            }

            foreach (var kvp in _viewCache)
            {
                kvp.Value.Destroy();
            }

            // 清理缓存，确保后续打开不会引用已销毁的视图实例
            _viewCache.Clear();
        }

        public Transform GetLayerTransform(UILayer layer)
        {
            if (_canvasCache.TryGetValue(layer, out var layerTransform))
            {
                return layerTransform;
            }

            Debug.LogError($"[UIManager] Layer {layer} not found in canvas cache.");
            return _rootCanvas.transform;
        }
    }
}