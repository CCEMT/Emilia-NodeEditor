using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Node.Editor;
using Emilia.Node.Universal.Editor;
using UnityEngine;

namespace Example.Expand
{
    [EditorHandle(typeof(ExpandAsset))]
    public class ExpandOperateMenuHandle : UniversalOperateMenuHandle
    {
        public override void InitializeCache(EditorGraphView graphView, List<OperateMenuActionInfo> actionInfos)
        {
            base.InitializeCache(graphView, actionInfos);

            OperateMenuActionInfo operateMenuActionInfo = new();
            operateMenuActionInfo.name = "Test";

            GeneralOperateMenuAction generalOperateMenuAction = new();
            generalOperateMenuAction.executeCallback = (_) => Debug.Log("Hello, World!");

            operateMenuActionInfo.action = generalOperateMenuAction;

            actionInfos.Add(operateMenuActionInfo);
        }
    }
}