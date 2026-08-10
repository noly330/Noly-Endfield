using UnityEngine;
using VContainer.Unity;
using Cysharp.Threading.Tasks;

namespace Endfield.Core
{
    public class GameLauncher : MonoBehaviour
    {
        private Canvas _rootCanvas;
        private LifetimeScope Scope;

        private void Awake()
        {
            Scope = GetComponent<LifetimeScope>();
            Scope.Container.Inject(this);
            _rootCanvas = GameObject.Find("UI").GetComponent<Canvas>();
            UIManager.Instance.Initialize(_rootCanvas);
        }

        void Start()
        {
            UIManager.Instance.OpenView(UIRegister.MainView,UILayer.Top).Forget();
        }

        private void OnDestroy()
        {
            UIManager.Instance.CloseAllViews();
            ResourcesLoader.Instance.ReleaseAll();
        }
    }
}