using System.Linq;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Example
{
    public class AllExampleWindow : OdinMenuEditorWindow
    {
        [MenuItem("Emilia/AllExampleWindow")]
        public static void OpenWindow()
        {
            EditorImGUIKit.OpenWindow<AllExampleWindow>("所有例子", 1200, 900);
        }

        [MenuItem("Emilia/清除EditorPrefs")]
        public static void ClearEditorPrefs()
        {
            EditorPrefs.DeleteAll();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            MenuWidth = 300;
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();
            tree.Config.DrawSearchToolbar = true;
            tree.Config.SearchFunction = item => SearchUtility.Matching(item.SearchString, tree.Config.SearchTerm);

            ExampleAsset[] editorGuideAsset = EditorAssetKit.GetEditorResources<ExampleAsset>();

            int amount = editorGuideAsset.Length;
            for (int i = 0; i < amount; i++)
            {
                ExampleAsset asset = editorGuideAsset[i];
                ExampleTreeItem treeItem = new ExampleTreeItem(asset);

                string itemName = asset.name;
                if (string.IsNullOrEmpty(asset.description) == false) itemName += $"({asset.description})";

                tree.Add(itemName, treeItem);
            }

            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            if (MenuTree == null) return;

            int toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;
            OdinMenuItem selected = this.MenuTree.Selection.FirstOrDefault();

            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            Toolbar(selected);
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        void Toolbar(OdinMenuItem selected)
        {
            ExampleTreeItem exampleTreeItem = selected?.Value as ExampleTreeItem;

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("刷新"))) ForceMenuTreeRebuild();

            if (exampleTreeItem != null)
            {
                GUI.color = Color.cyan;
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("修改描述"))) EditorImGUIKit.InputDropDown((text) => SetDescription(exampleTreeItem, text));
                GUI.color = Color.white;
            }

            GUILayout.FlexibleSpace();

            if (exampleTreeItem != null)
            {
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("选择文件"))) SelectAsset(exampleTreeItem);
            }
        }

        void SetDescription(ExampleTreeItem exampleTreeItem, string input)
        {
            ExampleAsset exampleAsset = exampleTreeItem.asset;
            if (exampleAsset == null) return;

            exampleAsset.description = input;
            EditorUtility.SetDirty(exampleAsset);
            AssetDatabase.SaveAssets();

            ForceMenuTreeRebuild();
        }

        void SelectAsset(ExampleTreeItem exampleTreeItem)
        {
            ExampleAsset exampleAsset = exampleTreeItem.asset;
            if (exampleAsset == null) return;

            EditorGUIUtility.PingObject(exampleAsset);
            Selection.activeObject = exampleAsset;
        }
    }
}