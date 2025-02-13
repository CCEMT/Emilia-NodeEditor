using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Node.Attributes;
using Emilia.Node.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    [EditorItem(typeof(StickyNoteAsset))]
    public class StickyNoteView : StickyNote, IEditorItemView
    {
        private EditorGraphView graphView;
        private StickyNoteAsset stickyAsset;
        public EditorItemAsset asset => stickyAsset;
        public GraphElement element => this;

        public void Initialize(EditorGraphView graphView, EditorItemAsset asset)
        {
            this.graphView = graphView;
            stickyAsset = asset as StickyNoteAsset;

            title = stickyAsset.stickyTitle;
            contents = stickyAsset.content;
            theme = stickyAsset.theme;
            fontSize = this.stickyAsset.fontSize;
            SetPositionNoUndo(stickyAsset.position);

            this.Q<TextField>("title-field").RegisterCallback<ChangeEvent<string>>(e => stickyAsset.stickyTitle = e.newValue);
            this.Q<TextField>("contents-field").RegisterCallback<ChangeEvent<string>>(e => stickyAsset.content = e.newValue);

            RegisterCallback<StickyNoteChangeEvent>((_) => {
                stickyAsset.fontSize = fontSize;
                stickyAsset.theme = theme;
            });
        }

        public void OnValueChanged()
        {
            title = this.stickyAsset.stickyTitle;
            contents = this.stickyAsset.content;
            theme = this.stickyAsset.theme;
            SetPositionNoUndo(stickyAsset.position);
            graphView.graphSave.SetDirty();
        }

        public override void SetPosition(Rect rect)
        {
            base.SetPosition(rect);
            if (this.graphView == null) return;
            this.graphView.RegisterCompleteObjectUndo("Graph MoveNode");
            stickyAsset.position = rect;
        }

        public void SetPositionNoUndo(Rect newPos)
        {
            base.SetPosition(newPos);
            asset.position = newPos;
        }

        public override void OnResized()
        {
            base.OnResized();
            stickyAsset.position = GetPosition();
        }

        public ICopyPastePack GetPack()
        {
            return new ItemCopyPastePack(asset);
        }

        public void Delete()
        {
            this.graphView.itemSystem.DeleteItem(this);
        }

        public void RemoveView()
        {
            graphView.RemoveItemView(this);
        }

        public IEnumerable<Object> CollectSelectedObjects()
        {
            yield return asset;
        }

        public void UpdateSelected() { }

        public void Dispose()
        {
            graphView = null;
        }
    }
}