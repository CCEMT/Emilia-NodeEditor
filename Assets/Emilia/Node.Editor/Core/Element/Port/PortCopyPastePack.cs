using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    [Serializable]
    public class PortCopyPastePack : IPortCopyPastePack
    {
        [SerializeField]
        private string _nodeId;

        [SerializeField]
        private string _portId;

        [OdinSerialize, NonSerialized]
        private Type _portType;

        [SerializeField]
        private EditorPortDirection _direction;

        [OdinSerialize, NonSerialized]
        private List<IEdgeCopyPastePack> _connectionPacks;

        public string nodeId => _nodeId;
        public string portId => this._portId;
        public Type portType => this._portType;
        public EditorPortDirection direction => _direction;
        public List<IEdgeCopyPastePack> connectionPacks => _connectionPacks;

        public PortCopyPastePack(string nodeId, string portId, Type portType, EditorPortDirection direction, List<IEdgeCopyPastePack> connectionPacks)
        {
            this._nodeId = nodeId;
            this._portId = portId;
            this._portType = portType;
            this._direction = direction;

            this._connectionPacks = connectionPacks;
        }

        public bool CanDependency(ICopyPastePack pack)
        {
            return false;
        }

        public void Paste(CopyPasteContext copyPasteContext)
        {
            GraphCopyPasteContext graphCopyPasteContext = (GraphCopyPasteContext) copyPasteContext.userData;
            EditorGraphView graphView = graphCopyPasteContext.graphView;

            if (graphView == null) return;
            IEditorPortView portView = graphView.graphSelected.selected.OfType<IEditorPortView>().FirstOrDefault();
            if (portView == null) return;

            int connectionAmount = _connectionPacks.Count;
            for (int i = 0; i < connectionAmount; i++)
            {
                IEdgeCopyPastePack edgeCopyPastePack = _connectionPacks[i];
                if (edgeCopyPastePack == null) continue;

                EditorEdgeAsset copyAsset = edgeCopyPastePack.copyAsset;

                EditorEdgeAsset pasteAsset = Object.Instantiate(copyAsset);
                pasteAsset.name = copyAsset.name;
                pasteAsset.id = Guid.NewGuid().ToString();

                pasteAsset.PasteChild();

                if (direction == EditorPortDirection.Input)
                {
                    pasteAsset.inputNodeId = portView.master.asset.id;
                    pasteAsset.inputPortId = portView.info.id;
                }
                else
                {
                    pasteAsset.outputNodeId = portView.master.asset.id;
                    pasteAsset.outputPortId = portView.info.id;
                }

                graphView.RegisterCompleteObjectUndo("Graph Paste");
                IEditorEdgeView edgeView = graphView.AddEdge(pasteAsset);

                copyPasteContext.pasteContent.Add(edgeView);

                Undo.RegisterCreatedObjectUndo(pasteAsset, "Graph Pause");
            }
        }
    }
}