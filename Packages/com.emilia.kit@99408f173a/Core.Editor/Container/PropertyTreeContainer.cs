using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;

namespace Emilia.Kit.Editor
{
    [Serializable]
    public class PropertyTreeContainer
    {
        private const string TargetPropertyName = "_target";

        [Serializable]
        private abstract class DrawerBase
        {
            public abstract object target { get; set; }
        }

        [Serializable]
        private class Drawer<T> : DrawerBase
        {
            [OdinSerialize, HideReferenceObjectPicker, HideLabel]
            public T _target;

            public override object target
            {
                get => _target;
                set => _target = value is T typedValue ? typedValue : default;
            }
        }

        private sealed class TreeState
        {
            public DrawerBase drawer;
            public PropertyTree propertyTree;

            public void Dispose()
            {
                propertyTree?.Dispose();
                propertyTree = null;
                drawer = null;
            }
        }

        private Dictionary<object, TreeState> _propertyTrees = new Dictionary<object, TreeState>();

        public void DrawTargetCheck(object target, Action<PropertyTree, InspectorProperty> onCheck = null)
        {
            if (target == null) return;
            TreeState state = GetTreeState(target);
            PropertyTree propertyTree = state.propertyTree;
            InspectorProperty targetProperty = propertyTree.RootProperty.Children[TargetPropertyName];
            if (targetProperty == null) return;

            propertyTree.BeginDraw(false);
            EditorGUI.BeginChangeCheck();
            targetProperty.Draw();
            if (EditorGUI.EndChangeCheck())
            {
                state.drawer.target = targetProperty.ValueEntry?.WeakSmartValue;
                onCheck?.Invoke(propertyTree, targetProperty);
            }

            propertyTree.EndDraw();
        }

        TreeState GetTreeState(object target)
        {
            if (_propertyTrees == null) _propertyTrees = new Dictionary<object, TreeState>();
            if (_propertyTrees.TryGetValue(target, out TreeState state))
            {
                if (ReferenceEquals(target, state.drawer.target) == false) state = ResetTreeState(target);
                return state;
            }

            state = CreateTreeState(target);
            _propertyTrees.Add(target, state);
            return state;
        }

        public PropertyTree ResetPropertyTree(object target)
        {
            return ResetTreeState(target)?.propertyTree;
        }

        private TreeState ResetTreeState(object target)
        {
            if (_propertyTrees == null) return null;
            if (_propertyTrees.TryGetValue(target, out TreeState state)) state.Dispose();
            TreeState newState = CreateTreeState(target);
            _propertyTrees[target] = newState;
            return newState;
        }

        private static TreeState CreateTreeState(object target)
        {
            Type drawerType = typeof(Drawer<>).MakeGenericType(target.GetType());
            DrawerBase drawer = (DrawerBase) Activator.CreateInstance(drawerType, true);
            drawer.target = target;

            return new TreeState {
                drawer = drawer,
                propertyTree = PropertyTree.Create(drawer)
            };
        }

        public void DisposeTarget(object target)
        {
            if (_propertyTrees == null) return;
            if (_propertyTrees.TryGetValue(target, out TreeState state)) state.Dispose();
            _propertyTrees.Remove(target);
        }

        public void Dispose()
        {
            if (this._propertyTrees == null) return;
            foreach (TreeState state in _propertyTrees.Values) state?.Dispose();
            _propertyTrees.Clear();
        }
    }
}
