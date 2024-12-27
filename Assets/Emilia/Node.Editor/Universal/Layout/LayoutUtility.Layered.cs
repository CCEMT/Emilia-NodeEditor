using System;
using System.Threading.Tasks;
using Emilia.Kit;
using Emilia.Node.Editor;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Layout.Layered;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public static partial class LayoutUtility
    {
        /// <summary>
        /// 层级布局
        /// </summary>
        public static void LayeredLayout(EditorGraphView graphView, SugiyamaLayoutSettings layoutSettings = null, float time = 0.5f)
        {
            GeometryGraph layoutGraph = new GeometryGraph();

            SetGraph(graphView, layoutGraph);

            if (layoutSettings == null)
            {
                layoutSettings = new SugiyamaLayoutSettings {
                    Transformation = PlaneTransformation.Rotation(Math.PI / 2),
                    EdgeRoutingSettings = {EdgeRoutingMode = EdgeRoutingMode.StraightLine},
                    NodeSeparation = 20,
                    LayerSeparation = 50,
                };
            }

            LayeredLayout layout = new LayeredLayout(layoutGraph, layoutSettings);

            graphView.window.ShowNotification(new GUIContent("正在计算布局"));

            Task.Run(OnLayout);

            void OnLayout()
            {
                layout.Run();

                EditorKit.UnityInvoke(() => graphView.window.RemoveNotification());

                SetPosition(graphView, layoutGraph, time);
            }
        }
    }
}