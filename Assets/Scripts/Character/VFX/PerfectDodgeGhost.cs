using UnityEngine;
using UnityEngine.Rendering;

namespace Endfield
{
    /// <summary>
    /// 残影/轮廓 ghost：显示一个烘焙网格（SkinnedMeshRenderer.BakeMesh 快照）。
    /// 自包含：Awake 自动补 MeshFilter/MeshRenderer/蓝白材质、关阴影，prefab 只需挂本脚本即可。
    /// 由 PerfectDodgeVisual 从池里取用：BakeMesh 到 GetMesh() → Show 贴体 → SetAlpha 淡出 → 回池。
    /// 材质用实例（不污染共享材质），alpha 可单独控制。
    /// </summary>
    public class PerfectDodgeGhost : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Material _matInstance;
        private Mesh _mesh;

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter == null) _meshFilter = gameObject.AddComponent<MeshFilter>();

            _meshRenderer = GetComponent<MeshRenderer>();
            if (_meshRenderer == null) _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;   // 残影不投影
            _meshRenderer.receiveShadows = false;
        }

        /// <summary>本 ghost 自己的烘焙网格（每次被占用时 BakeMesh 覆盖复用）。</summary>
        public Mesh GetMesh() => _mesh != null ? _mesh : (_mesh = new Mesh());

        /// <summary>贴到指定位置/旋转并设透明度（网格已由调用方 BakeMesh 进 GetMesh()）。</summary>
        public void Show(Vector3 pos, Quaternion rot, float alpha)
        {
            transform.SetPositionAndRotation(pos, rot);
            if (_meshFilter != null) _meshFilter.mesh = GetMesh();
            EnsureMaterial();
            SetAlpha(alpha);
        }

        public void SetAlpha(float alpha)
        {
            if (_matInstance != null) _matInstance.SetFloat("_Alpha", Mathf.Clamp01(alpha));
        }

        private void EnsureMaterial()
        {
            if (_matInstance != null) return;
            if (_meshRenderer == null) return;
            if (_meshRenderer.sharedMaterial == null)
            {
                var shader = Shader.Find("Endfield/PerfectDodgeGhost");
                if (shader != null) _meshRenderer.sharedMaterial = new Material(shader);
            }
            if (_meshRenderer.sharedMaterial != null)
                _matInstance = new Material(_meshRenderer.sharedMaterial);
        }
    }
}
