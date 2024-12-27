using System;
using System.Collections.Generic;
using Emilia.Node.Editor;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public static partial class LayoutUtility
    {
        public static void SetGraph(EditorGraphView graphView, GeometryGraph geometryGraph, Func<Microsoft.Msagl.Core.Layout.Node> nodeFunc = null, Func<Edge> edgeFunc = null)
        {
            Dictionary<IEditorNodeView, Microsoft.Msagl.Core.Layout.Node> nodeMap = new Dictionary<IEditorNodeView, Microsoft.Msagl.Core.Layout.Node>();

            int nodeAmount = graphView.nodeViews.Count;
            for (int i = 0; i < nodeAmount; i++)
            {
                IEditorNodeView nodeView = graphView.nodeViews[i];

                string id = nodeView.asset.id;
                Rect rect = nodeView.asset.position;

                ICurve curve = CurveFactory.CreateRectangle(rect.width, rect.height, new Point() {X = rect.x, Y = rect.y});

                Microsoft.Msagl.Core.Layout.Node layoutNode = new Microsoft.Msagl.Core.Layout.Node(curve, id);
                if (nodeFunc != null)
                {
                    layoutNode = nodeFunc.Invoke();
                    layoutNode.BoundaryCurve = curve;
                    layoutNode.UserData = id;
                }

                layoutNode.UserData = nodeView;

                geometryGraph.Nodes.Add(layoutNode);

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

                Edge edge = new Edge(outputLayoutNode, inputLayoutNode);
                if (edgeFunc != null)
                {
                    edge = edgeFunc.Invoke();
                    edge.Source = outputLayoutNode;
                    edge.Target = inputLayoutNode;
                }

                geometryGraph.Edges.Add(edge);
            }
        }
    }
}