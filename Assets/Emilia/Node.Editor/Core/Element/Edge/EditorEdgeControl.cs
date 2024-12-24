using System;
using System.Collections.Generic;
using Emilia.Reflection.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public class EditorEdgeControl : EdgeControl_Internals
    {
        protected struct EdgeCornerSweepValues
        {
            public Vector2 circleCenter;
            public double sweepAngle;
            public double startAngle;
            public double endAngle;
            public Vector2 crossPoint1;
            public Vector2 crossPoint2;
            public float radius;
        }

        protected const float EdgeLengthFromPort = 12.0f;
        protected const float EdgeTurnDiameter = 16.0f;
        protected const float EdgeSweepResampleRatio = 4.0f;
        protected const int EdgeStraightLineSegmentDivisor = 5;

        protected EditorOrientation _inputEditorOrientation;
        protected EditorOrientation _outputEditorOrientation;

        public virtual EditorOrientation inputEditorOrientation
        {
            get => this._inputEditorOrientation;
            set
            {
                if (this._inputEditorOrientation == value) return;
                this._inputEditorOrientation = value;
                MarkDirtyRepaint();
            }
        }

        public virtual EditorOrientation outputEditorOrientation
        {
            get => this._outputEditorOrientation;
            set
            {
                if (this._outputEditorOrientation == value) return;
                this._outputEditorOrientation = value;
                MarkDirtyRepaint();
            }
        }

        protected List<Vector2> lastLocalControlPoints = new List<Vector2>();

        protected override void UpdateRenderPoints()
        {
            ComputeControlPoints();

            if (renderPointsDirty_Internals == false && controlPoints != null) return;

            Vector2 p1 = parent.ChangeCoordinatesTo(this, controlPoints[0]);
            Vector2 p2 = parent.ChangeCoordinatesTo(this, controlPoints[1]);
            Vector2 p3 = parent.ChangeCoordinatesTo(this, controlPoints[2]);
            Vector2 p4 = parent.ChangeCoordinatesTo(this, controlPoints[3]);

            if (lastLocalControlPoints.Count == 4)
            {
                if (Approximately(p1, lastLocalControlPoints[0]) &&
                    Approximately(p2, lastLocalControlPoints[1]) &&
                    Approximately(p3, lastLocalControlPoints[2]) &&
                    Approximately(p4, lastLocalControlPoints[3]))
                {
                    renderPointsDirty_Internals = false;
                    return;
                }
            }

            lastLocalControlPoints.Clear();
            lastLocalControlPoints.Add(p1);
            lastLocalControlPoints.Add(p2);
            lastLocalControlPoints.Add(p3);
            lastLocalControlPoints.Add(p4);

            renderPoints_Internals.Clear();

            if (inputEditorOrientation == EditorOrientation.Custom || outputEditorOrientation == EditorOrientation.Custom)
            {
                renderPoints_Internals.Add(p1);
                renderPoints_Internals.Add(p4);
                return;
            }

            float diameter = EdgeTurnDiameter;

            bool sameOrientations = inputEditorOrientation == outputEditorOrientation;
            if (sameOrientations &&
                ((outputEditorOrientation == EditorOrientation.Horizontal && Mathf.Abs(p1.y - p4.y) < 2 && p1.x + EdgeLengthFromPort < p4.x - EdgeLengthFromPort) ||
                 (outputEditorOrientation == EditorOrientation.Vertical && Mathf.Abs(p1.x - p4.x) < 2 && p1.y + EdgeLengthFromPort < p4.y - EdgeLengthFromPort)))
            {
                RenderStraightLines(p1, p2, p3, p4);
                return;
            }

            bool renderBothCorners = true;

            EdgeCornerSweepValues corner1 = GetCornerSweepValues(p1, p2, p3, diameter, Direction.Output);
            EdgeCornerSweepValues corner2 = GetCornerSweepValues(p2, p3, p4, diameter, Direction.Input);

            if (! ValidateCornerSweepValues(ref corner1, ref corner2))
            {
                if (sameOrientations)
                {
                    RenderStraightLines(p1, p2, p3, p4);
                    return;
                }

                renderBothCorners = false;

                Vector2 px = outputOrientation == Orientation.Horizontal ? new Vector2(p4.x, p1.y) : new Vector2(p1.x, p4.y);

                corner1 = GetCornerSweepValues(p1, px, p4, diameter, Direction.Output);
            }

            renderPoints_Internals.Add(p1);

            if (! sameOrientations && renderBothCorners)
            {
                float minDistance = 2 * diameter * diameter;
                if ((p3 - p2).sqrMagnitude < minDistance ||
                    (p4 - p1).sqrMagnitude < minDistance)
                {
                    Vector2 px = (p2 + p3) * 0.5f;
                    corner1 = GetCornerSweepValues(p1, px, p4, diameter, Direction.Output);
                    renderBothCorners = false;
                }
            }

            GetRoundedCornerPoints(renderPoints_Internals, corner1, Direction.Output);
            if (renderBothCorners) GetRoundedCornerPoints(renderPoints_Internals, corner2, Direction.Input);

            renderPoints_Internals.Add(p4);
        }

        protected void RenderStraightLines(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float safeSpan = outputOrientation == Orientation.Horizontal
                ? Mathf.Abs(p1.x + EdgeLengthFromPort - (p4.x - EdgeLengthFromPort))
                : Mathf.Abs(p1.y + EdgeLengthFromPort - (p4.y - EdgeLengthFromPort));

            float safeSpan3 = safeSpan / EdgeStraightLineSegmentDivisor;
            float nodeToP2Dist = Mathf.Min(safeSpan3, EdgeTurnDiameter);
            nodeToP2Dist = Mathf.Max(0, nodeToP2Dist);

            Vector2 offset = outputOrientation == Orientation.Horizontal
                ? new Vector2(EdgeTurnDiameter - nodeToP2Dist, 0)
                : new Vector2(0, EdgeTurnDiameter - nodeToP2Dist);

            renderPoints_Internals.Add(p1);
            renderPoints_Internals.Add(p2 - offset);
            renderPoints_Internals.Add(p3 + offset);
            renderPoints_Internals.Add(p4);
        }

        protected bool ValidateCornerSweepValues(ref EdgeCornerSweepValues corner1, ref EdgeCornerSweepValues corner2)
        {
            Vector2 circlesMidpoint = (corner1.circleCenter + corner2.circleCenter) / 2;

            Vector2 p2CenterToCross1 = corner1.circleCenter - corner1.crossPoint1;
            Vector2 p2CenterToCirclesMid = corner1.circleCenter - circlesMidpoint;
            double angleToCirclesMid = outputOrientation == Orientation.Horizontal
                ? Math.Atan2(p2CenterToCross1.y, p2CenterToCross1.x) - Math.Atan2(p2CenterToCirclesMid.y, p2CenterToCirclesMid.x)
                : Math.Atan2(p2CenterToCross1.x, p2CenterToCross1.y) - Math.Atan2(p2CenterToCirclesMid.x, p2CenterToCirclesMid.y);

            if (double.IsNaN(angleToCirclesMid)) return false;

            angleToCirclesMid = Math.Sign(angleToCirclesMid) * 2 * Mathf.PI - angleToCirclesMid;
            if (Mathf.Abs((float) angleToCirclesMid) > 1.5 * Mathf.PI) angleToCirclesMid = -1 * Math.Sign(angleToCirclesMid) * 2 * Mathf.PI + angleToCirclesMid;

            float h = p2CenterToCirclesMid.magnitude;
            float p2AngleToMidTangent = Mathf.Acos(corner1.radius / h);

            if (double.IsNaN(p2AngleToMidTangent)) return false;

            float maxSweepAngle = Mathf.Abs((float) corner1.sweepAngle) - p2AngleToMidTangent * 2;

            if (Mathf.Abs((float) angleToCirclesMid) < Mathf.Abs((float) corner1.sweepAngle))
            {
                corner1.sweepAngle = Math.Sign(corner1.sweepAngle) * Mathf.Min(maxSweepAngle, Mathf.Abs((float) corner1.sweepAngle));
                corner2.sweepAngle = Math.Sign(corner2.sweepAngle) * Mathf.Min(maxSweepAngle, Mathf.Abs((float) corner2.sweepAngle));
            }

            return true;
        }

        protected EdgeCornerSweepValues GetCornerSweepValues(
            Vector2 p1, Vector2 cornerPoint, Vector2 p2, float diameter, Direction closestPortDirection)
        {
            EdgeCornerSweepValues corner = new EdgeCornerSweepValues();

            corner.radius = diameter / 2;

            Vector2 d1Corner = (cornerPoint - p1).normalized;
            Vector2 d1 = d1Corner * diameter;
            float dx1 = d1.x;
            float dy1 = d1.y;

            Vector2 d2Corner = (cornerPoint - p2).normalized;
            Vector2 d2 = d2Corner * diameter;
            float dx2 = d2.x;
            float dy2 = d2.y;

            float angle = (float) (Math.Atan2(dy1, dx1) - Math.Atan2(dy2, dx2)) / 2;

            float tan = (float) Math.Abs(Math.Tan(angle));
            float segment = corner.radius / tan;

            if (segment > diameter)
            {
                segment = diameter;
                corner.radius = diameter * tan;
            }

            corner.crossPoint1 = cornerPoint - d1Corner * segment;
            corner.crossPoint2 = cornerPoint - d2Corner * segment;

            corner.circleCenter = GetCornerCircleCenter(cornerPoint, corner.crossPoint1, corner.crossPoint2, segment, corner.radius);

            corner.startAngle = Math.Atan2(corner.crossPoint1.y - corner.circleCenter.y, corner.crossPoint1.x - corner.circleCenter.x);
            corner.endAngle = Math.Atan2(corner.crossPoint2.y - corner.circleCenter.y, corner.crossPoint2.x - corner.circleCenter.x);

            corner.sweepAngle = corner.endAngle - corner.startAngle;

            if (closestPortDirection == Direction.Input)
            {
                double endAngle = corner.endAngle;
                corner.endAngle = corner.startAngle;
                corner.startAngle = endAngle;
            }

            if (corner.sweepAngle > Math.PI) corner.sweepAngle = -2 * Math.PI + corner.sweepAngle;
            else if (corner.sweepAngle < -Math.PI) corner.sweepAngle = 2 * Math.PI + corner.sweepAngle;

            return corner;
        }

        protected Vector2 GetCornerCircleCenter(Vector2 cornerPoint, Vector2 crossPoint1, Vector2 crossPoint2, float segment, float radius)
        {
            float dx = cornerPoint.x * 2 - crossPoint1.x - crossPoint2.x;
            float dy = cornerPoint.y * 2 - crossPoint1.y - crossPoint2.y;

            var cornerToCenterVector = new Vector2(dx, dy);

            float L = cornerToCenterVector.magnitude;

            if (Mathf.Approximately(L, 0))
            {
                return cornerPoint;
            }

            float d = new Vector2(segment, radius).magnitude;
            float factor = d / L;

            return new Vector2(cornerPoint.x - cornerToCenterVector.x * factor, cornerPoint.y - cornerToCenterVector.y * factor);
        }

        protected void GetRoundedCornerPoints(List<Vector2> points, EdgeCornerSweepValues corner, Direction closestPortDirection)
        {
            int pointsCount = Mathf.CeilToInt((float) Math.Abs(corner.sweepAngle * EdgeSweepResampleRatio));
            int sign = Math.Sign(corner.sweepAngle);
            bool backwards = closestPortDirection == Direction.Input;

            for (int i = 0; i < pointsCount; ++i)
            {
                float sweepIndex = backwards ? i - pointsCount : i;

                double sweepedAngle = corner.startAngle + sign * sweepIndex / EdgeSweepResampleRatio;

                var pointX = (float) (corner.circleCenter.x + Math.Cos(sweepedAngle) * corner.radius);
                var pointY = (float) (corner.circleCenter.y + Math.Sin(sweepedAngle) * corner.radius);

                if (i == 0 && backwards)
                {
                    if (outputOrientation == Orientation.Horizontal)
                    {
                        if (corner.sweepAngle < 0 && points[points.Count - 1].y > pointY) continue;
                        else if (corner.sweepAngle >= 0 && points[points.Count - 1].y < pointY) continue;
                    }
                    else
                    {
                        if (corner.sweepAngle < 0 && points[points.Count - 1].x < pointX) continue;
                        else if (corner.sweepAngle >= 0 && points[points.Count - 1].x > pointX) continue;
                    }
                }

                points.Add(new Vector2(pointX, pointY));
            }
        }

        protected static bool Approximately(Vector2 v1, Vector2 v2)
        {
            return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y);
        }
    }
}