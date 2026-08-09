using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using Endfield.Core;

namespace Endfield.Editor
{
    /// <summary>
    /// Addressable 批量工具：
    /// 1) 扫描 TexturePath 目录下的资源加入 "UI Texture" 组
    /// 2) 反射读取 UIRegister 里的 ViewEntry，把对应 prefab 加入 "UI Prefab" 组
    /// </summary>
    public static class AddressableTools
    {
        private const string TextureGroupName = "UI Texture";
        private const string PrefabGroupName = "UI Prefab";
        private const string TexturePath = "Assets/Res/UI";

        [MenuItem("Tools/Addressable Tools/Add All To Addressable")]
        public static void AddAllToAddressableFromMenu() => AddFolderToAddressable();

        [MenuItem("Assets/Addressable Tools/Add Folder To Addressable", priority = 100)]
        public static void AddFolderToAddressable()
        {            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("未找到 Addressable Settings，请先通过 Groups 窗口创建。");
                return;
            }

            AddTextureToAddressable(settings);
            AddUIRegisterToAddressable(settings);
            AssetDatabase.SaveAssets();
        }

        private static void AddTextureToAddressable(AddressableAssetSettings settings)
        {
            if (!AssetDatabase.IsValidFolder(TexturePath)) return;

            AddressableAssetGroup targetGroup = GetOrCreateGroup(settings, TextureGroupName);

            string[] assetGUIDs = AssetDatabase.FindAssets("", new[] { TexturePath });
            int addCount = 0;

            foreach (string g in assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(g);
                if (AssetDatabase.IsValidFolder(assetPath)) continue;

                AddressableAssetEntry entry = settings.CreateOrMoveEntry(g, targetGroup);
                if (entry.address != assetPath)
                    entry.address = assetPath;
                entry.labels.Add("UI Resource");
                addCount++;
            }

            Debug.Log($"成功将 [{TexturePath}] 下的 {addCount} 个资源添加到组 [{TextureGroupName}]");
        }

        private static void AddUIRegisterToAddressable(AddressableAssetSettings settings)
        {
            AddressableAssetGroup targetGroup = GetOrCreateGroup(settings, PrefabGroupName);

            System.Type uiRegisterType = typeof(UIRegister);
            FieldInfo[] fields = uiRegisterType.GetFields(BindingFlags.Public | BindingFlags.Static);
            int addCount = 0;

            foreach (FieldInfo field in fields)
            {
                // 修复原版 bug：字段类型是 ViewEntry（不是 string），取它的 PrefabPath
                if (field.FieldType == typeof(ViewEntry) &&
                    field.GetValue(null) is ViewEntry ve &&
                    !string.IsNullOrEmpty(ve.PrefabPath))
                {
                    string assetPath = ve.PrefabPath;
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);
                    if (!string.IsNullOrEmpty(guid))
                    {
                        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, targetGroup);
                        if (entry.address != assetPath)
                            entry.address = assetPath;
                        entry.labels.Add("Prefab");
                        addCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"路径 {assetPath} 无效或不存在");
                    }
                }
            }

            Debug.Log($"成功将 UIRegister 中的 {addCount} 个资源添加到组 [{PrefabGroupName}]");
        }

        private static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string groupName)
        {
            AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group != null) return group;

            group = settings.CreateGroup(
                groupName,
                setAsDefaultGroup: false,
                readOnly: false,
                postEvent: false,
                schemasToCopy: new System.Collections.Generic.List<AddressableAssetGroupSchema>(settings.DefaultGroup.Schemas)
            );
            Debug.Log($"创建新的 Addressable Group: {groupName}");
            return group;
        }
    }
}
