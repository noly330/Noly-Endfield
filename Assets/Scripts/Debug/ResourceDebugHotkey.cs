#if UNITY_EDITOR
using Endfield.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endfield
{
    /// <summary>
    /// 【编辑器调试】按 P 打印当前所有资源的引用计数。
    /// 仅编辑器生效：整个文件被 UNITY_EDITOR 包住，打包后会被完全剔除。
    /// </summary>
    public sealed class ResourceDebugHotkey : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject("[ResourceDebugHotkey]");
            go.hideFlags = HideFlags.DontSave;
            DontDestroyOnLoad(go);
            go.AddComponent<ResourceDebugHotkey>();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;
            if (!Keyboard.current.pKey.wasPressedThisFrame) return;

            Debug.Log($"[ResourceDebug] 资源引用计数：\n{ResourcesLoader.Instance.DebugDumpRefCounts()}");
        }
    }
}
#endif
