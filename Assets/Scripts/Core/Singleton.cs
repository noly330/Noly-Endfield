
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
                        _instance = FindObjectOfType<T>() as T;

                        if (_instance == null)
                        {
                            GameObject go = new GameObject(typeof(T).Name);
                            _instance = go.AddComponent<T>();
                        }
                    }
                }

                return _instance;
            }
        }


        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }


    }
}