using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Endfield.Core
{
    public abstract class BaseWidget : IView
    {
        protected virtual int SortOrder => 0;

        protected Transform Parent { get; private set; }
        protected abstract ViewEntry ViewEntry { get; }
        private readonly Dictionary<string, Component> _componentCache = new();
        public Transform transform { get; private set; }
        public GameObject gameObject { get; private set; }
        public RectTransform transformRect => transform.GetComponent<RectTransform>();

        public async UniTask Initialize(Transform parent, string _ = "")
        {
            if (parent == null)
            {
                Debug.LogError("parent empty Use");
                return;
            }
            Parent = parent;
            if (string.IsNullOrEmpty(ViewEntry.PrefabPath)) return;
            var prefab = await ResourcesLoader.Instance.LoadPrefab(ViewEntry);
            if (prefab == null) return;

            gameObject = Object.Instantiate(prefab, parent);
            transform = gameObject.transform;

            if (SortOrder > 0)
            {
                if (!transform.TryGetComponent<Canvas>(out var canvas))
                {
                    canvas = transform.AddComponent<Canvas>();
                    transform.AddComponent<CanvasRenderer>();
                    transform.AddComponent<GraphicRaycaster>();
                }
                canvas.overrideSorting = true;
                canvas.sortingOrder = SortOrder;
            }
            gameObject.SetActive(false);
            OnInit();
        }

        public async void Show(object data = null)
        {
            try
            {
                await UniTask.WaitUntil(() => gameObject != null);
                gameObject.SetActive(true);
                OnShown(data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Hide()
        {
            if (gameObject != null) gameObject.SetActive(false);
            OnHidden();
        }

        public void Destroy()
        {
            OnHidden();
            OnDestroy();
            if (transform != null) Object.Destroy(transform.gameObject);
            _componentCache.Clear();
            ResourcesLoader.Instance.Release(ViewEntry.PrefabPath);
            transform = null;
            gameObject = null;
        }

        /// <summary>
        /// 获取 UI 组件（泛型版本）。这是操作 UI 的核心方法。
        /// </summary>
        protected T GetComponent<T>(string path = "") where T : Component
        {
            string cacheKey = $"{path}_{typeof(T).Name}";
            if (_componentCache.TryGetValue(cacheKey, out var cachedComp))
            {
                return cachedComp as T;
            }

            Transform target = string.IsNullOrEmpty(path) ? transform : transform.Find(path);
            if (target == null)
            {
                Debug.LogError($"[BaseView] Path not found: {path} in {transform.name}");
                return null;
            }

            var comp = target.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogError($"[BaseView] Component {typeof(T).Name} not found at path: {path}");
                return null;
            }

            _componentCache[cacheKey] = comp;
            return comp;
        }


        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void OnInit()
        {
        }

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