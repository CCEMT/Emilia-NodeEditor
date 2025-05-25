namespace Emilia.Node.Editor
{
    public class CreateNodeInfo
    {
        /// <summary>
        /// 节点信息
        /// </summary>
        public MenuNodeInfo menuInfo;

        /// <summary>
        /// 创建节点时的连接器（用于在创建节点后连接目标节点）
        /// </summary>
        public CreateNodeConnector createNodeConnector;

        public CreateNodeInfo(MenuNodeInfo menuInfo)
        {
            this.menuInfo = menuInfo;
        }
    }
}