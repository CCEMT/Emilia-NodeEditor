using System;
using Emilia.Kit;
using Emilia.Node.Editor;

namespace Example.Expand
{
    //拓展本地设置
    [EditorHandle(typeof(ExpandAsset))]
    public class ExpandLocalSettingHandle : GraphLocalSettingHandle
    {
    }
}