using System;
using System.Collections.Generic;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    [Serializable]
    public class CreateNodeViewState
    {
        public const string CreateNodeViewStateSaveKey = "CreateNodeViewStateSaveKey";

        public List<int> expandedIDs = new List<int>();

        public void SetExpandedIDs(IEnumerable<int> newExpandedIDs)
        {
            this.expandedIDs.Clear();
            this.expandedIDs.AddRange(newExpandedIDs);
        }

        public void Save(EditorGraphView graphView)
        {
            graphView.graphLocalSettingSystem.SetTypeSettingValue(CreateNodeViewStateSaveKey, this);
        }

        public static CreateNodeViewState Get(EditorGraphView graphView)
        {
            CreateNodeViewState createNodeViewState = graphView.graphLocalSettingSystem.GetTypeSettingValue(CreateNodeViewStateSaveKey, new CreateNodeViewState());
            return createNodeViewState;
        }
    }
}