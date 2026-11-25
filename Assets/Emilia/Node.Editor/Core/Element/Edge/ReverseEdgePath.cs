using System;
using System.Collections.Generic;
using UnityEngine;

namespace Emilia.Node.Editor
{
    internal readonly struct ReverseEdgeRequest
    {
        internal readonly string id;
        internal readonly Vector2 from;
        internal readonly Vector2 to;
        internal readonly Vector2 outputDirection;
        internal readonly Vector2 inputDirection;
        internal readonly Rect outputBounds;
        internal readonly Rect inputBounds;
        internal readonly int outputOrder;
        internal readonly int inputOrder;

        internal ReverseEdgeRequest(string id, Vector2 from, Vector2 to, Vector2 outputDirection, Vector2 inputDirection,
            Rect outputBounds, Rect inputBounds, int outputOrder = 0, int inputOrder = 0)
        {
            this.id = id;
            this.from = from;
            this.to = to;
            this.outputDirection = outputDirection.normalized;
            this.inputDirection = inputDirection.normalized;
            this.outputBounds = outputBounds;
            this.inputBounds = inputBounds;
            this.outputOrder = outputOrder;
            this.inputOrder = inputOrder;
        }

        internal bool Matches(ReverseEdgeRequest other)
        {
            return id == other.id && Near(from, other.from) && Near(to, other.to) &&
                   Near(outputDirection, other.outputDirection) && Near(inputDirection, other.inputDirection) &&
                   Near(outputBounds.min, other.outputBounds.min) && Near(outputBounds.max, other.outputBounds.max) &&
                   Near(inputBounds.min, other.inputBounds.min) && Near(inputBounds.max, other.inputBounds.max) &&
                   outputOrder == other.outputOrder && inputOrder == other.inputOrder;
        }

        private static bool Near(Vector2 a, Vector2 b) => (a - b).sqrMagnitude < 0.0001f;
    }

    internal enum ReverseEdgeSide
    {
        Gap,
        Before,
        After
    }

    internal readonly struct ReverseEdgePlacement
    {
        internal readonly ReverseEdgeSide side;
        internal readonly int layer;

        internal ReverseEdgePlacement(ReverseEdgeSide side, int layer)
        {
            this.side = side;
            this.layer = layer;
        }
    }

    internal readonly struct ReverseEdgeBezier
    {
        internal readonly Vector2 p0;
        internal readonly Vector2 p1;
        internal readonly Vector2 p2;
        internal readonly Vector2 p3;

        internal ReverseEdgeBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }

    internal sealed class ReverseEdgePath
    {
        private const float SampleToleranceSquared = 0.0625f;

        internal readonly ReverseEdgeRequest request;
        internal readonly ReverseEdgePlacement placement;
        internal readonly ReverseEdgeBezier[] curves;
        internal readonly Vector2[] points;
        internal readonly Rect bounds;
        internal readonly float length;
        internal int version;

        private readonly float[] distances;

        internal ReverseEdgePath(ReverseEdgeRequest request, ReverseEdgePlacement placement, ReverseEdgeBezier[] curves)
        {
            this.request = request;
            this.placement = placement;
            this.curves = curves;
            List<Vector2> samples = new() { curves[0].p0 };
            foreach (ReverseEdgeBezier curve in curves) Sample(curve.p0, curve.p1, curve.p2, curve.p3, samples, 0);
            points = samples.ToArray();
            distances = new float[points.Length];
            Vector2 min = points[0];
            Vector2 max = points[0];
            for (int i = 1; i < points.Length; i++)
            {
                distances[i] = distances[i - 1] + Vector2.Distance(points[i - 1], points[i]);
                min = Vector2.Min(min, points[i]);
                max = Vector2.Max(max, points[i]);
            }

            length = distances[distances.Length - 1];
            bounds = Rect.MinMaxRect(min.x - 0.5f, min.y - 0.5f, max.x + 0.5f, max.y + 0.5f);
        }

        internal Vector2 GetPoint(float rate)
        {
            if (length < 0.0001f) return points[0];
            float distance = Mathf.Clamp01(rate) * length;
            int index = Array.BinarySearch(distances, distance);
            if (index >= 0) return points[index];
            index = ~index;
            if (index == 0) return points[0];
            if (index >= points.Length) return points[points.Length - 1];
            float segmentLength = distances[index] - distances[index - 1];
            return Vector2.Lerp(points[index - 1], points[index], (distance - distances[index - 1]) / segmentLength);
        }

