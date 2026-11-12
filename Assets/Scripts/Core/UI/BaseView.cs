using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Object = UnityEngine.Object;

namespace Endfield.Core
{
    public abstract class BaseView : IView
    {
        protected virtual int SortOrder => 0;
        [Inject]
        protected IObjectResolver Container { get; set; }  //在UIManager.OpenView时注入
        protected Transform RootTransform { get; private set; }
        protected GameObject RootGameObject => RootTransform == null ? null : RootTransform.gameObject;
        /// <summary>视图级资源作用域：由 UIManager 创建并移交，视图销毁时统一释放。</summary>
        protected ResourceScope Scope { get; private set; }
        //组件缓存
        private readonly Dictionary<string, Component> _componentCache = new Dictionary<string, Component>();
        // 由本视图创建的子 Widget：销毁视图时一并销毁，确保它们的资源作用域被释放
        private readonly List<BaseWidget> _widgets = new();
        public virtual async UniTask Initialize(Transform root, ResourceScope scope = null)
        {
            Scope = scope;
            RootTransform = root;

            if (SortOrder > 0)
            {
                if (!RootTransform.TryGetComponent<Canvas>(out var canvas))
                {
                    canvas = RootTransform.AddComponent<Canvas>();
                    RootTransform.AddComponent<CanvasRenderer>();
                    RootTransform.AddComponent<GraphicRaycaster>();
                }
                //无视父级，独立控制自己的渲染顺序
                canvas.overrideSorting = true;
                canvas.sortingOrder = SortOrder;
            }

            await OnInit(root);
        }
        public virtual void Show(object data = null)
        {
            if (RootGameObject == null) return;
            //防御性编程
            if (!RootGameObject.TryGetComponent<Canvas>(out _))
            {
                RootGameObject.AddComponent<Canvas>();
                RootGameObject.AddComponent<GraphicRaycaster>();
            }
            RootGameObject.SetActive(true);
            OnShown(data);

        }
        public virtual void Hide()
        {
            if (RootGameObject != null) RootGameObject.SetActive(false);
            OnHidden();
        }
        public void Destroy()
        {
            if (RootTransform == null && Scope == null) return;   // 幂等：重复 Destroy 直接忽略

            OnHidden();
            OnDestroy();

            foreach (var widget in _widgets)
                widget.Destroy();
            _widgets.Clear();

            if (RootGameObject != null) Object.Destroy(RootGameObject);
            _componentCache.Clear();

            Scope?.Dispose();
            Scope = null;

            RootTransform = null;
        }

        /// <summary>
        /// 获取 UI 组件（泛型版本）。这是操作 UI 的核心方法。
        /// </summary>
        protected T GetComponent<T>(string path = "", Transform parent = null) where T : Component
        {
            string cacheKey = $"{path}_{typeof(T).Name}";
            if (_componentCache.TryGetValue(cacheKey, out var cachedComp))
            {
                return cachedComp as T;
            }
            Transform root = parent == null ? RootTransform : parent;
            Transform target = string.IsNullOrEmpty(path) ? root : root.Find(path);
            if (target == null)
            {
                Debug.LogError($"[BaseView] Path not found: {path} in {root.name}");
                return null;
            }

            var comp = target.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogError($"[BaseView] Path not found: {path} in {root.name}");
                return null;
            }

            _componentCache[cacheKey] = comp;
            return comp;
        }

        public async UniTask<T> SetWidget<T>(ViewEntry entry, Transform parent) where T : BaseWidget
        {
            if (Container.Resolve(entry.ViewType) is T widget)
            {
                await widget.Initialize(parent);
                _widgets.Add(widget);
                return widget;
            }
            Debug.LogError($"[BaseView] View not found: {entry.ViewType}");
            return null;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual UniTask OnInit(Transform root) => UniTask.CompletedTask;

        /// <summary>
        /// 子类可重写，处理显示逻辑
        /// </summary>
        protected virtual void OnShown(object data)
        {
        }

        /// <summary>
        /// 子类可重写，处理隐藏逻辑
        /// </summary>
        protected virtual void OnHidden()
        {
        }

        /// <summary>
        /// 子类可重写，处理销毁逻辑
        /// </summary>
        protected virtual void OnDestroy()
        {
        }
    }
}