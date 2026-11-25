using System;
using Emilia.Reflection.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public partial class EditorEdgeControl
    {
        internal EditorEdgeView owner { get; set; }

        private ReverseEdgePath routedPath;
        private ReverseEdgePath transientPath;
        private ReverseEdgePath lastRenderedPath;
        private VisualElement routeSpace;
        private Rect baseLayout;
        private Rect routedLayout;
        private Rect lastRenderedLayout;
        private bool routeLayoutActive;

        internal void InvalidateRoute()
        {
            renderPointsDirty_Internals = true;
            lastLocalControlPoints.Clear();
            lastRenderedPath = null;
            MarkDirtyRepaint();
        }

        private void InvalidateRoutingContext()
        {
            InvalidateRoute();
            owner?.graphView?.reverseEdgeRouter?.Invalidate();
        }

        public override void UpdateLayout()
        {
            base.UpdateLayout();
            if (parent == null) return;
            if (!routeLayoutActive || layout != routedLayout) baseLayout = layout;
            ResolveRoute();
            if (ApplyRouteLayout()) base.UpdateLayout();
        }

        private void ResolveRoute()
        {
            routedPath = null;
            if (parent == null || disabledEdgeDrawOptimization || inputEditorOrientation == EditorOrientation.Custom ||
                outputEditorOrientation == EditorOrientation.Custom || controlPoints == null || controlPoints.Length < 4) return;
            Vector2 from = controlPoints[0];
            Vector2 to = controlPoints[3];
            float validate = from.x + from.y + to.x + to.y;
            if (float.IsNaN(validate) || float.IsInfinity(validate) || !IsUnnaturalConnection(from, to, controlPoints[1], controlPoints[2])) return;

            routedPath = owner?.graphView?.reverseEdgeRouter?.GetPath(owner);
            if (routedPath != null)
            {
                routeSpace = owner.graphView.contentViewContainer;
                return;
            }

            Vector2 outputDirection = (controlPoints[1] - from).normalized;
            Vector2 inputDirection = (controlPoints[2] - to).normalized;
            Rect outputBounds = new(from, Vector2.zero);
            Rect inputBounds = new(to, Vector2.zero);
            if (owner?.output != null && owner.outputPortView != null) outputBounds = parent.WorldToLocal(owner.outputPortView.master.element.worldBound);
            if (owner?.input != null && owner.inputPortView != null) inputBounds = parent.WorldToLocal(owner.inputPortView.master.element.worldBound);
            ReverseEdgeRequest request = new(owner?.asset?.id, from, to, outputDirection, inputDirection, outputBounds, inputBounds);
            if (transientPath == null || !transientPath.request.Matches(request))
                transientPath = ReverseEdgePathBuilder.Choose(request, Array.Empty<ReverseEdgePath>(), transientPath, 2);
            routedPath = transientPath;
            routeSpace = parent;
        }

        private bool ApplyRouteLayout()
        {
            if (routedPath == null)
            {
                if (!routeLayoutActive) return false;
                if (layout == routedLayout) this.SetLayout_Internals(baseLayout);
                routeLayoutActive = false;
                InvalidateRoute();
                return true;
            }

            Rect bounds = routeSpace.ChangeCoordinatesTo(parent, routedPath.bounds);
            float padding = Mathf.Max(edgeWidth * 0.5f + 1f, interceptWidth);
            bounds = ReverseEdgePathBuilder.Expand(bounds, padding);
            bool changed = bounds != layout;
            if (changed)
            {
                this.SetLayout_Internals(bounds);
                renderPointsDirty_Internals = true;
            }
            routedLayout = bounds;
            routeLayoutActive = true;
            return changed;
        }

        private bool UpdateRouteRenderPoints()
        {
            ResolveRoute();
            ApplyRouteLayout();
            if (routedPath == null) return false;
            if (!renderPointsDirty_Internals && ReferenceEquals(lastRenderedPath, routedPath) && lastRenderedLayout == layout) return true;
            renderPoints_Internals.Clear();
            foreach (Vector2 point in routedPath.points) renderPoints_Internals.Add(routeSpace.ChangeCoordinatesTo(this, point));
            lastRenderedPath = routedPath;
            lastRenderedLayout = layout;
            lastLocalControlPoints.Clear();
            renderPointsDirty_Internals = false;
            return true;
        }

        internal bool TryGetPointByRate(float rate, out Vector2 point)
        {
            UpdateLayout();
            if (routedPath == null)
            {
                point = default;
                return false;
            }
            point = routeSpace.ChangeCoordinatesTo(parent, routedPath.GetPoint(rate));
            return true;
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            UpdateLayout();
            UpdateRenderPoints();
            if (routedPath == null) return base.ContainsPoint(localPoint);
            float capDistance = 4f * capRadius * capRadius;
            int last = renderPoints_Internals.Count - 1;
            if ((localPoint - renderPoints_Internals[0]).sqrMagnitude <= capDistance ||
                (localPoint - renderPoints_Internals[last]).sqrMagnitude <= capDistance) return false;
            float distance = interceptWidth * interceptWidth;
            for (int i = 1; i <= last; i++)
            {
                if (ReverseEdgePathBuilder.DistanceToSegmentSquared(localPoint, renderPoints_Internals[i - 1], renderPoints_Internals[i]) <= distance) return true;
            }
            return false;
        }

        public override bool Overlaps(Rect rect)
        {
            UpdateLayout();
            UpdateRenderPoints();
            return base.Overlaps(rect);
        }
    }
}
