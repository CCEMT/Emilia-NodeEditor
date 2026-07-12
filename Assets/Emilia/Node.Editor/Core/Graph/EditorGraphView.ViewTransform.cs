using System.Collections;
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

                // 根据time参数决定使用动画过渡还是立即应用
                if (animationTime > 0)
                {
                    // 停止之前的Transform动画协程（如果存在）
                    if (updateViewTransformCoroutine != null) EditorCoroutineUtility.StopCoroutine(updateViewTransformCoroutine);
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
