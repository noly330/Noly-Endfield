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
            await LoadData();
            await InitGameplay();
            OpenUI();
            await InitServices();
            Prewarm();
        }

        /// <summary>阶段1：玩家数据 + 干员图鉴</summary>
        private async UniTask LoadData()
        {
            await UserDataService.Instance.InitializeAsync();
            await OperatorCatalog.BuildAsync();
        }

        /// <summary>阶段2：战斗（队伍 + 相机）</summary>
        private async UniTask InitGameplay()
        {
            var cam = Object.FindObjectOfType<ThirdPersonCamera>();
            await TeamManager.Instance.InitializeAsync(transform, cam);
        }

        /// <summary>阶段3：打开常驻 UI（工具栏 + 战斗 HUD）</summary>
        private void OpenUI()
        {
            UIManager.Instance.OpenView(UIRegister.TopToolBarView, layer: UILayer.Middle).Forget();
            UIManager.Instance.OpenView(UIRegister.CombatHUDView, layer: UILayer.Bottom).Forget();
        }

        /// <summary>阶段4：服务（对象池根 + 音频 + 伤害飘字层）</summary>
        private async UniTask InitServices()
        {
            var poolRoot = new GameObject("[PoolRoot]").transform;
            PrefabPoolManager.Instance.Initialize(poolRoot);

            await AudioService.Instance.InitializeAsync();

            var dmgLayer = new GameObject("DamageTextLayer").AddComponent<RectTransform>();
            dmgLayer.SetParent(_rootCanvas.transform, false);
            dmgLayer.anchorMin = Vector2.zero;
            dmgLayer.anchorMax = Vector2.one;
            dmgLayer.offsetMin = Vector2.zero;
            dmgLayer.offsetMax = Vector2.zero;
            await DamageTextSystem.Instance.InitializeAsync(dmgLayer);
        }

        /// <summary>阶段5：启动预热（精选高频池）</summary>
        private void Prewarm()
        {
            PrewarmRegister.PrewarmAll().Forget();
        }
    }
}