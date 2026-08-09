using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Endfield.Editor
{
    /// <summary>
    /// UI 一键生成工具：选中 prefab 后，
    /// 1) 将 prefab 加入 "UI Prefab" Addressable 分组（address = 完整路径）
    /// 2) 自动把 ViewEntry 写入 UIRegister.cs 并更新 All 列表
    /// </summary>
    public static class UIGenerator
    {
        private const string PrefabGroupName = "UI Prefab";

        [MenuItem("Tools/Generate UI")]
        public static void GenerateUI()
        {
            var selectedPrefab = Selection.gameObjects.FirstOrDefault();
            if (selectedPrefab == null)
            {
                Debug.LogWarning("[UI Generator]: 生成失败, 未选中预制体!");
                return;
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[UI Generator]: 未找到 AddressableAssetSettings!");
                return;
            }

            var prefabPath = AssetDatabase.GetAssetPath(selectedPrefab);
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError($"[UI Generator]: {selectedPrefab.name} 路径不存在!");
                return;
            }

            var uiGroup = settings.groups.FirstOrDefault(group => group.Name.Equals(PrefabGroupName));
            if (uiGroup == null)
            {
                Debug.LogError($"[UI Generator]: 找不到 '{PrefabGroupName}' Group!");
                return;
            }

            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(prefabPath), uiGroup);
            if (entry != null)
            {
                entry.address = prefabPath;
                entry.labels.Add("Prefab");
                Debug.Log($"[UI Generator]: 已添加 {selectedPrefab.name}");
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);

            var guids = AssetDatabase.FindAssets("UIRegister t:Script");
            if (guids.Length == 0)
            {
                Debug.LogError("[UI Generator]: 找不到 UIRegister.cs!");
                return;
            }

            var uiRegisterPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            UpdateUIRegister(uiRegisterPath, prefabPath);
        }

        private static void UpdateUIRegister(string filePath, string prefabPath)
        {
            var lines = File.ReadAllLines(filePath).ToList();
            var prefabName = Path.GetFileNameWithoutExtension(prefabPath);
            var fieldName = MakeValidFieldName(prefabName);

            var entryLine =
                $"\t\tpublic static readonly ViewEntry {fieldName} = " +
                $"new(typeof({prefabName}), \"{prefabPath}\");";

            var fieldRegex = new Regex(
                $@"public\s+static\s+readonly\s+ViewEntry\s+{Regex.Escape(fieldName)}\s*=");
            if (lines.Any(l => fieldRegex.IsMatch(l)))
            {
                Debug.Log($"[UI Generator] {fieldName} 已注册，跳过");
                return;
            }

            var insertIndex = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().StartsWith("public static readonly ViewEntry"))
                {
                    insertIndex = i + 1;
                }
            }

            if (insertIndex >= 0)
            {
                lines.Insert(insertIndex, entryLine);
            }
            else
            {
                var classStart = lines.FindIndex(l => l.Trim().StartsWith("public static class UIRegister"));
                if (classStart >= 0)
                {
                    lines.Insert(classStart + 1, entryLine);
                }
            }

            UpdateAllList(lines, fieldName);

            File.WriteAllLines(filePath, lines);
            AssetDatabase.Refresh();
        }

        private static void UpdateAllList(List<string> lines, string newFieldName)
        {
            var allStart = -1;
            var allEnd = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                var trimmed = lines[i].Trim();
                if (trimmed.Contains("IReadOnlyList<ViewEntry> All") ||
                    trimmed.Contains("ViewEntry[] All"))
                {
                    allStart = i;
                }

                if (allStart >= 0 && trimmed == "};")
                {
                    allEnd = i;
                    break;
                }
            }

            if (allStart < 0 || allEnd < 0)
            {
                var lastBrace = lines.FindLastIndex(l => l.Trim() == "}");
                if (lastBrace >= 0)
                {
                    lines.Insert(lastBrace, "");
                    lines.Insert(lastBrace,
                        "\t\tpublic static IReadOnlyList<ViewEntry> All { get; } = new ViewEntry[]");
                    lines.Insert(lastBrace + 1, "\t\t{");
                    lines.Insert(lastBrace + 2, $"\t\t\t{newFieldName},");
                    lines.Insert(lastBrace + 3, "\t\t};");
                }

                return;
            }

            for (int i = allStart; i < allEnd; i++)
            {
                if (lines[i].Contains(newFieldName))
                    return;
            }

            lines.Insert(allEnd, $"\t\t\t{newFieldName},");
        }

        private static string MakeValidFieldName(string prefabName)
        {
            var validName = Regex.Replace(prefabName, @"[^a-zA-Z0-9_]", "_");
            if (char.IsDigit(validName[0]))
                validName = "_" + validName;
            if (!Regex.IsMatch(validName, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                validName = "_" + validName;
            return validName;
        }
    }
}
