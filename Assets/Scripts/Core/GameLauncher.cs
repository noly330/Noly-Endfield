using UnityEngine;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using Endfield.Core.Resource;
using Endfield.Data.User;
using Endfield.Data.Catalog;
using Endfield.Core.Pool;
using Endfield.Module.Audio;

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

            //加载队伍
            var cam = Object.FindObjectOfType<ThirdPersonCamera>();
            await TeamManager.Instance.InitializeAsync(transform, cam);

            //加载UI
            UIManager.Instance.OpenView(UIRegister.TopToolBarView, layer: UILayer.Middle).Forget();
            
            // 启动预热：遍历预热表
            var poolRoot = new GameObject("[PoolRoot]").transform;
            PrefabPoolManager.Instance.Initialize(poolRoot);
            await AudioService.Instance.InitializeAsync();
            PrewarmRegister.PrewarmAll().Forget();
        }
    }
}