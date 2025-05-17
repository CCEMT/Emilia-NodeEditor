using System.Linq;
using Emilia.Kit;
using Emilia.Node.Editor;
using Emilia.Reflection.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    [EditorHandle(typeof(EditorUniversalGraphAsset))]
    public class UniversalGraphOperateHandle : GraphOperateHandle
    {
        public override void OpenCreateNodeMenu(EditorGraphView graphView, Vector2 mousePosition, CreateNodeContext createNodeContext = default)
        {
            Rect? screenPosition = graphView.GetElementPanelOwnerObjectScreenPosition_Internal();
            if (screenPosition == null) return;

            graphView.createNodeMenu.MenuCreateInitialize(createNodeContext);

            NodeCreationContext nodeCreationContext = new NodeCreationContext {
                screenMousePosition = screenPosition.Value.position + mousePosition,
                index = -1,
            };

            graphView.createNodeMenu.ShowCreateNodeMenu(nodeCreationContext);
        }

        public override void Cut(EditorGraphView graphView)
        {
            base.Cut(graphView);
            graphView.CutSelectionCallback_Internals();
        }

        public override void Copy(EditorGraphView graphView)
        {
            base.Copy(graphView);
            graphView.CopySelectionCallback_Internals();
        }

        public override void Paste(EditorGraphView graphView, Vector2? mousePosition = null)
        {
            base.Paste(graphView, mousePosition);
            if (mousePosition == null) graphView.PasteCallback_Internals();
            else
            {
                var pasteObjects = graphView.graphCopyPaste.UnserializeAndPasteCallback("Paste", graphView.GetSerializedData_Internal(), mousePosition);
                graphView.graphSelected.UpdateSelected(pasteObjects.OfType<ISelectedHandle>().ToList());

                graphView.SetSelection(pasteObjects.OfType<ISelectable>().ToList());
                graphView.UpdateSelected();

                graphView.clipboard_Internal = graphView.graphCopyPaste.SerializeGraphElementsCallback(pasteObjects);
            }
        }

        public override void Delete(EditorGraphView graphView)
        {
            base.Delete(graphView);
            graphView.DeleteSelectionCallback_Internals();
        }

        public override void Duplicate(EditorGraphView graphView)
        {
            base.Duplicate(graphView);
            graphView.DuplicateSelectionCallback_Internals();
        }

        public override void Save(EditorGraphView graphView)
        {
            base.Save(graphView);
            graphView.Save();
        }
    }
}