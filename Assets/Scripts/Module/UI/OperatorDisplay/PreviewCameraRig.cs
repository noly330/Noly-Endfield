using UnityEngine;

namespace Endfield.Module.UI
{
    /// <summary>
    /// 预览相机调参组件：挂着 PreviewCamera 上，每帧按"对焦点 + offset"摆放相机并 LookAt。
    /// 使用时在 Inspector 实时改 offset/fov 即生效（LateUpdate 每帧应用）。
    /// 点 CustomEditor 的"记录当前调参"按钮会把当前值存进 EditorPrefs，下次启动自动作为默认值加载。
    /// </summary>
    public class PreviewCameraRig : MonoBehaviour
    {
        private const string KeyOffset = "NolyPreviewCamera.Offset";
        private const string KeyFov = "NolyPreviewCamera.Fov";
        private const string KeyRotation = "NolyPreviewCamera.Rotation";

        [Tooltip("当前干员的 CameraBasePoint")]
        public Transform target;
        [Tooltip("相机相对对焦点的偏移（方向+距离；y 别设太大，会俯视只剩头顶）")]
        public Vector3 offset = new(0f, 0f, 3.5f);
        [Tooltip("视野角度")]
        public float fov = 35f;
        [Tooltip("在 LookAt 对焦点基础上再叠加的角度偏移（俯仰/偏航，几何单位角度）")]
        public Vector3 rotation = Vector3.zero;

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
#if UNITY_EDITOR
            LoadRecorded();
#endif
        }

        private void LateUpdate()
        {
            if (target == null) return;

            transform.position = target.position + offset;

            // 只按"水平方向"朝向角色（算 yaw），高度不参与俯仰：
            // 这样改变 offset.y（抬高/降低相机）= 纯平移、不旋转；要俯仰用 rotation.x 手动调。
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(rotation);
            else
                transform.rotation = Quaternion.Euler(rotation);

            if (_camera != null) _camera.fieldOfView = fov;
        }

#if UNITY_EDITOR
        public const string OffsetKey = KeyOffset;
        public const string FovKey = KeyFov;
        public const string RotationKey = KeyRotation;

        /// <summary>把当前调好的参数记录到 EditorPrefs（由 CustomEditor 按钮调用）。</summary>
        public void SaveRecorded()
        {
            UnityEditor.EditorPrefs.SetString(OffsetKey, $"{offset.x:0.####},{offset.y:0.####},{offset.z:0.####}");
            UnityEditor.EditorPrefs.SetFloat(FovKey, fov);
            UnityEditor.EditorPrefs.SetString(RotationKey, $"{rotation.x:0.####},{rotation.y:0.####},{rotation.z:0.####}");
        }

        private void LoadRecorded()
        {
            if (UnityEditor.EditorPrefs.HasKey(OffsetKey))
            {
                var p = UnityEditor.EditorPrefs.GetString(OffsetKey).Split(',');
                if (p.Length == 3)
                    offset = new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
            }
            if (UnityEditor.EditorPrefs.HasKey(FovKey))
                fov = UnityEditor.EditorPrefs.GetFloat(FovKey);
            if (UnityEditor.EditorPrefs.HasKey(RotationKey))
            {
                var p = UnityEditor.EditorPrefs.GetString(RotationKey).Split(',');
                if (p.Length == 3)
                    rotation = new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2]));
            }
        }
#endif
    }
}
