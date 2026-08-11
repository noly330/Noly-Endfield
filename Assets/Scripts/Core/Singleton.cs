
using UnityEngine;
namespace Endfield.Core
{
    public class Singleton<T> where T : class, new()
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                _instance ??= new T();
                return _instance;
            }
        }

    }
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static object _lock = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        // 只查找不创建：单例应放场景里（场景卸载/销毁阶段返回 null 是合法的，调用方判空处理）
                        _instance = FindObjectOfType<T>() as T;
                    }
                }

                return _instance;
            }
        }


        /// <summary>是否跨场景常驻（DontDestroyOnLoad）。默认 true；场景级服务可覆写为 false。</summary>
        protected virtual bool KeepAcrossScenes => true;

        protected virtual void Awake()
        {
            // 允许 getter 提前设置 _instance（访问发生在 Awake 之前）：_instance == this 不算重复，不能自毁
            if (_instance == null || _instance == this)
            {
                _instance = this as T;
                if (KeepAcrossScenes)
                    DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }


    }
}