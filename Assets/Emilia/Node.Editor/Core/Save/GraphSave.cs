using System.IO;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Emilia.Node.Attributes;
using UnityEditor;

namespace Emilia.Node.Editor
{
    /// <summary>
    /// 保存系统
    /// </summary>
    public class GraphSave : BasicGraphViewModule
    {
        private EditorGraphAsset sourceGraphAsset;

        private GraphSaveHandle handle;

        private bool _dirty;

        public bool dirty => this._dirty;
        public override int order => 500;

        public override void Initialize(EditorGraphView graphView)
        {
            base.Initialize(graphView);
            handle = EditorHandleUtility.CreateHandle<GraphSaveHandle>(this.graphView.graphAsset.GetType());
        }

        /// <summary>
        /// 重置副本
        /// </summary>
        public void ResetCopy(EditorGraphView editorGraphView, EditorGraphAsset source)
        {
            if (source == null) return;

            string path = AssetDatabase.GetAssetPath(source);
            string tempPath = $"{TempFolderKit.TempFolderPath}/{source.name}.asset";

            TempFolderKit.CreateTempFolder();

            bool isTemp = path.Contains(TempFolderKit.TempFolderPath);
            if (isTemp)
            {
                if (this.sourceGraphAsset == null) return;

                path = AssetDatabase.GetAssetPath(sourceGraphAsset);
                source = sourceGraphAsset;
            }

            bool isExist = AssetDatabase.LoadAssetAtPath<EditorGraphAsset>(tempPath);
            if (isExist) AssetDatabase.DeleteAsset(tempPath);

            bool result = AssetDatabase.CopyAsset(path, tempPath);
            if (result == false) return;

            EditorGraphAsset copy = AssetDatabase.LoadAssetAtPath<EditorGraphAsset>(tempPath);
            if (copy == null) return;

            this.sourceGraphAsset = source;
            editorGraphView.graphAsset = copy;
        }

        /// <summary>
        /// 设置为脏数据状态
        /// </summary>
        public void SetDirty()
        {
            if (this.graphView == null) return;
            if (this.graphView.isInitialized == false) return;
            this._dirty = true;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save(EditorGraphView editorGraphView, bool force = true)
        {
            if (editorGraphView == null) return;

            if (force) OnSave(editorGraphView);
            else
            {
                GraphSettingStruct? graphSetting = editorGraphView.GetGraphData<BasicGraphData>()?.graphSetting;

                bool isInquire = graphSetting != null && graphSetting.Value.immediatelySave == false && dirty;
                if (isInquire == false) OnSave(editorGraphView);
                else
                {
                    if (EditorUtility.DisplayDialog("是否保存", "是否保存当前修改", "保存", "不保存")) OnSave(editorGraphView);
                }
            }
        }

        private void OnSave(EditorGraphView editorGraphView)
        {
            if (editorGraphView == null) return;

            handle?.OnSaveBefore(editorGraphView);

            if (editorGraphView.graphAsset != null) editorGraphView.graphAsset.SaveAll();

            editorGraphView.graphLocalSettingSystem.SaveAll();

            if (sourceGraphAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(editorGraphView.graphAsset);
                string savePath = AssetDatabase.GetAssetPath(this.sourceGraphAsset);

                string filePath = Path.GetFullPath(path);
                string saveFilePath = Path.GetFullPath(savePath);

                File.Copy(filePath, saveFilePath, true);
                AssetDatabase.ImportAsset(savePath);
            }

            this._dirty = false;

            handle?.OnSaveAfter(editorGraphView);
        }

        public override void Dispose()
        {
            this._dirty = false;
            this.sourceGraphAsset = null;
            this.handle = null;
            base.Dispose();
        }
    }
}