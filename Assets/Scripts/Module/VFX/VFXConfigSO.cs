using System.Collections.Generic;
using UnityEngine;

namespace Endfield.Module.VFX
{
    [System.Serializable]
    public class VFXData
    {
        public string name;          // 帧事件参数
        public string prefabPath;    // Addressable 地址
        public string anchorName;    // 容器：支持路径 "Bip001/.../VFX_1a" 或名字 "VFX_1a"
    }

    [CreateAssetMenu(menuName = "Endfield/VFX/VFXConfig")]
    public class VFXConfigSO : ScriptableObject
    {
        public List<VFXData> vfxDatas = new List<VFXData>();
    }
}