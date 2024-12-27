using System;
using System.Threading.Tasks;
using Emilia.Kit;
using Emilia.Node.Editor;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Prototype.Phylo;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public static partial class LayoutUtility
    {
        /// <summary>
        /// 树形布局
        /// </summary>
        public static void TreeLayout(EditorGraphView graphView, SugiyamaLayoutSettings layoutSettings = null)
        {
            PhyloTree tree = new PhyloTree();

            SetGraph(graphView, tree, edgeFunc: () => new PhyloEdge(null, null));

            if (layoutSettings == null)
            {
                layoutSettings = new SugiyamaLayoutSettings() {
                    Transformation = PlaneTransformation.Rotation(Math.PI),
                    NodeSeparation = 20,
                    LayerSeparation = 50,
                };
            }

            graphView.window.ShowNotification(new GUIContent("正在计算布局"));

            Task.Run(OnLayout);

            void OnLayout()
            {
                LayoutHelpers.CalculateLayout(tree, layoutSettings, null);

                EditorKit.UnityInvoke(() => graphView.window.RemoveNotification());

                SetPosition(graphView, tree, 0.5f);
            }
        }
    }
}