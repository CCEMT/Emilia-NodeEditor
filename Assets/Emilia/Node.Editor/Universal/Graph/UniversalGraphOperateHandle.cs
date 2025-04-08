using Emilia.Node.Editor;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalGraphOperateHandle : GraphOperateHandle<EditorUniversalGraphAsset>
    {
        public override void OpenCreateNodeMenu(Vector2 mousePosition, CreateNodeContext createNodeContext = default)
        {
            smartValue.createNodeMenu.MenuCreateInitialize(createNodeContext);
            smartValue.RequestNodeCreation_Internals(null, -1, mousePosition);
        }

        public override void Cut()
        {
            smartValue.CutSelectionCallback_Internals();
        }

        public override void Copy()
        {
            smartValue.CopySelectionCallback_Internals();
        }

        public override void Paste(Vector2? mousePosition = null)
        {
            if (mousePosition == null) smartValue.PasteCallback_Internals();
            else smartValue.graphCopyPaste.UnserializeAndPasteCallback("Paste", smartValue.GetSerializedData_Internal());
        }

        public override void Delete()
        {
            smartValue.DeleteSelectionCallback_Internals();
        }

        public override void Duplicate()
        {
            smartValue.DuplicateSelectionCallback_Internals();
        }

        public override void Save()
        {
            smartValue.Save();
        }
    }
}