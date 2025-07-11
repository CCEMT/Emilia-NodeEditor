using System.IO;
using Emilia.Kit;
using Emilia.Kit.Editor;
using UnityEditor;

namespace Emilia.Node.Editor
{
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
        public EditorGraphAsset ResetCopy(EditorGraphAsset source)
        {
            if (source == null) return null;
            this.sourceGraphAsset = source;

            string path = AssetDatabase.GetAssetPath(source);
            string tempPath = $"{TempFolderKit.TempFolderPath}/{source.name}.asset";

            TempFolderKit.CreateTempFolder();

            AssetDatabase.CopyAsset(path, tempPath);

            EditorGraphAsset copy = AssetDatabase.LoadAssetAtPath<EditorGraphAsset>(tempPath);

            return copy;
        }

        /// <summary>
        /// 设置为脏数据状态
        /// </summary>
        public void SetDirty()
        {
            if (this.graphView.loadProgress != 1) return;
            this._dirty = true;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void OnSave()
        {
            if (this.graphView == null) return;

            handle?.OnSaveBefore(this.graphView);

            if (this.graphView.graphAsset != null) graphView.graphAsset.SaveAll();

            this.graphView.graphLocalSettingSystem.SaveAll();

            if (sourceGraphAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(graphView.graphAsset);
                string savePath = AssetDatabase.GetAssetPath(this.sourceGraphAsset);

                string filePath = Path.GetFullPath(path);
                string saveFilePath = Path.GetFullPath(savePath);

                File.Copy(filePath, saveFilePath, true);
                AssetDatabase.ImportAsset(savePath);
            }

            this._dirty = false;

            handle?.OnSaveAfter(this.graphView);
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