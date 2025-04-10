using Emilia.Node.Editor;
using Emilia.Reflection.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalGraphOperateHandle : GraphOperateHandle<EditorUniversalGraphAsset>
    {
        public override void OpenCreateNodeMenu(Vector2 mousePosition, CreateNodeContext createNodeContext = default)
        {
            Rect? screenPosition = smartValue.GetElementPanelOwnerObjectScreenPosition_Internal();
            if (screenPosition == null) return;

            smartValue.createNodeMenu.MenuCreateInitialize(createNodeContext);

            NodeCreationContext nodeCreationContext = new NodeCreationContext {
                screenMousePosition = screenPosition.Value.position + mousePosition,
                index = -1,
            };

            smartValue.createNodeMenu.ShowCreateNodeMenu(nodeCreationContext);
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