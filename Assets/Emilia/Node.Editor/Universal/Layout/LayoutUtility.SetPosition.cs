using System.Collections;
using Emilia.Node.Editor;
using Microsoft.Msagl.Core.Layout;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public static partial class LayoutUtility
    {
        public static void SetPosition(EditorGraphView graphView, GeometryGraph layoutGraph, float time)
        {
            int layoutNodeAmount = layoutGraph.Nodes.Count;
            for (int i = 0; i < layoutNodeAmount; i++)
            {
                Microsoft.Msagl.Core.Layout.Node layoutNode = layoutGraph.Nodes[i];
                IEditorNodeView nodeView = layoutNode.UserData as IEditorNodeView;

                Vector2 layoutPosition = new Vector2((float) layoutNode.Center.X, (float) layoutNode.Center.Y);
                if (time <= 0) nodeView.SetPositionNoUndo(new Rect(layoutPosition, nodeView.asset.position.size));
                else EditorCoroutineUtility.StartCoroutineOwnerless(SetPositionAnimation(nodeView, layoutPosition, time));
            }

            IEnumerator SetPositionAnimation(IEditorNodeView nodeView, Vector2 layoutPosition, float moveTime)
            {
                Vector2 startPosition = nodeView.asset.position.position;
                double startTime = EditorApplication.timeSinceStartup;
                while (EditorApplication.timeSinceStartup - startTime < moveTime)
                {
                    float t = (float) ((EditorApplication.timeSinceStartup - startTime) / moveTime);
                    Vector2 currentPosition = Vector2.Lerp(startPosition, layoutPosition, t);
                    nodeView.SetPositionNoUndo(new Rect(currentPosition, nodeView.asset.position.size));
                    graphView.window.Repaint();
                    yield return 0;
                }

                nodeView.SetPositionNoUndo(new Rect(layoutPosition, nodeView.asset.position.size));
            }
        }
    }
}