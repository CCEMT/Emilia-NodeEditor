using System;
using UnityEngine;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GraphSettingAttribute : Attribute
    {
        public GraphSettingStruct settingStruct = new GraphSettingStruct {
            maxLoadTimeMs = 0.0416f,
            immediatelySave = true,
            zoomSize = new Vector2(0.15f, 3f)
        };

        /// <summary>
        /// 最大加载时间
        /// </summary>
        public float maxLoadTimeMs
        {
            get => settingStruct.maxLoadTimeMs;
            set => settingStruct.maxLoadTimeMs = value;
        }

        /// <summary>
        /// 实时保存
        /// </summary>
        public bool immediatelySave
        {
            get => settingStruct.immediatelySave;
            set => settingStruct.immediatelySave = value;
        }

        /// <summary>
        /// Zoom的最小和最大值
        /// </summary>
        public Vector2 zoomSize
        {
            get => settingStruct.zoomSize;
            set => settingStruct.zoomSize = value;
        }
    }

    [Serializable]
    public struct GraphSettingStruct
    {
        public float maxLoadTimeMs;
        public bool immediatelySave;
        public Vector2 zoomSize;
    }
}