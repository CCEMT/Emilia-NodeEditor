using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Emilia.Node.Editor;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public static class LayoutUtility
    {
        public static void RunLayout(EditorGraphView graphView)
        {
            GeometryGraph layoutGraph = new GeometryGraph();

            Dictionary<IEditorNodeView, Microsoft.Msagl.Core.Layout.Node> nodeMap = new Dictionary<IEditorNodeView, Microsoft.Msagl.Core.Layout.Node>();

            int nodeAmount = graphView.nodeViews.Count;
            for (int i = 0; i < nodeAmount; i++)
            {
                IEditorNodeView nodeView = graphView.nodeViews[i];

                string id = nodeView.asset.id;
                Rect rect = nodeView.asset.position;

                ICurve curve = CurveFactory.CreateRectangle(rect.width, rect.height, new Point() {X = rect.x, Y = rect.y});

                Microsoft.Msagl.Core.Layout.Node layoutNode = new Microsoft.Msagl.Core.Layout.Node(curve, id);
                layoutNode.UserData = nodeView;

                layoutGraph.Nodes.Add(layoutNode);

                nodeMap[nodeView] = layoutNode;
            }

            int edgeAmount = graphView.edgeViews.Count;
            for (int i = 0; i < edgeAmount; i++)
            {
                IEditorEdgeView edgeView = graphView.edgeViews[i];

                IEditorNodeView inputNode = graphView.graphElementCache.nodeViewById.GetValueOrDefault(edgeView.asset.inputNodeId);
                IEditorNodeView outputNode = graphView.graphElementCache.nodeViewById.GetValueOrDefault(edgeView.asset.outputNodeId);

                Microsoft.Msagl.Core.Layout.Node inputLayoutNode = nodeMap[inputNode];
                Microsoft.Msagl.Core.Layout.Node outputLayoutNode = nodeMap[outputNode];

                Edge edge = new Edge(inputLayoutNode, outputLayoutNode);
                layoutGraph.Edges.Add(edge);
            }

            SugiyamaLayoutSettings settings = new SugiyamaLayoutSettings {
                Transformation = PlaneTransformation.Rotation(Math.PI / 2),
                EdgeRoutingSettings = {EdgeRoutingMode = EdgeRoutingMode.StraightLine},
                NodeSeparation = 20,
                LayerSeparation = 50,
            };

            LayeredLayout layout = new LayeredLayout(layoutGraph, settings);

            graphView.window.ShowNotification(new GUIContent("正在计算布局"));

            Task.Run(OnLayout);

            void OnLayout()
            {
                layout.Run();

                graphView.window.RemoveNotification();

                int layoutNodeAmount = layoutGraph.Nodes.Count;
                for (int i = 0; i < layoutNodeAmount; i++)
                {
                    Microsoft.Msagl.Core.Layout.Node layoutNode = layoutGraph.Nodes[i];
                    IEditorNodeView nodeView = layoutNode.UserData as IEditorNodeView;

                    Vector2 layoutPosition = new Vector2((float) layoutNode.Center.X, (float) layoutNode.Center.Y);
                    EditorCoroutineUtility.StartCoroutineOwnerless(SetPosition(nodeView, layoutPosition, 0.5f));
                }
            }

            IEnumerator SetPosition(IEditorNodeView nodeView, Vector2 layoutPosition, float time = 0.5f)
            {
                Vector2 startPosition = nodeView.asset.position.position;
                double startTime = EditorApplication.timeSinceStartup;
                while (EditorApplication.timeSinceStartup - startTime < time)
                {
                    float t = (float) ((EditorApplication.timeSinceStartup - startTime) / time);
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