using System;
using System.Reflection;
using Emilia.Node.Attributes;

namespace Emilia.Node.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GraphDataAttribute : Attribute
    {
        public Type graphAssetType;

        public GraphDataAttribute(Type graphAssetType)
        {
            this.graphAssetType = graphAssetType;
        }
    }

    public class GraphData
    {
        public virtual void OnCreate(EditorGraphView graphView) { }
    }

    [GraphData(typeof(EditorGraphAsset))]
    public class BasicGraphData : GraphData
    {
        public GraphSettingStruct graphSetting = GraphSettingStruct.Default;

        public override void OnCreate(EditorGraphView graphView)
        {
            base.OnCreate(graphView);
            graphSetting = graphView.graphAsset?.GetType().GetCustomAttribute<GraphSettingAttribute>(true)?.settingStruct ?? GraphSettingStruct.Default;
        }
    }
}