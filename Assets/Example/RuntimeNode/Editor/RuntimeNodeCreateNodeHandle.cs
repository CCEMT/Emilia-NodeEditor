using Emilia.Kit;
using Emilia.Kit.Editor;
using Emilia.Node.Editor;
using Example.RuntimeNode.Runtime;
using Sirenix.Utilities;

namespace Emilia.RuntimeNode.Editor
{
    //创建编辑节点与运行时节点的关联
    [EditorHandle(typeof(RuntimeNodeAsset))]
    public class RuntimeNodeCreateNodeHandle : CreateNodeHandle
    {
        public override void Initialize(object arg)
        {
            base.Initialize(arg);

            CreateNodeHandleContext context = (CreateNodeHandleContext) arg;

            RuntimeNodeMenuAttribute menuAttribute = context.nodeType.GetCustomAttribute<RuntimeNodeMenuAttribute>();
            if (menuAttribute != null)
            {
                path = menuAttribute.path;
                priority = menuAttribute.priority;
            }

            nodeData = ReflectUtility.CreateInstance(context.nodeType);
        }
    }
}