using System;
using Emilia.Kit.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class NodeMessageButtonElement : VisualElement
    {
        private NodeMessageLevel level;

        private Clickable _clickable;

        public Clickable clickable
        {
            get => _clickable;
            set
            {
                if (_clickable != null && _clickable.target == this) this.RemoveManipulator(_clickable);

                _clickable = value;

                if (_clickable != null) this.AddManipulator(_clickable);
            }
        }

        public event Action clicked
        {
            add
            {
                if (_clickable == null) clickable = new Clickable(value);
                else _clickable.clicked += value;
            }
            remove
            {
                if (_clickable != null) _clickable.clicked -= value;
            }
        }

        public NodeMessageButtonElement(Action clickEvent)
        {
            name = "message-button";

            clickable = new Clickable(clickEvent);
            focusable = true;

            RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            if (clickable != null) ReflectUtility.Invoke(clickable, "SimulateSingleClick", new object[] {evt});
            evt.StopPropagation();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (panel?.contextType != ContextType.Editor) return;

            // KeyCodes are hardcoded in the Editor, but in runtime we should use the more versatile NavigationSubmit.
            if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
            {
                if (clickable != null) ReflectUtility.Invoke(clickable, "SimulateSingleClick", new object[] {evt});
                evt.StopPropagation();
            }
        }

        public void SetLevel(NodeMessageLevel level)
        {
            this.level = level;

            Texture icon = NodeMessageLevelUtility.GetIcon(level);
            style.backgroundImage = (Texture2D) icon;
        }
    }
}