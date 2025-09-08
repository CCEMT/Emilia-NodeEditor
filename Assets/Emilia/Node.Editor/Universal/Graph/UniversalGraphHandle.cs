using Emilia.Kit;
using Emilia.Node.Editor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Universal.Editor
{
    [EditorHandle(typeof(EditorUniversalGraphAsset))]
    public class UniversalGraphHandle : GraphHandle
    {
        private const string TransformPositionSetting = "TransformPositionKey";
        private const string TransformScaleSetting = "TransformScaleKey";

        public const string GridBackgroundStyleFilePath = "Node/Styles/GridBackground.uss";
        public const string GraphViewStyleFilePath = "Node/Styles/UniversalEditorGraphView.uss";

        private EditorGraphView editorGraphView;
        private GraphLoadingContainer loadingContainer;
        private GraphCompilationContainer currentCompilationContainer;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            editorGraphView = graphView;
        }

        public override void OnLoadBefore(EditorGraphView graphView)
        {
            base.OnLoadBefore(graphView);
            AddManipulator();
            GraphViewInitialize();
            AddGridBackground();
            AddLoadingMask();

            graphView.onLogicTransformChange -= OnLogicTransformChange;
            graphView.onLogicTransformChange += OnLogicTransformChange;

            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationStarted += OnCompilationStarted;
        }

        private void OnLogicTransformChange(Vector3 position, Vector3 scale)
        {
            editorGraphView.graphLocalSettingSystem.SetAssetSettingValue(TransformPositionSetting, position);
            editorGraphView.graphLocalSettingSystem.SetAssetSettingValue(TransformScaleSetting, scale);
        }

        private void OnCompilationStarted(object context)
        {
            currentCompilationContainer = new GraphCompilationContainer();
            editorGraphView.Add(currentCompilationContainer);
            editorGraphView.SetEnabled(false);

            editorGraphView.onUpdate += CheckCompilationFinished;
        }

        private void CheckCompilationFinished()
        {
            if (EditorApplication.isCompiling) return;
            if (currentCompilationContainer != null) this.currentCompilationContainer.RemoveFromHierarchy();

            if (this.editorGraphView != null) this.editorGraphView.SetEnabled(true);

            EditorApplication.update -= CheckCompilationFinished;
            this.currentCompilationContainer = null;
        }

        protected virtual void AddManipulator()
        {
            editorGraphView.AddManipulator(new GraphContentDragger());
            editorGraphView.AddManipulator(new GraphSelectionDragger());
            editorGraphView.AddManipulator(new GraphRectangleSelector());
        }

        protected void AddGridBackground()
        {
            GridBackground background = new GridBackground();
            StyleSheet styleSheet = ResourceUtility.LoadResource<StyleSheet>(GridBackgroundStyleFilePath);
            background.styleSheets.Add(styleSheet);

            editorGraphView.Insert(0, background);
        }

        protected void GraphViewInitialize()
        {
            StyleSheet graphViewStyleSheet = ResourceUtility.LoadResource<StyleSheet>(GraphViewStyleFilePath);
            editorGraphView.styleSheets.Add(graphViewStyleSheet);
        }

        private void AddLoadingMask()
        {
            if (this.loadingContainer == null)
            {
                this.loadingContainer = new GraphLoadingContainer(editorGraphView);
                editorGraphView.Add(this.loadingContainer);
            }

            this.loadingContainer.style.display = DisplayStyle.Flex;
            this.loadingContainer.DisplayLoading();

            editorGraphView.SetEnabled(false);
        }

        public override void OnLoadAfter(EditorGraphView graphView)
        {
            base.OnLoadAfter(graphView);
            this.loadingContainer.style.display = DisplayStyle.None;
            graphView.SetEnabled(true);

            Vector3 position = graphView.transform.position;
            if (editorGraphView.graphLocalSettingSystem.HasAssetSetting(TransformPositionSetting))
            {
                position = editorGraphView.graphLocalSettingSystem.GetAssetSettingValue<Vector3>(TransformPositionSetting);
            }

            Vector3 scale = graphView.transform.scale;
            if (editorGraphView.graphLocalSettingSystem.HasAssetSetting(TransformScaleSetting))
            {
                editorGraphView.graphLocalSettingSystem.GetAssetSettingValue<Vector3>(TransformScaleSetting);
            }

            graphView.UpdateViewTransform(position, scale);
        }

        public override void Dispose(EditorGraphView graphView)
        {
            graphView.onLogicTransformChange -= OnLogicTransformChange;
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            graphView.onUpdate -= CheckCompilationFinished;

            editorGraphView = null;
            currentCompilationContainer = null;
        }
    }
}