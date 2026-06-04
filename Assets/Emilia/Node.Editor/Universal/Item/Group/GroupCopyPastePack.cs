using System;
using System.Collections.Generic;
using Emilia.Kit;
using Sirenix.Serialization;

namespace Emilia.Node.Editor
{
    /// <summary>
    /// 组拷贝粘贴
    /// </summary>
    public class GroupCopyPastePack : ItemCopyPastePack
    {
        [OdinSerialize, NonSerialized] private List<string> copyInnerNodes;

        public GroupCopyPastePack(EditorItemAsset asset) : base(asset)
        {
            EditorGroupAsset groupAsset = asset as EditorGroupAsset;
            copyInnerNodes = groupAsset == null ? new List<string>() : new List<string>(groupAsset.innerNodes);
        }

        public override bool CanDependency(ICopyPastePack pack)
        {
            INodeCopyPastePack nodeCopyPastePack = pack as INodeCopyPastePack;
            if (nodeCopyPastePack == null) return false;

            if (copyInnerNodes == null) return false;

            string copyNodeId = nodeCopyPastePack.copyAsset.id;
            if (copyInnerNodes.Contains(copyNodeId)) return true;

            return false;
        }

        protected override void PasteDependency(CopyPasteContext copyPasteContext)
        {
            EditorGroupAsset groupAsset = this._pasteAsset as EditorGroupAsset;
            groupAsset.innerNodes.Clear();

            int amount = copyPasteContext.dependency.Count;
            for (int i = 0; i < amount; i++)
            {
                ICopyPastePack pack = copyPasteContext.dependency[i];
                INodeCopyPastePack nodeCopyPastePack = pack as INodeCopyPastePack;
                if (nodeCopyPastePack == null) continue;
                groupAsset.innerNodes.Add(nodeCopyPastePack.pasteAsset.id);
            }
        }
    }
}