        private static void Sample(Vector2 a, Vector2 b, Vector2 c, Vector2 d, List<Vector2> points, int depth)
        {
            // Segment distance also detects collinear control points that overshoot the endpoints.
            if (depth >= 10 || (ReverseEdgePathBuilder.DistanceToSegmentSquared(b, a, d) <= SampleToleranceSquared &&
                                ReverseEdgePathBuilder.DistanceToSegmentSquared(c, a, d) <= SampleToleranceSquared))
            {
                if ((points[points.Count - 1] - d).sqrMagnitude > 0.000001f) points.Add(d);
                return;
            }

            Vector2 ab = (a + b) * 0.5f;
            Vector2 bc = (b + c) * 0.5f;
            Vector2 cd = (c + d) * 0.5f;
            Vector2 abc = (ab + bc) * 0.5f;
            Vector2 bcd = (bc + cd) * 0.5f;
            Vector2 middle = (abc + bcd) * 0.5f;
            Sample(a, ab, abc, middle, points, depth + 1);
            Sample(middle, bcd, cd, d, points, depth + 1);
        }
    }

    internal static class ReverseEdgePathBuilder
    {
        internal const float NodeClearance = 12f;
        internal const float LayerSpacing = 8f;
        internal const float CornerRadius = 8f;
        private const float GapClearance = 4f;
        private const float CircleHandleRatio = 0.55228475f;
        private const float SharedPortLength = 16f;

        private struct RouteScore
        {
            internal float nodeOverlap;
            internal float parallelOverlap;
            internal float expansion;
            internal int crossings;
            internal float length;

            internal bool clear => nodeOverlap < 0.01f && parallelOverlap < 4f;

            internal bool BetterThan(RouteScore other)
            {
                if (Mathf.Abs(nodeOverlap - other.nodeOverlap) > 0.01f) return nodeOverlap < other.nodeOverlap;
                if (Mathf.Abs(parallelOverlap - other.parallelOverlap) > 4f) return parallelOverlap < other.parallelOverlap;
                if (Mathf.Abs(expansion - other.expansion) > 0.01f) return expansion < other.expansion;
                if (Mathf.Abs(length - other.length) > 0.01f) return length < other.length;
                return crossings < other.crossings;
            }
        }

        internal static bool IsReverse(Vector2 from, Vector2 to, Vector2 outputDirection, Vector2 inputDirection)
        {
            Vector2 delta = to - from;
            return Vector2.Dot(delta, outputDirection) < -12f || Vector2.Dot(-delta, inputDirection) < -12f;
        }

        internal static Rect Region(ReverseEdgeRequest request)
        {
            Rect bounds = Union(request.outputBounds, request.inputBounds);
            bounds = Union(bounds, Rect.MinMaxRect(Mathf.Min(request.from.x, request.to.x), Mathf.Min(request.from.y, request.to.y),
                Mathf.Max(request.from.x, request.to.x), Mathf.Max(request.from.y, request.to.y)));
            return Expand(bounds, NodeClearance * 2);
        }

        internal static ReverseEdgePath Choose(ReverseEdgeRequest request, IReadOnlyList<ReverseEdgePath> occupied,
            ReverseEdgePath previous, int layerCount)
        {
            ReverseEdgePath best = null;
            RouteScore bestScore = default;
            bool vertical = Mathf.Abs(request.outputDirection.y) > Mathf.Abs(request.outputDirection.x);
            int gapCapacity = GapCapacity(Axes(request.outputBounds, vertical), Axes(request.inputBounds, vertical));
            if (previous != null)
            {
                best = Build(request, previous.placement);
                if (best != null)
                {
                    bestScore = Score(best, occupied);
                    if (bestScore.clear && (previous.placement.side == ReverseEdgeSide.Gap || gapCapacity == 0))
                        return previous.request.Matches(request) ? previous : best;
                }
            }

            int candidateCount = Mathf.Max(2, layerCount);
            for (int layer = 0; layer < candidateCount; layer++)
            {
                for (int side = 0; side < 3; side++)
                {
                    ReverseEdgePath candidate = Build(request, new ReverseEdgePlacement((ReverseEdgeSide) side, layer));
                    if (candidate == null) continue;
                    RouteScore score = Score(candidate, occupied);
                    if (best != null && !score.BetterThan(bestScore)) continue;
                    best = candidate;
                    bestScore = score;
                }

                // A crossing alone does not justify expanding an otherwise clear local route.
                if (best != null && bestScore.clear && (best.placement.side == ReverseEdgeSide.Gap || layer + 1 >= gapCapacity)) break;
            }

            if (previous != null && previous.request.Matches(request) && best != null &&
                best.placement.side == previous.placement.side && best.placement.layer == previous.placement.layer) return previous;
            return best;
        }

