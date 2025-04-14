using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public static class GraphLayoutUtility
    {
        [Flags]
        public enum AlignmentType
        {
            Horizontal = 1,
            Vertical = 2,

            TopOrLeft = 4,
            Center = 8,
            BottomOrRight = 16,
        }

        public static void Start(float interval, AlignmentType alignmentType, List<IEditorNodeView> elements)
        {
            Undo.IncrementCurrentGroup();
            
            if (alignmentType.HasFlag(AlignmentType.Horizontal)) LayoutHorizontal(interval, alignmentType, elements);
            else if (alignmentType.HasFlag(AlignmentType.Vertical)) LayoutVertical(interval, alignmentType, elements);
            
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Undo.IncrementCurrentGroup();
        }

        private static void LayoutHorizontal(float interval, AlignmentType alignmentType, List<IEditorNodeView> elements)
        {
            float y = GetY(alignmentType, elements);

            elements.Sort((a, b) => a.asset.position.x.CompareTo(b.asset.position.x));

            float startX = elements.FirstOrDefault().asset.position.x;
            float currentX = startX;

            for (int i = 0; i < elements.Count; i++)
            {
                IEditorNodeView element = elements[i];
                Rect position = element.asset.position;

                position.y = y;
                float width = element.asset.position.width;

                position.x = currentX;
                currentX += width + interval;

                element.SetPosition(position);
            }
        }

        private static void LayoutVertical(float interval, AlignmentType alignmentType, List<IEditorNodeView> elements)
        {
            float x = GetX(alignmentType, elements);
            
            elements.Sort((a, b) => a.asset.position.y.CompareTo(b.asset.position.y));
            
            float startY = elements.FirstOrDefault().asset.position.y;
            
            for (int i = 0; i < elements.Count; i++)
            {
                IEditorNodeView element = elements[i];
                Rect position = element.asset.position;

                position.x = x;
                position.y = startY + interval * i;

                element.SetPosition(position);
            }
        }

        private static float GetY(AlignmentType alignmentType, List<IEditorNodeView> elements)
        {
            if (alignmentType.HasFlag(AlignmentType.TopOrLeft))
            {
                float minY = int.MaxValue;
                foreach (IEditorNodeView element in elements)
                {
                    if (element.asset.position.y < minY) minY = element.asset.position.y;
                }

                return minY;
            }

            if (alignmentType.HasFlag(AlignmentType.Center))
            {
                float centerY = 0;
                foreach (IEditorNodeView element in elements) centerY += element.asset.position.y;
                centerY /= elements.Count;
                return centerY;
            }

            if (alignmentType.HasFlag(AlignmentType.BottomOrRight))
            {
                float maxY = int.MinValue;
                foreach (IEditorNodeView element in elements)
                {
                    if (element.asset.position.y > maxY) maxY = element.asset.position.y;
                }
                return maxY;
            }

            return 0;
        }

        private static float GetX(AlignmentType alignmentType, List<IEditorNodeView> elements)
        {
            if (alignmentType.HasFlag(AlignmentType.TopOrLeft))
            {
                float minX = int.MaxValue;
                foreach (IEditorNodeView element in elements)
                {
                    if (element.asset.position.x < minX) minX = element.asset.position.x;
                }
                return minX;
            }

            if (alignmentType.HasFlag(AlignmentType.Center))
            {
                float centerX = 0;
                foreach (IEditorNodeView element in elements) centerX += element.asset.position.x;
                centerX /= elements.Count;
                return centerX;
            }

            if (alignmentType.HasFlag(AlignmentType.BottomOrRight))
            {
                float maxX = int.MinValue;
                foreach (IEditorNodeView element in elements)
                {
                    if (element.asset.position.x > maxX) maxX = element.asset.position.x;
                }
                return maxX;
            }

            return 0;
        }
    }
}