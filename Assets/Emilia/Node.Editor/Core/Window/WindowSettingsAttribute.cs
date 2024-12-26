using System;
using UnityEngine;

namespace Emilia.Node
{
    /// <summary>
    /// Window窗口设置
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class WindowSettingsAttribute : Attribute
    {
        /// <summary>
        /// 窗口类型
        /// </summary>
        public Type windowType;

        /// <summary>
        /// 开启时窗口大小
        /// </summary>
        public Vector2 startSize = new Vector2(850, 600);

        /// <summary>
        /// Window窗口标题
        /// </summary>
        public string title;

        /// <summary>
        /// 允许通过资源打开窗口
        /// </summary>
        public bool canOpenAsset = true;

        /// <summary>
        /// 获取开始大小表达式
        /// </summary>
        public string getStartSizeExpression;

        /// <summary>
        /// 标题表达式
        /// </summary>
        public string titleExpression;

        public WindowSettingsAttribute() { }

        public WindowSettingsAttribute(string title)
        {
            this.title = title;
        }

        public WindowSettingsAttribute(float width, float height)
        {
            startSize = new Vector2(width, height);
        }

        public WindowSettingsAttribute(string title, float width, float height)
        {
            this.title = title;
            startSize = new Vector2(width, height);
        }
    }
}