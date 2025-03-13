using System.Collections.Generic;

namespace Emilia.Node.Editor
{
    public interface ICreateNodeCollect
    {
        List<CreateNodeInfo> Collect(List<CreateNodeInfo> allNodeInfos);
    }
}