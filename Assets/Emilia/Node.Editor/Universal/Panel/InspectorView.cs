using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// 内置Inspector面板
    /// </summary>
    public class InspectorView : GraphPanel
    {
        protected UnityEditor.Editor editor;
        protected IMGUIContainer imguiContainer;
        protected List<Object> selectedObjects = new();
        protected Vector2 scrollPosition;

        public InspectorView()
        {
            name = nameof(InspectorView);
            style.overflow = Overflow.Hidden;

            imguiContainer = new IMGUIContainer(OnImGUI);
            imguiContainer.name = $"{nameof(InspectorView)}-IMGUI";
            imguiContainer.style.flexGrow = 1;
            imguiContainer.style.flexShrink = 1;
            Add(imguiContainer);
        }

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);

            UpdateTransform();

            graphView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            graphView.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
        }

        /// <summary>
        /// 设置显示的对象
        /// </summary>
        public void SetObjects(List<Object> selected)
        {
            List<Object> newSelectedObjects = selected == null ? new List<Object>() : selected.Where(selectedObject => selectedObject != null).ToList();
            if (editor != null && IsSameSelectedObjects(newSelectedObjects)) return;

            style.display = DisplayStyle.None;
            DisposeEditor();

            selectedObjects = newSelectedObjects;
            if (selectedObjects.Count == 0) return;
            if (CanCreateEditor(selectedObjects) == false) return;

            scrollPosition = Vector2.zero;
            UnityEditor.Editor.CreateCachedEditor(selectedObjects.ToArray(), null, ref editor);
            style.display = DisplayStyle.Flex;
        }

        protected bool CanCreateEditor(List<Object> targets)
        {
            Type targetType = null;

            for (int i = 0; i < targets.Count; i++)
            {
                Type otherType;
                object target = targets[i];

                if (i == 0)
                {
                    targetType = target.GetType();
                }
                else if (targetType != (otherType = target.GetType()))
                {
                    if (targetType.IsAssignableFrom(otherType)) continue;
                    if (otherType.IsAssignableFrom(targetType))
                    {
                        targetType = otherType;
                        continue;
                    }

                    return false;
                }
            }

            return true;
        }

        protected bool IsSameSelectedObjects(List<Object> targets)
        {
            if (selectedObjects == null) return false;
            if (selectedObjects.Count != targets.Count) return false;

            for (int i = 0; i < targets.Count; i++)
            {
                if (ReferenceEquals(selectedObjects[i], targets[i]) == false) return false;
            }

            return true;
        }

        protected void OnGeometryChangedEvent(GeometryChangedEvent evt)
        {
            UpdateTransform();
        }

        protected void UpdateTransform()
        {
            Rect worldRect = graphView.graphPanelSystem.graphRect;
            Vector2 localPosition = graphView.WorldToLocal(worldRect.position);
            transform.position = localPosition;

            Rect layoutRect = graphView.graphPanelSystem.graphLayoutRect;
            float width = layoutRect.width / 3f;
            width = Mathf.Max(width, 225);
            style.width = width;
        }

        protected void OnImGUI()
        {
            if (editor == null) return;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            try
            {
                editor.DrawHeader();
                editor.OnInspectorGUI();
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            graphView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);

            DisposeEditor();
        }

        protected void DisposeEditor()
        {
            if (editor == null) return;
            Object.DestroyImmediate(editor);
            editor = null;
        }
    }
}