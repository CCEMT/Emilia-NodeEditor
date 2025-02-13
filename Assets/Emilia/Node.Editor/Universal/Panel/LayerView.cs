using System.Collections.Generic;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class LayerView : GraphPanel
    {
        public static readonly GUIStyle BreadCrumbLeft = "GUIEditor.BreadcrumbLeft";
        public static readonly GUIStyle BreadCrumbMid = "GUIEditor.BreadcrumbMid";
        public static readonly GUIStyle BreadCrumbLeftBg = "GUIEditor.BreadcrumbLeftBackground";
        public static readonly GUIStyle BreadCrumbMidBg = "GUIEditor.BreadcrumbMidBackground";

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            name = nameof(LayerView);

            Add(new IMGUIContainer(OnImGUI));

            if (parentView != null) parentView.canResizable = false;
        }

        private void OnImGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            Stack<EditorGraphAsset> graphAssets = new Stack<EditorGraphAsset>();

            EditorGraphAsset current = graphView.graphAsset;
            while (current != null)
            {
                graphAssets.Push(current);
                current = current.parent as EditorGraphAsset;
            }

            int i = 0;
            while (graphAssets.Count > 0)
            {
                EditorGraphAsset graphAsset = graphAssets.Pop();

                GUIStyle style1 = i == 0 ? BreadCrumbLeft : BreadCrumbMid;
                GUIStyle style2 = i == 0 ? BreadCrumbLeftBg : BreadCrumbMidBg;

                string label = graphAsset.name;
                GUIContent guiContent = new GUIContent(label);
                Rect rect = GetLayoutRect(guiContent, style1);
                if (Event.current.type == EventType.Repaint) style2.Draw(rect, GUIContent.none, 0);

                if (GUI.Button(rect, guiContent, style1))
                {
                    if (graphView.graphAsset != graphAsset) graphView.Reload(graphAsset);
                }

                i++;
            }

            GUILayout.EndHorizontal();
        }

        private Rect GetLayoutRect(GUIContent content, GUIStyle style)
        {
            Texture image = content.image;
            content.image = null;
            Vector2 vector = style.CalcSize(content);
            content.image = image;
            if (image != null) vector.x += vector.y;
            GUILayoutOption[] options = {GUILayout.MaxWidth(vector.x)};
            return GUILayoutUtility.GetRect(content, style, options);
        }
    }
}