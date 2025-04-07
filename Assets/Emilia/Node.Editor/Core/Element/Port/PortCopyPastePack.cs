using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;

namespace Emilia.Node.Editor
{
    public class PortCopyPastePack : IPortCopyPastePack
    {
        private string portId;
        private List<ICopyPastePack> _connectionPacks;

        public List<ICopyPastePack> connectionPacks => _connectionPacks;

        public PortCopyPastePack(string portId, List<ICopyPastePack> connectionPacks)
        {
            this.portId = portId;
            this._connectionPacks = connectionPacks;
        }

        public bool CanDependency(ICopyPastePack pack) => false;

        public void Paste(CopyPasteContext copyPasteContext)
        {
            GraphCopyPasteContext graphCopyPasteContext = (GraphCopyPasteContext) copyPasteContext.userData;
            EditorGraphView graphView = graphCopyPasteContext.graphView;

            if (graphView == null) return;
            IEditorPortView portView = graphView.graphSelected.selected.OfType<IEditorPortView>().FirstOrDefault();
            if (portView == null) return;

            //TODO

        }
    }
}