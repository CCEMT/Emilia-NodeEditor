using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Node.Editor;
using Emilia.Reflection.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalGraphSelectedHandle : GraphSelectedHandle<EditorUniversalGraphAsset>
    {
        public override void Initialize(object weakSmartValue)
        {
            base.Initialize(weakSmartValue);
            bool isUseSelection = smartValue.window.GetType() != InspectorWindow_Internals.inspectorWindowType_Internals;
            if (isUseSelection == false) return;
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            List<Object> selectedInspectors = new List<Object>();

            foreach (ISelectable selectable in smartValue.selection)
            {
                ISelectedHandle selectableElement = selectable as ISelectedHandle;
                if (selectableElement == null) continue;
                selectedInspectors.AddRange(selectableElement.GetSelectedObjects());
            }

            bool isGraphSelect = Selection.objects.Any(selectedObject => selectedInspectors.Contains(selectedObject));
            if (isGraphSelect == false) smartValue.ClearSelection();
        }

        public override void UpdateSelectedInspector(List<ISelectedHandle> selection)
        {
            List<Object> selectedInspectors = new List<Object>();

            foreach (ISelectedHandle selectable in selection) selectedInspectors.AddRange(selectable.GetSelectedObjects());

            bool isUseSelection = smartValue.window.GetType() != InspectorWindow_Internals.inspectorWindowType_Internals;

            if (selectedInspectors.Count > 0)
            {
                if (isUseSelection)
                {
                    int selectedCount = selectedInspectors.Count;
                    for (int i = 0; i < selectedCount; i++)
                    {
                        Object selectedObject = selectedInspectors[i];
                        if (selectedObject == null) continue;
                        SelectedOwnerUtility.SetSelectedOwner(selectedObject, smartValue);
                        SelectedOwnerUtility.Update();
                    }

                    Selection.objects = selectedInspectors.ToArray();
                }
                else
                {
                    InspectorView inspectorView = smartValue.graphPanelSystem.GetPanel<InspectorView>();
                    if (inspectorView == null) inspectorView = smartValue.graphPanelSystem.OpenFloatPanel<InspectorView>();
                    inspectorView.SetObjects(selectedInspectors);
                }
            }
            else
            {
                if (isUseSelection)
                {
                    Selection.objects = null;
                    SelectedOwnerUtility.Update();
                }
                else smartValue.graphPanelSystem.ClosePanel<InspectorView>();
            }
        }

        public override void Dispose()
        {
            bool isUseSelection = smartValue.window.GetType() != InspectorWindow_Internals.inspectorWindowType_Internals;
            if (isUseSelection) Selection.selectionChanged -= OnSelectionChanged;

            base.Dispose();
            Selection.objects = null;
            SelectedOwnerUtility.Update();
        }
    }
}