using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class NodeMessageElement : VisualElement
    {
        private string _message;
        private NodeMessageLevel _level;

        public string message => this._message;
        public NodeMessageLevel level => this._level;

        public Action onRemove;

        public NodeMessageElement()
        {
            name = "node-message";
        }

        public void Init(string message, NodeMessageLevel level)
        {
            this._message = message;
            this._level = level;

            Texture icon = NodeMessageLevelUtility.GetIcon(level);
            Color color = NodeMessageLevelUtility.GetColor(level);

            Image image = new Image();
            image.name = "icon";
            image.image = icon;
            image.style.width = 16;
            image.style.height = 16;

            Add(image);

            Label messageLabel = new Label();
            messageLabel.name = "message";
            messageLabel.text = message;
            messageLabel.style.color = color;

            Add(messageLabel);

            style.color = color;
        }

        public void WaitUntilRemove(Func<bool> condition)
        {
            var item = schedule.Execute(() => {
                if (condition()) Remove();
            });

            item.Every(100).Until(condition);
        }

        public void Remove()
        {
            this.onRemove?.Invoke();
        }
    }
}