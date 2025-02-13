using System;
using UnityEngine;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GraphSettingAttribute : Attribute
    {
        /// <summary>
        /// 最大加载时间
        /// </summary>
        public float maxLoadTimeMs = 0.0416f;

        /// <summary>
        /// 是否启用快速撤销
        /// </summary>
        public bool fastUndo = true;

        /// <summary>
        /// 实时保存
        /// </summary>
        public bool immediatelySave = true;

        /// <summary>
        /// Zoom的最小和最大值
        /// </summary>
        public Vector2 zoomSize = new Vector2(0.15f, 3f);
    }
}