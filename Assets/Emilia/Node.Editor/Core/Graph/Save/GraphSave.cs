using System.IO;
using Emilia.Kit;
using UnityEditor;

namespace Emilia.Node.Editor
{
    public class GraphSave : GraphViewModule
    {
        private EditorGraphAsset sourceGraphAsset;

        private IGraphSaveHandle handle;

        private bool _dirty;

        public bool dirty => this._dirty;
        public override int order => 500;

        public override void Reset(EditorGraphView graphView)
        {
            base.Reset(graphView);

            if (handle != null) EditorHandleUtility.ReleaseHandle(handle);
            handle = EditorHandleUtility.BuildHandle<IGraphSaveHandle>(this.graphView.graphAsset.GetType(), this.graphView);
        }

        public EditorGraphAsset ResetCopy(EditorGraphAsset source)
        {
            this.sourceGraphAsset = source;

            string path = AssetDatabase.GetAssetPath(source);
            string tempPath = $"{EditorAssetKit.dataParentPath}/Temp/{source.name}.asset";

            File.Copy(path, tempPath, true);

            EditorGraphAsset copy = AssetDatabase.LoadAssetAtPath<EditorGraphAsset>(tempPath);

            return copy;
        }

        public void SetDirty()
        {
            if (this.graphView.loadProgress != 1) return;
            this._dirty = true;
        }

        public void OnSave()
        {
            handle?.OnSaveBefore();

            if (this.graphView.graphAsset != null) graphView.graphAsset.Save();

            this.graphView.graphLocalSettingSystem.SaveAll();

            if (sourceGraphAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(graphView.graphAsset);
                string savePath = AssetDatabase.GetAssetPath(this.sourceGraphAsset);

                File.Copy(path, savePath, true);
                AssetDatabase.ImportAsset(savePath);
            }

            this._dirty = false;

            handle?.OnSaveAfter();
        }

        public override void Dispose()
        {
            this._dirty = false;
            
            this.sourceGraphAsset = null;
            
            if (handle != null)
            {
                EditorHandleUtility.ReleaseHandle(handle);
                handle = null;
            }
            
            base.Dispose();
        }
    }
}