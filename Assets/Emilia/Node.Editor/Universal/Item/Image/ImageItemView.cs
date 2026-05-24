using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Node.Attributes;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using UiImage = UnityEngine.UIElements.Image;

namespace Emilia.Node.Universal.Editor
{
    [EditorItem(typeof(ImageItemAsset))]
    public class ImageItemView : GraphElement, IEditorItemView, IResizable, IUniversalPasteHandler
    {
        private ImageItemAsset imageAsset;
        private UiImage image;
        private Label placeholder;
        private VisualElement selectionBorder;
        private Texture2D previewTexture;

        public EditorItemAsset asset => imageAsset;
        public GraphElement element => this;
        public EditorGraphView graphView { get; protected set; }
        public bool isSelected { get; protected set; }

        public static float CalculateBorderWidth(bool hasImage, bool isSelected)
        {
            return hasImage && isSelected == false ? 0f : 1f;
        }

        public ImageItemView()
        {
            name = nameof(ImageItemView);
        }

        public void Initialize(EditorGraphView graphView, EditorItemAsset asset)
        {
            this.graphView = graphView;
            imageAsset = asset as ImageItemAsset;

            capabilities = Capabilities.Selectable | Capabilities.Movable | Capabilities.Deletable | Capabilities.Ascendable | Capabilities.Copiable;

            StyleSheet styleSheet = ResourceUtility.LoadResource<StyleSheet>("Node/Styles/UniversalEditorItemView.uss");
            if (styleSheet != null) styleSheets.Add(styleSheet);

            AddToClassList("image-item-view");

            image = new UiImage();
            image.name = "image-preview";
            image.scaleMode = ScaleMode.ScaleToFit;
            Add(image);

            placeholder = new Label("拖拽或粘贴图片");
            placeholder.name = "image-placeholder";
            Add(placeholder);

            selectionBorder = new VisualElement();
            selectionBorder.name = "selection-border";
            Add(selectionBorder);

            ResizableElement resizableElement = new();
            Add(resizableElement);

            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<DragUpdatedEvent>(OnDragUpdated);
            RegisterCallback<DragPerformEvent>(OnDragPerform);

            SetPositionNoUndo(imageAsset.position);
            RefreshImage();
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            graphView?.UpdateSelected();
        }

        private void OnDragUpdated(DragUpdatedEvent evt)
        {
            if (ImageItemUtility.CanCreatePayloadFromDrag() == false) return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.StopPropagation();
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            if (ImageItemUtility.TryCreatePayloadFromDrag(out ImageItemPayload payload) == false) return;

            DragAndDrop.AcceptDrag();
            ApplyImage(payload, true);
            evt.StopPropagation();
        }

        public bool CanPaste()
        {
            return WindowsClipboardImageUtility.HasImage();
        }

        public bool TryPaste()
        {
            if (WindowsClipboardImageUtility.TryGetImage(out ImageItemPayload payload) == false) return false;

            ApplyImage(payload, true);
            return true;
        }

        public void ApplyImage(ImageItemPayload payload, bool recordUndo)
        {
            if (payload.isValid == false || imageAsset == null) return;

            if (recordUndo) graphView.RegisterCompleteObjectUndo("Graph ImageChange");

            imageAsset.SetImage(payload);
            if (imageAsset.position.size == new Vector2(100, 100))
            {
                Rect position = imageAsset.position;
                position.size = ImageItemUtility.CalculateInitialSize(payload.width, payload.height);
                imageAsset.position = position;
                base.SetPosition(position);
            }

            RefreshImage();
            graphView.graphSave.SetDirty();
        }

        private void RefreshImage()
        {
            if (previewTexture != null)
            {
                Object.DestroyImmediate(previewTexture);
                previewTexture = null;
            }

            if (imageAsset != null && imageAsset.hasImage)
            {
                previewTexture = ImageItemUtility.CreateTexture(imageAsset.imageData);
                image.image = previewTexture;
            }
            else
            {
                image.image = null;
            }

            bool hasImage = previewTexture != null;
            image.style.display = hasImage ? DisplayStyle.Flex : DisplayStyle.None;
            placeholder.style.display = hasImage ? DisplayStyle.None : DisplayStyle.Flex;
            RefreshBackground();
            RefreshBorder();
        }

        private void RefreshBackground()
        {
            if (imageAsset == null) return;

            style.backgroundColor = imageAsset.backgroundColor;
        }

        private void RefreshBorder()
        {
            bool hasImage = imageAsset != null && imageAsset.hasImage;
            float borderWidth = CalculateBorderWidth(hasImage, isSelected);

            style.borderTopWidth = borderWidth;
            style.borderRightWidth = borderWidth;
            style.borderBottomWidth = borderWidth;
            style.borderLeftWidth = borderWidth;
        }

        public override void SetPosition(Rect rect)
        {
            base.SetPosition(rect);
            if (graphView == null || imageAsset == null) return;

            graphView.RegisterCompleteObjectUndo("Graph MoveItem");
            imageAsset.position = rect;
        }

        public void SetPositionNoUndo(Rect position)
        {
            base.SetPosition(position);
            imageAsset.position = position;
        }

        public void OnValueChanged(bool isSilent = false)
        {
            SetPositionNoUndo(imageAsset.position);
            RefreshImage();
            if (isSilent == false) graphView.graphSave.SetDirty();
        }

        public ICopyPastePack GetPack() => new ItemCopyPastePack(asset);

        public void Delete()
        {
            graphView.itemSystem.DeleteItem(this);
        }

        public void RemoveView()
        {
            graphView.RemoveItemView(this);
        }

        public bool Validate() => true;

        public bool IsSelected() => isSelected;

        public void Select()
        {
            isSelected = true;
            AddToClassList("selected");
            RefreshBorder();
        }

        public void Unselect()
        {
            isSelected = false;
            RemoveFromClassList("selected");
            RefreshBorder();
        }

        public IEnumerable<Object> GetSelectedObjects()
        {
            yield return asset;
        }

        public void OnStartResize() { }

        public void OnResized()
        {
            SetPosition(GetPosition());
        }

        public void Dispose()
        {
            if (previewTexture != null)
            {
                Object.DestroyImmediate(previewTexture);
                previewTexture = null;
            }

            graphView = null;
        }
    }
}
