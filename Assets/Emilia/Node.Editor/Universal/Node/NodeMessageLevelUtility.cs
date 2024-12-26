using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public static class NodeMessageLevelUtility
    {
        public static Texture GetIcon(NodeMessageLevel level)
        {
            switch (level)
            {
                case NodeMessageLevel.Info:
                    return EditorGUIUtility.IconContent("console.infoicon").image;
                case NodeMessageLevel.Warning:
                    return EditorGUIUtility.IconContent("console.warnicon").image;
                case NodeMessageLevel.Error:
                    return EditorGUIUtility.IconContent("console.erroricon").image;
                default:
                    return null;
            }
        }

        public static Color GetColor(NodeMessageLevel level)
        {
            switch (level)
            {
                case NodeMessageLevel.Info:
                    return Color.white;
                case NodeMessageLevel.Warning:
                    return Color.yellow;
                case NodeMessageLevel.Error:
                    return Color.red;
            }

            return Color.white;
        }
    }
}