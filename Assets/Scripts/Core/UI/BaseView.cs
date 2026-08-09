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
        protected IObjectResolver Container{get;set;}
        private string _viewName;
        protected Transform RootTransform{get;private set;}
        protected GameObject RootGameObject => RootTransform == null ? null : RootTransform.gameObject;
        //组件缓存
        private readonly Dictionary<string,Component> _componentCache = new Dictionary<string,Component>();
        public virtual async UniTask Initialize(Transform root, string viewName = "")
        {
            _viewName = viewName;
            RootTransform = root;
            if(SortOrder > 0)
            {
                if(!RootTransform.TryGetComponent<Canvas>(out var canvas))
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
            if(RootGameObject == null)  return;
            //防御性编程
            if(!RootGameObject.TryGetComponent<Canvas>(out _))
            {
                RootGameObject.AddComponent<Canvas>();
                RootGameObject.AddComponent<GraphicRaycaster>();
            }
            RootGameObject.SetActive(true);
            OnShown(data);
            
        }
        public virtual void Hide()
        {
            if(RootGameObject != null)  RootGameObject.SetActive(false);
            OnHidden();
        }
        public void Destroy()
        {
            OnHidden();
            OnDestroy();
            if (RootGameObject != null) Object.Destroy(RootGameObject);
            _componentCache.Clear();
            RootTransform = null;
        }

        /// <summary>
        /// 获取 UI 组件（泛型版本）。这是操作 UI 的核心方法。
        /// </summary>
        protected T GetComponent<T>(string path = "",Transform parent = null) where T : Component
        {
            string cacheKey = $"{path}_{typeof(T).Name}";
            if(_componentCache.TryGetValue(cacheKey,out var cachedComp))
            {
                return cachedComp as T;
            }
            Transform root = parent == null ? RootTransform : parent;
            Transform target = string.IsNullOrEmpty(path) ? root : root.Find(path);

            var comp = target.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogError($"[BaseView] Path not found: {path} in {root.name}");
                return null;
            }

            _componentCache[cacheKey] = comp;
            return comp;
        }

        public async UniTask<T> SetWidget<T>(ViewEntry entry,Transform parent) where T : BaseWidget
        {
            if (Container.Resolve(entry.ViewType) is T widget)
            {
                await widget.Initialize(parent);
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