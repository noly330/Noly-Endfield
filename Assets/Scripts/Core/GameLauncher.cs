using UnityEngine;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using Endfield.Core.Resource;
using Endfield.Data.User;
using Endfield.Data.Catalog;

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
            LoadGame().Forget();
        }

        private void OnDestroy()
        {
            UIManager.Instance.CloseAllViews();
            ResourcesLoader.Instance.ReleaseAll();
        }

        private async UniTask LoadGame()
        {
            await UserDataService.Instance.InitializeAsync();  //加载账号数据
            await OperatorCatalog.BuildAsync();  //加载干员数据
            
            UIManager.Instance.OpenView(UIRegister.MainView, UILayer.Top).Forget();
            // 启动预热：遍历预热表
            PrewarmRegister.PrewarmAll().Forget();
        }
    }
}