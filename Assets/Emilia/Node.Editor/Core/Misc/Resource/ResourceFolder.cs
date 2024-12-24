using System;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector;

namespace Emilia.Node.Editor
{
    [Serializable]
    public class ResourceFolder
    {
        [LabelText("路径过滤")]
        public string pathFilter;

        [LabelText("文件夹")]
        public FolderAsset folderAsset;
    }
}