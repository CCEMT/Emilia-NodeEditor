using Emilia.Kit;
using Emilia.Node.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalGraphHandle : GraphHandle<EditorUniversalGraphAsset>
    {
        public const string GridBackgroundStyleFilePath = "Node/Styles/GridBackground.uss";
        public const string GraphViewStyleFilePath = "Node/Styles/UniversalEditorGraphView.uss";

        private GraphLoadingContainer loadingContainer;

        public override void OnLoadBefore()
        {
            AddManipulator();
            GraphViewInitialize();
            AddGridBackground();
            AddLoadingMask();

            smartValue.onLogicTransformChange -= OnLogicTransformChange;
            smartValue.onLogicTransformChange += OnLogicTransformChange;
        }

        private void OnLogicTransformChange(Vector3 position, Vector3 scale)
        {
            UniversalGraphAssetLocalSetting setting = smartValue.graphLocalSettingSystem.assetSetting as UniversalGraphAssetLocalSetting;
            setting.position = position;
            setting.scale = scale;
        }

        protected void AddManipulator()
        {
            smartValue.AddManipulator(new ContentDragger());
            smartValue.AddManipulator(new GraphSelectionDragger());
            smartValue.AddManipulator(new GraphRectangleSelector());
        }

        protected void AddGridBackground()
        {
            GridBackground background = new GridBackground();
            StyleSheet styleSheet = ResourceUtility.LoadResource<StyleSheet>(GridBackgroundStyleFilePath);
            background.styleSheets.Add(styleSheet);

            smartValue.Insert(0, background);
        }

        protected void GraphViewInitialize()
        {
            StyleSheet graphViewStyleSheet = ResourceUtility.LoadResource<StyleSheet>(GraphViewStyleFilePath);
            smartValue.styleSheets.Add(graphViewStyleSheet);
        }

        private void AddLoadingMask()
        {
            if (this.loadingContainer == null)
            {
                this.loadingContainer = new GraphLoadingContainer(smartValue);
                smartValue.Add(this.loadingContainer);
            }

            this.loadingContainer.style.display = DisplayStyle.Flex;
            this.loadingContainer.DisplayLoading();

            smartValue.SetEnabled(false);
        }

        public override void OnLoadAfter()
        {
            this.loadingContainer.style.display = DisplayStyle.None;
            smartValue.SetEnabled(true);
            
            UniversalGraphAssetLocalSetting universalSetting = smartValue.graphLocalSettingSystem.assetSetting as UniversalGraphAssetLocalSetting;
            smartValue.UpdateViewTransform(universalSetting.position, universalSetting.scale);
        }

        public override void Dispose()
        {
            smartValue.onLogicTransformChange -= OnLogicTransformChange;
            base.Dispose();
        }
    }
}