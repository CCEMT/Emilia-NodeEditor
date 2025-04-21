using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Node.Attributes;
using Emilia.Node.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Node.Universal.Editor
{
    [EditorItem(typeof(StickyNoteAsset))]
    public class StickyNoteView : StickyNote, IEditorItemView
    {
        private EditorGraphView graphView;
        private StickyNoteAsset stickyAsset;
        public EditorItemAsset asset => stickyAsset;
        public GraphElement element => this;
        public bool isSelected { get; protected set; }

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

        public virtual void OnValueChanged(bool isSilent = false)
        {
            title = this.stickyAsset.stickyTitle;
            contents = this.stickyAsset.content;
            fontSize = this.stickyAsset.fontSize;
            theme = this.stickyAsset.theme;

            SetPositionNoUndo(stickyAsset.position);

            if (isSilent == false)  graphView.graphSave.SetDirty();
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

        public ICopyPastePack GetPack() => new ItemCopyPastePack(asset);

        public void Delete()
        {
            this.graphView.itemSystem.DeleteItem(this);
        }

        public void RemoveView()
        {
            graphView.RemoveItemView(this);
        }

        public virtual bool Validate() => true;

        public virtual bool IsSelected() => isSelected;

        public virtual void Select()
        {
            isSelected = true;
        }

        public virtual void Unselect()
        {
            isSelected = false;
        }

        public virtual IEnumerable<Object> GetSelectedObjects()
        {
            yield return asset;
        }

        public void Dispose()
        {
            graphView = null;
        }
    }
}