        internal static ReverseEdgePath Build(ReverseEdgeRequest request, ReverseEdgePlacement placement)
        {
            bool vertical = Mathf.Abs(request.outputDirection.y) > Mathf.Abs(request.outputDirection.x);
            Vector2 from = Axes(request.from, vertical);
            Vector2 to = Axes(request.to, vertical);
            Vector2 outputDirection = Axes(request.outputDirection, vertical);
            Vector2 inputDirection = Axes(request.inputDirection, vertical);
            Rect outputBounds = Axes(request.outputBounds, vertical);
            Rect inputBounds = Axes(request.inputBounds, vertical);
            Rect bounds = Union(outputBounds, inputBounds);
            float offset = NodeClearance + placement.layer * LayerSpacing;
            float corridor;
            if (placement.side == ReverseEdgeSide.Gap)
            {
                float low = Mathf.Min(outputBounds.yMax, inputBounds.yMax) + GapClearance;
                float high = Mathf.Max(outputBounds.yMin, inputBounds.yMin) - GapClearance;
                int capacity = GapCapacity(outputBounds, inputBounds);
                if (placement.layer >= capacity) return null;
                float gapOffset;
                if (capacity % 2 == 0) gapOffset = (placement.layer / 2 + 0.5f) * LayerSpacing;
                else gapOffset = ((placement.layer + 1) / 2) * LayerSpacing;
                if (placement.layer % 2 == 0) gapOffset = -gapOffset;
                corridor = (low + high) * 0.5f + gapOffset;
            }
            else if (placement.side == ReverseEdgeSide.Before) corridor = bounds.yMin - offset;
            else corridor = bounds.yMax + offset;

            float outputRail = outputDirection.x >= 0 ? Mathf.Max(outputBounds.xMax, from.x) + offset : Mathf.Min(outputBounds.xMin, from.x) - offset;
            List<Vector2> corners = new();
            AddWaypoint(corners, from);
            AddWaypoint(corners, new Vector2(outputRail, from.y));
            AddWaypoint(corners, new Vector2(outputRail, corridor));
            if (Mathf.Abs(inputDirection.x) > 0.5f)
            {
                float inputRail = inputDirection.x >= 0 ? Mathf.Max(inputBounds.xMax, to.x) + offset : Mathf.Min(inputBounds.xMin, to.x) - offset;
                AddWaypoint(corners, new Vector2(inputRail, corridor));
                AddWaypoint(corners, new Vector2(inputRail, to.y));
            }
            else
            {
                float inputRail = outputDirection.x >= 0 ? inputBounds.xMin - offset : inputBounds.xMax + offset;
                float inputExit = inputDirection.y >= 0 ? Mathf.Max(inputBounds.yMax, to.y) + offset : Mathf.Min(inputBounds.yMin, to.y) - offset;
                AddWaypoint(corners, new Vector2(inputRail, corridor));
                AddWaypoint(corners, new Vector2(inputRail, inputExit));
                AddWaypoint(corners, new Vector2(to.x, inputExit));
            }
            AddWaypoint(corners, to);
            return new ReverseEdgePath(request, placement, RoundCorners(corners, vertical));
        }

        private static int GapCapacity(Rect outputBounds, Rect inputBounds)
        {
            float low = Mathf.Min(outputBounds.yMax, inputBounds.yMax) + GapClearance;
            float high = Mathf.Max(outputBounds.yMin, inputBounds.yMin) - GapClearance;
            if (high < low) return 0;
            return Mathf.FloorToInt((high - low) / LayerSpacing) + 1;
        }

        private static void AddWaypoint(List<Vector2> points, Vector2 point)
        {
            while (points.Count > 0)
            {
                Vector2 previous = points[points.Count - 1];
                if ((point - previous).sqrMagnitude < 0.000001f) return;
                if (points.Count < 2) break;
                Vector2 incoming = previous - points[points.Count - 2];
                if (Mathf.Abs(Cross(incoming, point - previous)) > 0.0001f) break;
                points.RemoveAt(points.Count - 1);
            }
            points.Add(point);
        }

