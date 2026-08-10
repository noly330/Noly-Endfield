using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endfield
{
    /// <summary>
    /// 【调试】按 Y 在世界原点 (0,0,0) 召唤 3 只怪兽（居中排开、间隔 2），
    /// 测试对象池 生成 → 战斗 → 死亡 → 回收 整条链。
    /// 用完可整个删除（文件 + 场景组件）。
    /// </summary>
    public class DebugSpawner : MonoBehaviour
    {
        [SerializeField] private string _prefabAddress = "Assets/Res/Prefab/Character/Enemy/怪兽.prefab";
        [SerializeField] private int _count = 3;
        [SerializeField] private float _spacing = 4f;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.yKey.wasPressedThisFrame)
            {
                SpawnDebug();
            }
        }

        private async void SpawnDebug()
        {
            for (int i = 0; i < _count; i++)
            {
                float x = (i - (_count - 1) * 0.5f) * _spacing;   // 居中排开：-2, 0, 2
                await EnemySpawner.Instance.SpawnAsync(_prefabAddress,
                    new Vector3(x, 0f, x), Quaternion.identity);
            }
        }
    }
}
