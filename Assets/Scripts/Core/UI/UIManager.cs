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
        /// 打开一个视图
        /// </summary>
        /// <param name="entry">Register</param>
        /// <param name="data">传递的数据</param>
        /// <param name="layer">界面层级类型</param>
        public async UniTask OpenView(ViewEntry entry, object data = null, UILayer layer = UILayer.Bottom)
        {
            if (_viewCache.TryGetValue(entry.PrefabPath, out var cachedView))
            {
                cachedView.Show(data);
                if (layer == UILayer.Pop) _viewStack.Push(cachedView);
                return;
            }
            var viewPrefab = await ResourcesLoader.Instance.LoadPrefab(entry);
            if (viewPrefab == null)
            {
                Debug.LogError($"[UIManager] Failed to load view prefab: {entry.PrefabPath}");
                return;
            }
            var canvasTrs = GetLayerTransform(layer);
            var viewGo = Object.Instantiate(viewPrefab, canvasTrs, false);
            viewGo.name = entry.Name;

            //依赖注入
            //通过之前用 [Inject] 注入的 IObjectResolver（VContainer 的依赖注入容器）解析视图类型
            //entry.ViewType 是视图的类类型（比如 LoginView、MainMenuView）
            //容器会负责：
            //创建该类型的实例
            //自动注入该实例中所有标记了 [Inject] 的属性/字段
            //如果该类型有构造函数依赖，也会自动递归解析
            //检查解析结果是否实现了 IView 接口，否则报错
            if (Container.Resolve(entry.ViewType) is not IView viewInstance)
            {
                Debug.LogError($"[UIManager] Failed to resolve view instanceInit: {entry.ViewType}");
                return;
            }
            await viewInstance.Initialize(viewGo.transform, entry.Name);
            viewInstance.Show(data);

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
        public void CloseView(IView view,bool destroy = true)
        {
            if(view == null) return;
            if (destroy)
            {
                //Where(kv => kv.Value == view)：遍历字典，找出所有值为 view 的条目
                var keysToRemove = _viewCache.Where(kv => kv.Value == view).Select(kv => kv.Key).ToList();
                foreach(var key in keysToRemove)
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
            if (!_viewCache.TryGetValue(entry.PrefabPath, out var view)) return;
            CloseView(entry);
            view.Destroy();
            _viewCache.Remove(entry.PrefabPath);
        }

        public void CloseAllViews()
        {
            while (_viewStack.Count > 0)
            {
                _viewStack.Pop().Hide();
            }

            foreach (var view in _viewCache.Values)
            {
                view.Destroy();
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