using System;
using System.Collections.Generic;
using System.Linq;
using Endfield.Core;
using UnityEditor;
using UnityEngine;

namespace Endfield.Editor
{
    /// <summary>
    /// 资源调试面板：查看 ResourcesLoader 当前的缓存与引用计数。
    /// 菜单：Tools/Resource Debug Window
    /// </summary>
    public class ResourceDebugWindow : EditorWindow
    {
        private string _search = string.Empty;
        private bool _autoRefresh = true;
        private bool _sortByRefCount = true;
        private Vector2 _scroll;

        [MenuItem("Tools/Resource Debug Window")]
        private static void Open()
        {
            var window = GetWindow<ResourceDebugWindow>("Resource Debug");
            window.minSize = new Vector2(760f, 320f);
            window.Show();
        }

        private void OnInspectorUpdate()
        {
            if (_autoRefresh && EditorApplication.isPlaying)
                Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (!EditorApplication.isPlaying)
                EditorGUILayout.HelpBox("进入 Play 模式后查看运行时资源引用计数。", MessageType.Info);

            var entries = GetEntries();
            DrawSummary(entries);
            DrawHeader();
            DrawRows(entries);
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("搜索", GUILayout.Width(36));
                _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField, GUILayout.MinWidth(200));

                if (GUILayout.Button("清空", EditorStyles.toolbarButton, GUILayout.Width(44)))
                    _search = string.Empty;

                GUILayout.FlexibleSpace();

                _sortByRefCount = GUILayout.Toggle(_sortByRefCount, "按引用数", EditorStyles.toolbarButton, GUILayout.Width(78));
                _autoRefresh = GUILayout.Toggle(_autoRefresh, "自动刷新", EditorStyles.toolbarButton, GUILayout.Width(78));

                if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(44)))
                    Repaint();
            }
        }

        private static void DrawSummary(List<ResourcesLoader.DebugEntry> entries)
        {
            int cached = entries.Count(e => e.Cached);
            int totalRefs = entries.Sum(e => e.RefCount);
            int shared = entries.Count(e => e.RefCount > 1);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label($"条目 {entries.Count}", GUILayout.Width(90));
                GUILayout.Label($"缓存 {cached}", GUILayout.Width(90));
                GUILayout.Label($"总引用 {totalRefs}", GUILayout.Width(100));
                GUILayout.Label($"共享(>1) {shared}", GUILayout.Width(100));
                GUILayout.FlexibleSpace();
                GUILayout.Label(EditorApplication.isPlaying ? "Play 中" : "Edit 模式", GUILayout.Width(80));
            }
        }

        private static void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("引用", GUILayout.Width(40));
                GUILayout.Label("缓存", GUILayout.Width(90));
                GUILayout.Label("资源路径", GUILayout.ExpandWidth(true));
                GUILayout.Label("操作", GUILayout.Width(110));
            }
        }

        private void DrawRows(List<ResourcesLoader.DebugEntry> entries)
        {
            if (entries.Count == 0)
            {
                EditorGUILayout.LabelField("没有资源记录。", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            using var scroll = new EditorGUILayout.ScrollViewScope(_scroll, GUILayout.ExpandHeight(true));
            _scroll = scroll.scrollPosition;

            foreach (var entry in entries)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var oldColor = GUI.color;

                    GUI.color = entry.RefCount > 1 ? new Color(1f, 0.6f, 0.2f) : Color.white;
                    GUILayout.Label(entry.RefCount.ToString(), GUILayout.Width(40));

                    GUI.color = entry.Cached ? new Color(0.5f, 0.9f, 0.5f) : new Color(1f, 0.45f, 0.45f);
                    GUILayout.Label(entry.Cached ? entry.Status.ToString() : "Missing", GUILayout.Width(90));

                    GUI.color = oldColor;
                    EditorGUILayout.SelectableLabel(entry.Path, GUILayout.ExpandWidth(true), GUILayout.Height(18));

                    using (new EditorGUILayout.HorizontalScope(GUILayout.Width(110)))
                    {
                        if (GUILayout.Button("定位", GUILayout.Width(50)))
                        {
                            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(entry.Path);
                            if (asset != null)
                            {
                                Selection.activeObject = asset;
                                EditorGUIUtility.PingObject(asset);
                            }
                        }

                        if (GUILayout.Button("复制", GUILayout.Width(50)))
                            EditorGUIUtility.systemCopyBuffer = entry.Path;
                    }
                }
            }
        }

        private List<ResourcesLoader.DebugEntry> GetEntries()
        {
            var entries = ResourcesLoader.Instance.DebugGetEntries();

            if (!string.IsNullOrEmpty(_search))
                entries = entries.Where(e => e.Path.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            if (_sortByRefCount)
                entries.Sort((a, b) => b.RefCount.CompareTo(a.RefCount));
            else
                entries.Sort((a, b) => string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase));

            return entries;
        }
    }
}
