using Cysharp.Threading.Tasks;
using Endfield.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace Endfield.Module.UI
{
    /// <summary>
    /// 干员 3D 预览：预览相机把角色渲染进 RenderTexture，UI 的 RawImage 显示。
    /// 角色实例放专用层（Preview = 8），主相机运行时排除该层 → 与主世界角色互不干扰。
    /// 背景透明（alpha=0），透出 UI 面板自带的底；朝向正面。
    /// 舞台/相机/灯光/RT 全部代码创建：打开视图时懒初始化，隐藏时停渲染，销毁时清理。
    /// </summary>
    public class OperatorPreview
    {
        public const int PreviewLayer = 8;

        // 舞台放游戏区上方，避开物理与主相机视线（主相机已排除 Preview 层）
        private static readonly Vector3 StagePosition = new(0f, 20f, 0f);

        private static readonly string[] DisableTypeNames =
        {
            "CharacterPlayerController",
            "CharacterAIController",
            "BehaviorTree",
            "CharacterLink",
        };

        private static bool _mainCamExcluded;

        private Transform _stage;
        private Transform _mount;
        private Camera _camera;
        private PreviewCameraRig _rig;
        private RenderTexture _rt;
        private GameObject _character;
        private string _loadedAddress;
        private int _displayVersion;

        public RenderTexture TargetTexture => _rt;

        /// <summary>设置 RT 尺寸（匹配 RawImage 比例，防变形）。</summary>
        public void SetRenderTextureSize(int width, int height)
        {
            EnsureStage();

            int w = Mathf.Clamp(width, 128, 2048);
            int h = Mathf.Clamp(height, 128, 2048);
            if (_rt != null) _rt.Release();
            _rt = new RenderTexture(w, h, 16, RenderTextureFormat.ARGB32);   // 带 alpha，透明背景
            _camera.targetTexture = _rt;
        }

        /// <summary>展示某干员：加载 prefab → 实例化到挂载点 → 设层 → 禁用行为组件。</summary>
        public async UniTask Display(OperatorSO so)
        {
            if (so == null) return;
            EnsureStage();

            int version = ++_displayVersion;
            ClearCharacter();
            _mount.localRotation = Quaternion.identity;

            var prefab = await ResourcesLoader.Instance.Load<GameObject>(so.prefabAddress);
            if (prefab == null) return;
            if (version != _displayVersion) return;   // 切换期间有过新请求，丢弃过期结果

            _character = Object.Instantiate(prefab, _mount);
            SetLayerRecursive(_character.transform, PreviewLayer);
            DisableGameplay(_character);
            _loadedAddress = so.prefabAddress;

            // 把相机对焦点设为该干员的 CameraBasePoint（找不到用角色根，极少见）
            if (_rig != null)
            {
                var focus = FindByName(_character.transform, "HeadPoint");
                _rig.target = focus != null ? focus : _character.transform;
            }
        }

        private static Transform FindByName(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                var found = FindByName(child, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>显隐：隐藏时停相机 + 隐藏角色，省 GPU。</summary>
        public void SetVisible(bool visible)
        {
            if (_camera != null) _camera.enabled = visible;
            if (_character != null) _character.SetActive(visible);
        }

        public void Dispose()
        {
            ClearCharacter();
            if (_rt != null) { _rt.Release(); _rt = null; }
            if (_stage != null) { Object.Destroy(_stage.gameObject); _stage = null; }
            if (_loadedAddress != null)
            {
                ResourcesLoader.Instance.Release(_loadedAddress);
                _loadedAddress = null;
            }
        }

        private void EnsureStage()
        {
            if (_stage != null) return;

            _stage = new GameObject("[OperatorPreview]").transform;
            _stage.position = StagePosition;

            // 挂载点：角色实例的父级
            _mount = new GameObject("CharMount").transform;
            _mount.SetParent(_stage, false);

            // 预览相机：只渲染 Preview 层 → 透明 RT
            var camGo = new GameObject("PreviewCamera");
            camGo.transform.SetParent(_stage, false);
            _camera = camGo.AddComponent<Camera>();
            var camData = camGo.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = false;                          // 关后处理，避免改写 alpha → 背景不透明
            _camera.cullingMask = 1 << PreviewLayer;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0f, 0f, 0f, 0f);           // alpha=0 → 背景透明
            _camera.allowHDR = false;
            _camera.depth = -100;

            // 相机调参组件：运行时可用 Inspector 实时改 offset/fov，或点"记录当前调参"存默认值
            _rig = camGo.AddComponent<PreviewCameraRig>();

            // 灯光
            var lightGo = new GameObject("PreviewLight");
            lightGo.transform.SetParent(_stage, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 主相机排除 Preview 层（只需一次）
            if (!_mainCamExcluded)
            {
                var main = Camera.main;
                if (main != null) main.cullingMask &= ~(1 << PreviewLayer);
                _mainCamExcluded = true;
            }
        }

        private void ClearCharacter()
        {
            if (_character != null) Object.Destroy(_character);
            _character = null;
        }

        private static void SetLayerRecursive(Transform t, int layer)
        {
            t.gameObject.layer = layer;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursive(t.GetChild(i), layer);
        }

        /// <summary>
        /// 禁用战斗/AI 行为组件，让角色只站定播 Idle（默认动画状态）。
        /// TODO: 全局时间管理（TimeManager）上线后，这里接入统一的时间暂停接口。
        /// </summary>
        private static void DisableGameplay(GameObject root)
        {
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                foreach (var name in DisableTypeNames)
                {
                    if (mb.GetType().Name == name)
                    {
                        mb.enabled = false;
                        break;
                    }
                }
            }

            var cc = root.GetComponentInChildren<CharacterController>(true);
            if (cc != null) cc.enabled = false;
        }
    }
}
