using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Endfield.Editor
{
    /// <summary>
    /// 一键把场景里的干员生成 prefab 并标记 Addressable（Operator 分组）。
    /// 约定：prefab 文件名 = OperatorSO.name，TeamManager 按此约定加载。
    /// </summary>
    public static class OperatorPrefabGenerator
    {
        private const string PrefabFolder = "Assets/Res/Prefab/Character/Operator";
        private const string GroupName = "Operator";

        [MenuItem("Tools/干员 Prefab 生成")]
        public static void GenerateOperatorPrefabs()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[干员生成] 未找到 Addressable Settings");
                return;
            }

            var group = GetOrCreateGroup(settings, GroupName);

            var ops = Object.FindObjectsOfType<Operator>(true);
            if (ops.Length == 0)
            {
                Debug.LogWarning("[干员生成] 场景里没有 Operator");
                return;
            }

            foreach (var op in ops)
            {
                if (op.OperatorData == null)
                {
                    Debug.LogWarning($"[干员生成] {op.name} 的 OperatorSO 为空，跳过");
                    continue;
                }

                string soName = op.OperatorData.name;
                string path = $"{PrefabFolder}/{soName}.prefab";
                var savedGo = PrefabUtility.SaveAsPrefabAsset(op.gameObject, path);
                if (savedGo == null)
                {
                    Debug.LogError($"[干员生成] 保存 prefab 失败: {path}");
                    continue;
                }

                // 新资产必须刷新进 AssetDatabase 后才能拿到 GUID
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                string guid = AssetDatabase.AssetPathToGUID(path);
                if (!string.IsNullOrEmpty(guid))
                {
                    var entry = settings.CreateOrMoveEntry(guid, group);
                    if (entry.address != path)
                        entry.address = path;
                    entry.labels.Add("Operator");
                }

                Debug.Log($"[干员生成] {op.name} → {path}");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[干员生成] 完成");
        }

        private static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string groupName)
        {
            var group = settings.FindGroup(groupName);
            if (group != null) return group;

            return settings.CreateGroup(groupName, false, false, false,
                new System.Collections.Generic.List<AddressableAssetGroupSchema>(settings.DefaultGroup.Schemas));
        }
    }
}
