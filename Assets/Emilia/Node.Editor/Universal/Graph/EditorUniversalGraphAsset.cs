using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Node.Editor;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Universal.Editor
{
    public abstract class EditorUniversalGraphAsset : EditorGraphAsset
    {
        [LabelText("描述"), TextArea(3, 10)]
        public string description;

        [NonSerialized, OdinSerialize, HideInInspector]
        public EditorParametersManage editorParametersManage;

        public virtual string[] operateMenuTags => new[] {OperateMenuTagDefine.BaseActionTag, OperateMenuTagDefine.UniversalActionTag};

        public override void SetChildren(List<Object> childAssets)
        {
            editorParametersManage = null;
            base.SetChildren(childAssets);

            EditorParametersManage parametersManage = childAssets.OfType<EditorParametersManage>().FirstOrDefault();
            if (parametersManage == null) return;

            this.editorParametersManage = parametersManage;
            EditorAssetKit.SaveAssetIntoObject(this.editorParametersManage, this);
        }

        public override List<Object> GetChildren()
        {
            var assets = base.GetChildren();
            if (this.editorParametersManage != null) assets.Add(this.editorParametersManage);
            return assets;
        }

        public override void CollectAsset(List<Object> allAssets)
        {
            base.CollectAsset(allAssets);
            if (this.editorParametersManage != null) allAssets.Add(editorParametersManage);
        }
    }
}