        private static ReverseEdgeBezier[] RoundCorners(List<Vector2> points, bool vertical)
        {
            List<ReverseEdgeBezier> curves = new();
            Vector2 cursor = points[0];
            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector2 incoming = points[i] - points[i - 1];
                Vector2 outgoing = points[i + 1] - points[i];
                float radius = Mathf.Min(CornerRadius, incoming.magnitude * 0.5f, outgoing.magnitude * 0.5f);
                incoming.Normalize();
                outgoing.Normalize();
                Vector2 start = points[i] - incoming * radius;
                Vector2 end = points[i] + outgoing * radius;
                AddStraight(curves, cursor, start, vertical);
                float handle = radius * CircleHandleRatio;
                curves.Add(Curve(start, start + incoming * handle, end - outgoing * handle, end, vertical));
                cursor = end;
            }
            AddStraight(curves, cursor, points[points.Count - 1], vertical);
            if (curves.Count == 0) curves.Add(Curve(cursor, cursor, cursor, cursor, vertical));
            return curves.ToArray();
        }

        private static void AddStraight(List<ReverseEdgeBezier> curves, Vector2 from, Vector2 to, bool vertical)
        {
            Vector2 span = to - from;
            if (span.sqrMagnitude < 0.000001f) return;
            curves.Add(Curve(from, from + span / 3f, to - span / 3f, to, vertical));
        }

        private static ReverseEdgeBezier Curve(Vector2 a, Vector2 b, Vector2 c, Vector2 d, bool vertical)
            => new(Axes(a, vertical), Axes(b, vertical), Axes(c, vertical), Axes(d, vertical));

        private static Vector2 Axes(Vector2 point, bool vertical) => vertical ? new Vector2(point.y, point.x) : point;
        private static Rect Axes(Rect rect, bool vertical) => vertical ? new Rect(rect.y, rect.x, rect.height, rect.width) : rect;

