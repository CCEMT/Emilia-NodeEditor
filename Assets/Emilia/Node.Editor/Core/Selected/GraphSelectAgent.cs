using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    public struct GraphSelectAgent : ISelectable, ISelectedHandle
    {
        bool ISelectable.IsSelectable() => true;

        bool ISelectable.HitTest(Vector2 localPoint) => true;

        bool ISelectable.Overlaps(Rect rectangle) => true;

        void ISelectable.Select(VisualElement selectionContainer, bool additive) { }

        void ISelectable.Unselect(VisualElement selectionContainer) { }

        bool ISelectable.IsSelected(VisualElement selectionContainer) => true;


        bool ISelectedHandle.Validate() => this.agent?.IsSelected() ?? false;

        bool ISelectedHandle.IsSelected() => this.agent?.IsSelected() ?? false;

        void ISelectedHandle.Select()
        {
            this.agent?.Select();
        }

        void ISelectedHandle.Unselect()
        {
            this.agent?.Unselect();
        }

        IEnumerable<Object> ISelectedHandle.GetSelectedObjects() => agent?.GetSelectedObjects() ?? Enumerable.Empty<Object>();

        private ISelectedHandle agent;

        public GraphSelectAgent(ISelectedHandle agent)
        {
            this.agent = agent;
        }
    }
}