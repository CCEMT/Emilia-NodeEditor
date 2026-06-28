using System.Collections.Generic;
using Emilia.Node.Editor;

namespace Emilia.Node.Universal.Editor
{
    public interface IRelayEdgeDataStrategy
    {
        bool CanMergeGroup(RelayInsertGroup group);

        void PrepareEdgeAssets(
            RelayInsertGroup group,
            IEditorNodeView relayNodeView,
            List<EditorEdgeAsset> inputEdges,
            List<EditorEdgeAsset> outputEdges);
    }

    public interface IRelayRestoreEdgeDataStrategy
    {
        bool CanRestoreGroup(RelayRestoreGroup group);

        void PrepareRestoredEdgeAssets(RelayRestoreGroup group);
    }

    public class DefaultRelayEdgeDataStrategy : IRelayEdgeDataStrategy, IRelayRestoreEdgeDataStrategy
    {
        public static readonly DefaultRelayEdgeDataStrategy Instance = new();

        public virtual bool CanMergeGroup(RelayInsertGroup group) => true;

        public virtual void PrepareEdgeAssets(
            RelayInsertGroup group,
            IEditorNodeView relayNodeView,
            List<EditorEdgeAsset> inputEdges,
            List<EditorEdgeAsset> outputEdges) { }

        public virtual bool CanRestoreGroup(RelayRestoreGroup group) => true;

        public virtual void PrepareRestoredEdgeAssets(RelayRestoreGroup group) { }
    }

    public class RelayInsertGroup
    {
        public List<IEditorEdgeView> edges = new();
        public List<IEditorPortView> outputPorts = new();
        public List<IEditorPortView> inputPorts = new();
    }

    public class RelayInsertResult
    {
        public List<IEditorNodeView> relayNodeViews = new();
        public List<IEditorEdgeView> createdEdgeViews = new();
        public List<IEditorEdgeView> skippedEdgeViews = new();
    }

    public class RelayRestoreConnection
    {
        public IEditorPortView outputPort;
        public IEditorPortView inputPort;
        public IEditorEdgeView inputSideEdge;
        public IEditorEdgeView outputSideEdge;
        public EditorEdgeAsset restoredEdgeAsset;
    }

    public class RelayRestoreGroup
    {
        public RelayEditorNodeView relayNodeView;
        public List<IEditorEdgeView> inputEdges = new();
        public List<IEditorEdgeView> outputEdges = new();
        public List<IEditorPortView> outputPorts = new();
        public List<IEditorPortView> inputPorts = new();
        public List<RelayRestoreConnection> connections = new();
    }

    public class RelayRestoreResult
    {
        public bool success;
        public RelayEditorNodeView relayNodeView;
        public List<IEditorEdgeView> createdEdgeViews = new();
        public List<IEditorEdgeView> removedEdgeViews = new();
    }

    public static partial class RelayHelper
    {
        public const string InputPortId = "relay_input";
        public const string OutputPortId = "relay_output";
    }
}