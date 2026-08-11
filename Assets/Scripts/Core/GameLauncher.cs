using UnityEngine;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using Endfield.Core.Resource;
using Endfield.Data.User;
using Endfield.Data.Catalog;
using Endfield.Core.Pool;

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
            await UserDataService.Instance.InitializeAsync();  // 1. 玩家数据
            await OperatorCatalog.BuildAsync();                 // 2. 干员图鉴

            // 3. 加载队伍（主控 = 第一位干员）
            var cam = Object.FindObjectOfType<ThirdPersonCamera>();
            await TeamManager.Instance.InitializeAsync(transform, cam);

            UIManager.Instance.OpenView(UIRegister.MainView, UILayer.Top).Forget();
            // 启动预热：遍历预热表
            var poolRoot = new GameObject("[PoolRoot]").transform;
            PrefabPoolManager.Instance.Initialize(poolRoot);
            PrewarmRegister.PrewarmAll().Forget();
        }
    }
}