using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
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
        /// 未命中：加载预制体 → 实例化 → 用 VContainer 创建视图对象并注入依赖
        ///         → Initialize（内部 await OnInit）→ Show（触发 OnShown）。
        /// </summary>
        /// <param name="entry">视图注册项；缓存以 PrefabPath 为 key，同一 prefab 只会创建一个实例</param>
        /// <param name="data">传给 OnShown 的数据</param>
        /// <param name="layer">界面层级；只有 Pop 层会进入 _viewStack</param>
        public async UniTask OpenView(ViewEntry entry, object data = null, UILayer layer = UILayer.Bottom)
        {
            // 已缓存：只走 Show → OnShown。
            // 注意：不会重新 Initialize/OnInit；同一 prefab 用不同 layer 再打开会复用旧实例、停留在原层级；
            // Pop 层会再次入栈，重复打开同一弹窗会在 _viewStack 里留下重复项。
            if (_viewCache.TryGetValue(entry.PrefabPath, out var cachedView))
            {
                cachedView.Show(data);
                if (layer == UILayer.Pop) _viewStack.Push(cachedView);
                return;
            }

            // 未缓存：异步加载预制体（Addressables，引用计数 +1，销毁时需配对 Release），
            // 然后实例化到目标层级的 Canvas 下。
            // 注意：同一 entry 并发打开时这里没有去重，可能各自加载实例化，产生重复实例。
            var viewPrefab = await ResourcesLoader.Instance.LoadPrefab(entry);
            if (viewPrefab == null)
            {
                Debug.LogError($"[UIManager] Failed to load view prefab: {entry.PrefabPath}");
                return;
            }
            var canvasTrs = GetLayerTransform(layer);
            var viewGo = Object.Instantiate(viewPrefab, canvasTrs, false);
            viewGo.name = entry.Name;

            // 用 VContainer 的容器创建「视图类实例」并注入依赖：
            // - 视图类（BaseView 子类）是普通 C# 对象，与上面实例化的 viewGo 是两个东西，
            //   Initialize(viewGo.transform) 时才把两者关联起来；
            // - 构造函数依赖会被递归解析，标记 [Inject] 的字段/属性在这一步完成注入；
            // - 注入发生在 Initialize/OnInit 之前，所以 OnInit 里可以直接使用注入的依赖；
            // - 视图类型必须已在 GameLifetimeScope 注册：未注册时 Resolve 会直接抛异常，
            //   不会走到下面的 is not IView 判断；entry.ViewType 为 null 时同样会抛异常；
            //   is not IView 只能兜住「解析成功但类型不是 IView」的情况。
            if (Container.Resolve(entry.ViewType) is not IView viewInstance)
            {
                // 此分支中已实例化的 viewGo 和预制体引用尚未释放
                // TODO: Object.Destroy(viewGo); ResourcesLoader.Instance.Release(entry.PrefabPath);
                Debug.LogError($"[UIManager] Failed to resolve view instanceInit: {entry.ViewType}");
                return;
            }

            // Initialize 内部会 await OnInit（一次性初始化：找节点、建列表、绑按钮等），
            // 完成之后才 Show：SetActive(true) 并触发 OnShown。
            await viewInstance.Initialize(viewGo.transform, entry.Name);
            viewInstance.Show(data);

            // 以 PrefabPath 为 key 缓存，后续打开直接复用
            _viewCache[entry.PrefabPath] = viewInstance;
            if (layer == UILayer.Pop) _viewStack.Push(viewInstance);

        }
        /// <summary>
        /// 关闭指定视图
        /// </summary>
        /// <param name="entry">Register</param>
        /// <param name="destroy">如果为true,则同时销毁视图并从缓存中移除</param>
        public void CloseView(ViewEntry entry,bool destroy = false)
        {
            if(!_viewCache.TryGetValue(entry.PrefabPath, out var view)) return;
            if (destroy)
            {
                view.Destroy();
                _viewCache.Remove(entry.PrefabPath);
                ResourcesLoader.Instance.Release(entry.PrefabPath);
            }
            else
            {
                view.Hide();
            }
            //从栈中移除(如果存在)
            var tempStack = new Stack<IView>();
            while(_viewStack.Count > 0)
            {
                var v = _viewStack.Pop();
                if(v != view) tempStack.Push(v);
            }
            while(tempStack.Count > 0)
            {
                _viewStack.Push(tempStack.Pop());
            }
        }

        /// <summary>
        /// 关闭指定视图实例
        /// </summary>
        /// <param name="view">视图实例</param>
        /// <param name="destroy">如果为 true，则同时销毁视图并从缓存中移除所有对应的缓存条目</param>
        public void CloseView(IView view,bool destroy = false)
        {
            if(view == null) return;
            if (destroy)
            {
                //Where(kv => kv.Value == view)：遍历字典，找出所有值为 view 的条目
                var keysToRemove = _viewCache.Where(kv => kv.Value == view).Select(kv => kv.Key).ToList();
                foreach(var key in keysToRemove)
                {
                    _viewCache.Remove(key);
                    ResourcesLoader.Instance.Release(key);
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
            if (!_viewCache.TryGetValue(entry.PrefabPath, out var view)) return;
            CloseView(entry);
            view.Destroy();
            _viewCache.Remove(entry.PrefabPath);
            ResourcesLoader.Instance.Release(entry.PrefabPath);
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
                ResourcesLoader.Instance.Release(kvp.Key);
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