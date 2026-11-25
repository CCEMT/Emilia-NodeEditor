using System.Collections;
using System.Linq;
using Emilia.Reflection.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Emilia.Node.Editor
{
    public partial class EditorGraphView
    {
        /// <summary>
        /// 更新逻辑Transform
        /// </summary>
        public void UpdateLogicTransform(Vector3 position, Vector3 scale)
        {
            logicPosition = position;
            logicScale = scale;

            onLogicTransformChange?.Invoke(logicPosition, logicScale);
        }

        private void OnViewTransformChanged(GraphView graphview)
        {
            UpdateLogicTransform(graphview.viewTransform.position, graphview.viewTransform.scale);
        }

        protected override EventPropagation OverrideFrame(FrameType frameType)
        {
            if (graphPanelSystem == null) return base.OverrideFrame(frameType);

            Rect clientRect = this.WorldToLocal(graphPanelSystem.graphRect);
            float validateFloat = clientRect.x + clientRect.y + clientRect.width + clientRect.height;
            if (clientRect.width <= 0 || clientRect.height <= 0 || float.IsNaN(validateFloat) || float.IsInfinity(validateFloat)) return base.OverrideFrame(frameType);

            if (frameType == FrameType.Selection && !selection.OfType<GraphElement>().Any(element => element.IsSelectable() && element is not Edge))
                frameType = FrameType.All;

            Vector3 frameTranslation = clientRect.position;
            Vector3 frameScaling = Vector3.one;
            if (frameType == FrameType.All || frameType == FrameType.Selection)
            {
                Rect rectToFit;
                if (frameType == FrameType.All)
                {
                    rectToFit = CalculateRectToFitAll(contentViewContainer);
                }
                else
                {
                    rectToFit = Rect.zero;
                    bool hasElement = false;
                    foreach (GraphElement element in selection.OfType<GraphElement>())
                    {
                        VisualElement visualElement = element is Edge edge ? edge.edgeControl : element;
                        Rect elementRect = visualElement.ChangeCoordinatesTo(contentViewContainer, visualElement.GetRect_Internal());
                        rectToFit = hasElement ? RectUtils.Encompass(rectToFit, elementRect) : elementRect;
                        hasElement = true;
                    }
                }

                CalculateFrameTransform(rectToFit, clientRect, frameBorder_Internal, out frameTranslation, out frameScaling);
                // CalculateFrameTransform只使用尺寸，需要单独补上可用区域的本地偏移。
                frameTranslation += (Vector3) clientRect.position;
            }

            SetViewTransform(frameTranslation, frameScaling, frameAnimate_Internal ? 0.2f : 0);
            contentViewContainer.MarkDirtyRepaint();
            return EventPropagation.Stop;
        }

        protected override bool OverrideUpdateContentZoomer()
        {
            if (minScale != maxScale)
            {
                if (graphZoomer == null)
                {
                    graphZoomer = new GraphContentZoomer();
                    this.AddManipulator(graphZoomer);
                }

                graphZoomer.minScale = minScale;
                graphZoomer.maxScale = maxScale;
                graphZoomer.scaleStep = scaleStep;
                graphZoomer.referenceScale = referenceScale;
            }
            else
            {
                if (graphZoomer != null) this.RemoveManipulator(graphZoomer);
            }

            ValidateTransform();
            return true;
        }

        /// <summary>
        /// 设置视图Transform
        /// </summary>
        public void SetViewTransform(Vector3 newPosition, Vector3 newScale, float animationTime = 0.2f)
        {
            // 延迟一帧执行，确保在正确的UI更新周期中执行
            schedule.Execute(OnSetViewTransform).ExecuteLater(1);

            void OnSetViewTransform()
            {
                // 验证输入参数是否有效（检查无穷大和NaN）
                float validateFloat = newPosition.x + newPosition.y + newPosition.z + newScale.x + newScale.y + newScale.z;
                if (float.IsInfinity(validateFloat) || float.IsNaN(validateFloat)) return;

                // 将位置对齐到像素网格，避免模糊渲染
                newPosition.x = GUIUtility_Internals.RoundToPixelGrid_Internals(newPosition.x);
                newPosition.y = GUIUtility_Internals.RoundToPixelGrid_Internals(newPosition.y);

                UpdateLogicTransform(newPosition, newScale);

                // 防止未完成的缩放动画覆盖新的定位。
                if (updateViewTransformCoroutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(updateViewTransformCoroutine);
                    updateViewTransformCoroutine = null;
                }

                // 根据time参数决定使用动画过渡还是立即应用
                if (animationTime > 0)
                {
                    updateViewTransformCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(SetTransformAnimation());
                }
                else
                {
                    // 立即应用新的Transform
                    contentViewContainer.transform.position = newPosition;
                    contentViewContainer.transform.scale = newScale;

                    UpdatePersistedViewTransform_Internals();
                    if (viewTransformChanged != null) viewTransformChanged(this);
                }
            }

            // Transform动画协程，通过线性插值实现平滑过渡
            IEnumerator SetTransformAnimation()
            {
                // 记录动画起始状态
                Vector2 startPosition = contentViewContainer.transform.position;
                Vector3 startScale = contentViewContainer.transform.scale;

                double startTime = EditorApplication.timeSinceStartup;

                // 动画循环，直到达到指定时长
                while (EditorApplication.timeSinceStartup - startTime < animationTime)
                {
                    float t = (float) ((EditorApplication.timeSinceStartup - startTime) / animationTime);

                    Vector2 currentPosition = Vector2.Lerp(startPosition, newPosition, t);
                    Vector3 currentScale = Vector3.Lerp(startScale, newScale, t);

                    contentViewContainer.transform.position = currentPosition;
                    contentViewContainer.transform.scale = currentScale;

                    // 等待下一帧
                    yield return 0;
                }

                // 动画结束，确保最终值精确应用
                contentViewContainer.transform.position = newPosition;
                contentViewContainer.transform.scale = newScale;

                viewTransformChanged?.Invoke(this);
                UpdatePersistedViewTransform_Internals();

                updateViewTransformCoroutine = null;
            }
        }
    }
}
