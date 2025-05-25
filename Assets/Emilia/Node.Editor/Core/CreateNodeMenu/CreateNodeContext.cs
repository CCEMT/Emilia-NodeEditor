using UnityEngine;

namespace Emilia.Node.Editor
{
    public class CreateNodeContext
    {
        public Vector2 screenMousePosition;
        public GraphCreateNodeMenu nodeMenu;
        public ICreateNodeCollect nodeCollect;
        public CreateNodeConnector createNodeConnector;
    }
}