        internal static Rect Union(Rect a, Rect b)
            => Rect.MinMaxRect(Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin), Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));

        internal static Rect Expand(Rect rect, float amount)
            => Rect.MinMaxRect(rect.xMin - amount, rect.yMin - amount, rect.xMax + amount, rect.yMax + amount);

        private static RouteScore Score(ReverseEdgePath path, IReadOnlyList<ReverseEdgePath> occupied)
        {
            RouteScore score = new() { length = path.length };
            Rect nodeBounds = Union(path.request.outputBounds, path.request.inputBounds);
            score.expansion = Mathf.Max(0, nodeBounds.xMin - path.bounds.xMin) + Mathf.Max(0, path.bounds.xMax - nodeBounds.xMax) +
                              Mathf.Max(0, nodeBounds.yMin - path.bounds.yMin) + Mathf.Max(0, path.bounds.yMax - nodeBounds.yMax);
            for (int i = 1; i < path.points.Length; i++)
            {
                Vector2 a = path.points[i - 1];
                Vector2 b = path.points[i];
                score.nodeOverlap += NodeOverlapLength(a, b, path.request.outputBounds, path.request.from, path.request.outputDirection);
                score.nodeOverlap += NodeOverlapLength(a, b, path.request.inputBounds, path.request.to, path.request.inputDirection);
            }

            foreach (ReverseEdgePath other in occupied)
            {
                if (!Expand(path.bounds, LayerSpacing).Overlaps(other.bounds)) continue;
                ScorePair(path, other, ref score);
            }
            return score;
        }

        private static float NodeOverlapLength(Vector2 a, Vector2 b, Rect bounds, Vector2 port, Vector2 direction)
        {
            float overlap = SegmentOverlapLength(a, b, bounds);
            if (overlap <= 0) return 0;
            Rect portExit;
            if (Mathf.Abs(direction.x) > 0.5f)
            {
                float exit = direction.x > 0 ? bounds.xMax : bounds.xMin;
                portExit = Rect.MinMaxRect(Mathf.Min(port.x, exit), port.y - 4, Mathf.Max(port.x, exit), port.y + 4);
            }
            else
            {
                float exit = direction.y > 0 ? bounds.yMax : bounds.yMin;
                portExit = Rect.MinMaxRect(port.x - 4, Mathf.Min(port.y, exit), port.x + 4, Mathf.Max(port.y, exit));
            }
            Rect allowed = Rect.MinMaxRect(Mathf.Max(bounds.xMin, portExit.xMin), Mathf.Max(bounds.yMin, portExit.yMin),
                Mathf.Min(bounds.xMax, portExit.xMax), Mathf.Min(bounds.yMax, portExit.yMax));
            return Mathf.Max(0, overlap - SegmentOverlapLength(a, b, allowed));
        }

        private static float SegmentOverlapLength(Vector2 a, Vector2 b, Rect bounds)
        {
            if (bounds.width <= 0 || bounds.height <= 0) return 0;
            float start = 0;
            float end = 1;
            Vector2 span = b - a;
            if (!ClipAxis(a.x, span.x, bounds.xMin, bounds.xMax, ref start, ref end) ||
                !ClipAxis(a.y, span.y, bounds.yMin, bounds.yMax, ref start, ref end)) return 0;
            return span.magnitude * (end - start);
        }

        private static bool ClipAxis(float origin, float span, float min, float max, ref float start, ref float end)
        {
            if (Mathf.Abs(span) < 0.000001f) return origin >= min && origin <= max;
            float first = (min - origin) / span;
            float second = (max - origin) / span;
            start = Mathf.Max(start, Mathf.Min(first, second));
            end = Mathf.Min(end, Mathf.Max(first, second));
            return end > start;
        }

        private static void ScorePair(ReverseEdgePath path, ReverseEdgePath other, ref RouteScore score)
        {
            for (int i = 1; i < path.points.Length; i++)
            {
                Vector2 a = path.points[i - 1];
                Vector2 b = path.points[i];
                Vector2 ab = b - a;
                float length = ab.magnitude;
                if (length < 0.001f) continue;
                Vector2 direction = ab / length;
                for (int j = 1; j < other.points.Length; j++)
                {
                    Vector2 c = other.points[j - 1];
                    Vector2 d = other.points[j];
                    if (Mathf.Max(a.x, b.x) + LayerSpacing < Mathf.Min(c.x, d.x) || Mathf.Max(c.x, d.x) + LayerSpacing < Mathf.Min(a.x, b.x) ||
                        Mathf.Max(a.y, b.y) + LayerSpacing < Mathf.Min(c.y, d.y) || Mathf.Max(c.y, d.y) + LayerSpacing < Mathf.Min(a.y, b.y)) continue;
                    if (NearSharedPort((a + b) * 0.5f, (c + d) * 0.5f, path.request, other.request)) continue;
                    Vector2 cd = d - c;
                    float otherLength = cd.magnitude;
                    if (otherLength < 0.001f) continue;
                    float denominator = Cross(ab, cd);
                    if (Mathf.Abs(denominator) > 0.0001f)
                    {
                        float t = Cross(c - a, cd) / denominator;
                        float u = Cross(c - a, ab) / denominator;
                        if (t > 0 && t < 1 && u > 0 && u < 1) score.crossings++;
                    }

                    if (Mathf.Abs(Cross(direction, cd / otherLength)) > 0.25f) continue;
                    float distance = Mathf.Sqrt(Mathf.Min(Mathf.Min(DistanceToSegmentSquared(a, c, d), DistanceToSegmentSquared(b, c, d)),
                        Mathf.Min(DistanceToSegmentSquared(c, a, b), DistanceToSegmentSquared(d, a, b))));
                    if (distance >= LayerSpacing) continue;
                    float cProjection = Vector2.Dot(c - a, direction);
                    float dProjection = Vector2.Dot(d - a, direction);
                    float overlap = Mathf.Min(length, Mathf.Max(cProjection, dProjection)) - Mathf.Max(0, Mathf.Min(cProjection, dProjection));
                    if (overlap > 0) score.parallelOverlap += overlap * (1 - distance / LayerSpacing);
                }
            }
        }

        private static bool NearSharedPort(Vector2 a, Vector2 b, ReverseEdgeRequest first, ReverseEdgeRequest second)
        {
            return SharedPort(a, b, first.from, second.from) || SharedPort(a, b, first.from, second.to) ||
                   SharedPort(a, b, first.to, second.from) || SharedPort(a, b, first.to, second.to);
        }

        private static bool SharedPort(Vector2 a, Vector2 b, Vector2 first, Vector2 second)
        {
            return (first - second).sqrMagnitude < 0.01f && (a - first).sqrMagnitude < SharedPortLength * SharedPortLength &&
                   (b - first).sqrMagnitude < SharedPortLength * SharedPortLength;
        }

        internal static float DistanceToSegmentSquared(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 span = b - a;
            if (span.sqrMagnitude < 0.000001f) return (point - a).sqrMagnitude;
            float t = Mathf.Clamp01(Vector2.Dot(point - a, span) / span.sqrMagnitude);
            return (point - (a + span * t)).sqrMagnitude;
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    }
}
