using System.Collections.Generic;
using Emilia.Kit;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Node.Editor
{
    public abstract class EditorEdgeView : Edge, IEditorEdgeView
    {
        protected IEditorPortView _inputPortView;
        protected IEditorPortView _outputPortView;

        public EditorEdgeAsset asset { get; private set; }
        public EditorGraphView graphView { get; private set; }

        public bool isDrag { get; set; }

        public new Vector2[] PointsAndTangents => base.PointsAndTangents;

        public IEditorPortView inputPortView
        {
            get => this._inputPortView;
            set
            {
                this._inputPortView = value;
                input = _inputPortView?.portElement;

                if (this._inputPortView != null) editorEdgeControl.inputEditorOrientation = _inputPortView.editorOrientation;
                else if (this._outputPortView != null) editorEdgeControl.inputEditorOrientation = _outputPortView.editorOrientation;
            }
        }

        public IEditorPortView outputPortView
        {
            get => this._outputPortView;
            set
            {
                this._outputPortView = value;
                output = _outputPortView?.portElement;

                if (this._outputPortView != null) editorEdgeControl.outputEditorOrientation = _outputPortView.editorOrientation;
                else if (this._inputPortView != null) editorEdgeControl.outputEditorOrientation = _inputPortView.editorOrientation;
            }
        }

        protected EditorEdgeControl _editorEdgeControl;

        public EditorEdgeControl editorEdgeControl
        {
            get
            {
                if (this._editorEdgeControl == null) return edgeControl as EditorEdgeControl;
                return _editorEdgeControl;
            }
        }

        public Edge edgeElement => this;
        public bool isSelected { get; protected set; }
        protected virtual string styleFilePath => "Node/Styles/UniversalEditorEdgeView.uss";

        public virtual void Initialize(EditorGraphView graphView, EditorEdgeAsset asset)
        {
            this.graphView = graphView;
            this.asset = asset;

            if (asset != null)
            {
                IEditorNodeView inputNode = graphView.graphElementCache.GetEditorNodeView(asset.inputNodeId);
                IEditorNodeView outputNode = graphView.graphElementCache.GetEditorNodeView(asset.outputNodeId);

                inputPortView = inputNode.GetPortView(asset.inputPortId);
                outputPortView = outputNode.GetPortView(asset.outputPortId);
                
                if (inputPortView == null || outputPortView == null) return;

                input = inputPortView.portElement;
                output = outputPortView.portElement;

                input.Connect(this);
                output.Connect(this);
            }

            StyleSheet styleSheet = ResourceUtility.LoadResource<StyleSheet>(styleFilePath);
            styleSheets.Add(styleSheet);

            schedule.Execute(() => schedule.Execute(ForceUpdateEdgeControl).ExecuteLater(1)).ExecuteLater(1);
        }

        /// <summary>
        /// 根据比例获取点
        /// </summary>
        public Vector2 GetPointByRate(float rate)
        {
            float length = 0;
            Vector2[] points = PointsAndTangents;

            int amount = points.Length;
            if (amount == 0) return Vector2.zero;

            for (int i = 0; i < amount - 1; i++)
            {
                Vector2 point = points[i];
                Vector2 nextPoint = points[i + 1];
                length += Vector2.Distance(point, nextPoint);
            }

            float targetLength = length * rate;
            float currentLength = 0;
            for (int i = 0; i < amount - 1; i++)
            {
                Vector2 point = points[i];
                Vector2 nextPoint = points[i + 1];
                float distance = Vector2.Distance(point, nextPoint);
                if (currentLength + distance >= targetLength)
                {
                    float rateLength = targetLength - currentLength;
                    return Vector2.Lerp(point, nextPoint, rateLength / distance);
                }

                currentLength += distance;
            }

            return points[amount - 1];
        }

        public virtual void OnValueChanged(bool isSilent = false)
        {
            schedule.Execute(ForceUpdateEdgeControl).ExecuteLater(1);
            if (isSilent == false)  graphView.graphSave.SetDirty();
        }

        protected override EdgeControl CreateEdgeControl()
        {
            _editorEdgeControl = new EditorEdgeControl();
            return _editorEdgeControl;
        }

        /// <summary>
        /// 强制更新EdgeControl
        /// </summary>
        public void ForceUpdateEdgeControl()
        {
            if (inputPortView != null) editorEdgeControl.to = this.WorldToLocal(inputPortView.portElement.GetGlobalCenter());
            if (outputPortView != null) editorEdgeControl.from = this.WorldToLocal(outputPortView.portElement.GetGlobalCenter());
            UpdateEdgeControl();
        }

        public virtual ICopyPastePack GetPack() => new EdgeCopyPastePack(asset);

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
            if (asset != null) yield return asset;
        }

        public virtual void Delete()
        {
            graphView.connectSystem.Disconnect(this);
        }

        public virtual void RemoveView()
        {
            graphView.RemoveEdgeView(this);
        }

        public virtual void Dispose() { }
    }
}