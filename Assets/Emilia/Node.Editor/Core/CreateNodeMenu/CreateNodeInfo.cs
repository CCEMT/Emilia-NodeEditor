namespace Emilia.Node.Editor
{
    public struct CreateNodeInfo
    {
        /// <summary>
        /// 节点信息
        /// </summary>
        public MenuNodeInfo menuInfo;

        /// <summary>
        /// 创建节点后处理
        /// </summary>
        public ICreateNodePostprocess postprocess;

        public CreateNodeInfo(MenuNodeInfo menuInfo, ICreateNodePostprocess createNodePostprocess = null)
        {
            this.menuInfo = menuInfo;
            postprocess = createNodePostprocess;
        }
    }
}