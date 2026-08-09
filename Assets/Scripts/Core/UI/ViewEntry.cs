using System;
using System.IO;
using JetBrains.Annotations;

namespace Endfield.Core
{
    public class ViewEntry
    {
        // 表示 ViewType 可能为 null
        [CanBeNull] public Type ViewType { get; private set; }
        public string PrefabPath { get; private set; }
        public string Name { get; private set; }
        
        public ViewEntry(Type viewType, string prefabPath)
        {
            ViewType = viewType;
            PrefabPath = prefabPath;
            //从完整路径中提取文件名（不包括扩展名部分）
            Name = Path.GetFileNameWithoutExtension(prefabPath);
        }
    }
}