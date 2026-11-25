using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    internal sealed class ReverseEdgeRouter : IDisposable
    {
        private readonly EditorGraphView graphView;
        private readonly Dictionary<EditorEdgeView, ReverseEdgeRequest> requests = new();
        private readonly Dictionary<EditorEdgeView, ReverseEdgePath> paths = new();
        private readonly Dictionary<EditorEdgeView, PortPair> edges = new();
        private readonly Dictionary<VisualElement, int> ports = new();
        private readonly HashSet<IEditorNodeView> nodes = new();
        private readonly HashSet<EditorEdgeView> changedViews = new();
        private IVisualElementScheduledItem pendingRefresh;
        private bool dirty;
        private bool rebuilding;
        private bool disposed;
        private int version;

        private readonly struct PortPair
        {
            internal readonly VisualElement input;
            internal readonly VisualElement output;

            internal PortPair(EditorEdgeView edge)
            {
                input = edge.inputPortView?.portElement;
                output = edge.outputPortView?.portElement;
            }
        }

        internal ReverseEdgeRouter(EditorGraphView graphView) => this.graphView = graphView;

        internal void TrackNode(IEditorNodeView node)
        {
            if (nodes.Add(node)) node.element.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            Invalidate();
        }

        internal void UntrackNode(IEditorNodeView node)
        {
            if (nodes.Remove(node)) node.element.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            Invalidate();
        }

        internal void TrackEdge(IEditorEdgeView view)
        {
            if (view is not EditorEdgeView edge || edges.ContainsKey(edge)) return;
            PortPair pair = new(edge);
            edges.Add(edge, pair);
            TrackPort(pair.input);
            TrackPort(pair.output);
            Invalidate();
        }

        internal void UntrackEdge(IEditorEdgeView view)
        {
            if (view is not EditorEdgeView edge || !edges.TryGetValue(edge, out PortPair pair)) return;
            UntrackPort(pair.input);
            UntrackPort(pair.output);
            edges.Remove(edge);
            changedViews.Remove(edge);
            Invalidate();
        }

        internal void Invalidate()
        {
            if (disposed) return;
            dirty = true;
            ScheduleRefresh();
        }

        private void ScheduleRefresh()
        {
            if (pendingRefresh != null || disposed) return;
            pendingRefresh = graphView.schedule.Execute(Refresh);
            pendingRefresh.ExecuteLater(1);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt) => Invalidate();

        internal ReverseEdgePath GetPath(EditorEdgeView edge)
        {
            if (disposed || rebuilding || !edges.ContainsKey(edge)) return null;
            bool hasRequest = TryGetRequest(edge, out ReverseEdgeRequest request);
            if (hasRequest != requests.ContainsKey(edge) || (hasRequest && !requests[edge].Matches(request))) Invalidate();
            if (dirty) Rebuild();
            return paths.TryGetValue(edge, out ReverseEdgePath path) ? path : null;
        }

        private void Refresh()
        {
            pendingRefresh = null;
            if (disposed) return;
            if (dirty) Rebuild();
            List<EditorEdgeView> views = new(changedViews);
            changedViews.Clear();
            foreach (EditorEdgeView edge in views)
            {
                if (edges.ContainsKey(edge) && edge.panel != null) edge.UpdateEdgeControl();
            }
        }

        private void Rebuild()
        {
            rebuilding = true;
            try
            {
                Dictionary<EditorEdgeView, ReverseEdgeRequest> current = new();
                HashSet<EditorEdgeView> changed = new();
                List<Rect> oldRegions = new();
                foreach (EditorEdgeView edge in new List<EditorEdgeView>(edges.Keys))
                {
                    UpdatePorts(edge);
                    if (!TryGetRequest(edge, out ReverseEdgeRequest request)) continue;
                    current.Add(edge, request);
                    if (!requests.TryGetValue(edge, out ReverseEdgeRequest previous) || !previous.Matches(request)) changed.Add(edge);
                }

                foreach (KeyValuePair<EditorEdgeView, ReverseEdgeRequest> previous in requests)
                {
                    if (current.ContainsKey(previous.Key) && !changed.Contains(previous.Key)) continue;
                    Rect region = ReverseEdgePathBuilder.Region(previous.Value);
                    if (paths.TryGetValue(previous.Key, out ReverseEdgePath path)) region = ReverseEdgePathBuilder.Union(region, path.bounds);
                    oldRegions.Add(region);
                    if (current.ContainsKey(previous.Key)) continue;
                    paths.Remove(previous.Key);
                    MarkChanged(previous.Key);
                }

                List<EditorEdgeView> remaining = new(current.Keys);
                while (remaining.Count > 0)
                {
                    List<EditorEdgeView> group = new() { remaining[remaining.Count - 1] };
                    remaining.RemoveAt(remaining.Count - 1);
                    Rect region = GetRegion(group[0], current[group[0]]);
                    bool expanded;
                    do
                    {
                        expanded = false;
                        for (int i = remaining.Count - 1; i >= 0; i--)
                        {
                            EditorEdgeView edge = remaining[i];
                            Rect candidate = GetRegion(edge, current[edge]);
                            if (!group.Exists(member => GetRegion(member, current[member]).Overlaps(candidate))) continue;
                            region = ReverseEdgePathBuilder.Union(region, candidate);
                            group.Add(edge);
                            remaining.RemoveAt(i);
                            expanded = true;
                        }
                    } while (expanded);

                    bool needsUpdate = group.Exists(changed.Contains) || oldRegions.Exists(region.Overlaps);
                    if (!needsUpdate) continue;
                    group.Sort((a, b) => Compare(current[a], current[b]));
                    List<ReverseEdgePath> occupied = new();
                    foreach (EditorEdgeView edge in group)
                    {
                        paths.TryGetValue(edge, out ReverseEdgePath previous);
                        ReverseEdgePath path = ReverseEdgePathBuilder.Choose(current[edge], occupied, previous, group.Count + 1);
                        if (!ReferenceEquals(path, previous))
                        {
                            path.version = ++version;
                            paths[edge] = path;
                            MarkChanged(edge);
                        }
                        occupied.Add(path);
                    }
                }

                requests.Clear();
                foreach (KeyValuePair<EditorEdgeView, ReverseEdgeRequest> request in current) requests.Add(request.Key, request.Value);
                dirty = false;
            }
            finally
            {
                rebuilding = false;
            }
        }

        private Rect GetRegion(EditorEdgeView edge, ReverseEdgeRequest request)
        {
            Rect region = ReverseEdgePathBuilder.Region(request);
            if (paths.TryGetValue(edge, out ReverseEdgePath path)) region = ReverseEdgePathBuilder.Union(region, ReverseEdgePathBuilder.Expand(path.bounds, ReverseEdgePathBuilder.LayerSpacing));
            return region;
        }

        private void MarkChanged(EditorEdgeView edge)
        {
            edge.editorEdgeControl.InvalidateRoute();
            changedViews.Add(edge);
            ScheduleRefresh();
        }

        private static int Compare(ReverseEdgeRequest a, ReverseEdgeRequest b)
        {
            float aSpan = Mathf.Abs(a.to.x - a.from.x) + Mathf.Abs(a.to.y - a.from.y);
            float bSpan = Mathf.Abs(b.to.x - b.from.x) + Mathf.Abs(b.to.y - b.from.y);
            int order = aSpan.CompareTo(bSpan);
            if (order == 0) order = a.outputOrder.CompareTo(b.outputOrder);
            if (order == 0) order = a.inputOrder.CompareTo(b.inputOrder);
            return order == 0 ? string.CompareOrdinal(a.id, b.id) : order;
        }

        private bool TryGetRequest(EditorEdgeView edge, out ReverseEdgeRequest request)
        {
            request = default;
            if (edge.asset == null || edge.isDrag || edge.isGhostEdge || edge.input == null || edge.output == null ||
                edge.inputPortView == null || edge.outputPortView == null || edge.editorEdgeControl.disabledEdgeDrawOptimization ||
                edge.inputPortView.editorOrientation == EditorOrientation.Custom || edge.outputPortView.editorOrientation == EditorOrientation.Custom) return false;
            Vector2 from = graphView.contentViewContainer.WorldToLocal(edge.output.GetGlobalCenter());
            Vector2 to = graphView.contentViewContainer.WorldToLocal(edge.input.GetGlobalCenter());
            Vector2 outputDirection = edge.outputPortView.editorOrientation == EditorOrientation.Vertical ? Vector2.up : Vector2.right;
            Vector2 inputDirection = edge.inputPortView.editorOrientation == EditorOrientation.Vertical ? Vector2.down : Vector2.left;
            if (!Finite(from) || !Finite(to) || !ReverseEdgePathBuilder.IsReverse(from, to, outputDirection, inputDirection)) return false;
            Rect outputBounds = graphView.contentViewContainer.WorldToLocal(edge.outputPortView.master.element.worldBound);
            Rect inputBounds = graphView.contentViewContainer.WorldToLocal(edge.inputPortView.master.element.worldBound);
            if (!Finite(outputBounds.min) || !Finite(outputBounds.max) || !Finite(inputBounds.min) || !Finite(inputBounds.max)) return false;
            request = new ReverseEdgeRequest(edge.asset.id, from, to, outputDirection, inputDirection, outputBounds, inputBounds,
                PortOrder(edge.outputPortView), PortOrder(edge.inputPortView));
            return true;
        }

        private static bool Finite(Vector2 point) => !float.IsNaN(point.x) && !float.IsNaN(point.y) && !float.IsInfinity(point.x) && !float.IsInfinity(point.y);

        private static int PortOrder(IEditorPortView port)
        {
            IReadOnlyList<IEditorPortView> siblings = port.master.portViews;
            for (int i = 0; i < siblings.Count; i++)
            {
                if (ReferenceEquals(siblings[i], port)) return i;
            }
            return 0;
        }

        private void UpdatePorts(EditorEdgeView edge)
        {
            PortPair previous = edges[edge];
            PortPair current = new(edge);
            if (previous.input == current.input && previous.output == current.output) return;
            UntrackPort(previous.input);
            UntrackPort(previous.output);
            TrackPort(current.input);
            TrackPort(current.output);
            edges[edge] = current;
        }

        private void TrackPort(VisualElement port)
        {
            if (port == null) return;
            if (ports.TryGetValue(port, out int count)) ports[port] = count + 1;
            else
            {
                ports.Add(port, 1);
                port.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
        }

        private void UntrackPort(VisualElement port)
        {
            if (port == null || !ports.TryGetValue(port, out int count)) return;
            if (count > 1) ports[port] = count - 1;
            else
            {
                ports.Remove(port);
                port.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
        }

        public void Dispose()
        {
            disposed = true;
            pendingRefresh?.Pause();
            pendingRefresh = null;
            foreach (IEditorNodeView node in nodes) node.element.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            foreach (VisualElement port in ports.Keys) port.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            nodes.Clear();
            ports.Clear();
            edges.Clear();
            requests.Clear();
            paths.Clear();
            changedViews.Clear();
        }
    }
}
