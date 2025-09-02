using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Emilia.Reflection.Editor;
using MonoHook;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Kit.Editor
{
    public class SearchWindow_Hook : SearchWindow_Internals
    {
        [InitializeOnLoadMethod]
        static void InstallationHook()
        {
            Type searchWindowViewType = typeof(SearchWindow);
            HookRebuildSearch(searchWindowViewType);
        }

        private static void HookRebuildSearch(Type searchWindowViewType)
        {
            MethodInfo methodInfo = searchWindowViewType.GetMethod("RebuildSearch", BindingFlags.Instance | BindingFlags.NonPublic);

            Type graphViewHookType = typeof(SearchWindow_Hook);
            MethodInfo hookInfo = graphViewHookType.GetMethod(nameof(RebuildSearch_Hook), BindingFlags.Instance | BindingFlags.NonPublic);

            MethodHook hook = new MethodHook(methodInfo, hookInfo, null);
            hook.Install();
        }

        public static bool Open<P, W>(SearchWindowContext context, P provider) where P : ScriptableObject, ISearchWindowProvider where W : SearchWindow
        {
            Object[] objectsOfTypeAll = Resources.FindObjectsOfTypeAll(typeof(SearchWindow));
            if (objectsOfTypeAll.Length != 0)
            {
                try
                {
                    ((EditorWindow) objectsOfTypeAll[0]).Close();
                    return false;
                }
                catch
                {
                    filterWindow_Internal = null;
                }
            }
            if (DateTime.Now.Ticks / 10000L < lastClosedTime_Internal + 50L) return false;
            if (filterWindow_Internal == null)
            {
                filterWindow_Internal = CreateInstance<W>();
                filterWindow_Internal.hideFlags = HideFlags.HideAndDontSave;
            }

            filterWindow_Internal.Init_Internals(context, provider);
            return true;
        }

        protected virtual bool CanSearchPro() => true;

        private void RebuildSearch_Hook()
        {
            object result = ReflectUtility.Invoke(this, nameof(CanSearchPro));
            if (result is true) RebuildSearchPro();
            else BaseRebuildSearch();
        }

        private void RebuildSearchPro()
        {
            if (! hasSearch_Internals)
            {
                searchResultTree_Internals = null;
                if (selectionStack_Internals[selectionStack_Internals.Count - 1].name == "Search")
                {
                    selectionStack_Internals.Clear();
                    selectionStack_Internals.Add(tree_Internals[0] as SearchTreeGroupEntry);
                }
                animTarget_Internals = 1;
                lastTime_Internals = DateTime.Now.Ticks;
            }
            else
            {
                List<(SearchTreeEntry, int)> collection = new();

                foreach (SearchTreeEntry searchTreeEntry in tree_Internals)
                {
                    if (searchTreeEntry is SearchTreeGroupEntry) continue;
                    string entryName = searchTreeEntry.name;
                    int source = SearchUtility.Search(entryName, search_Internals);
                    if (source != 0) collection.Add((searchTreeEntry, source));
                }

                collection.Sort((a, b) => b.Item2.CompareTo(a.Item2));

                List<SearchTreeEntry> searchTreeEntryList = new List<SearchTreeEntry>();
                searchTreeEntryList.Add(new SearchTreeGroupEntry(new GUIContent("Search")));
                searchTreeEntryList.AddRange(collection.Select((i) => i.Item1));
                searchResultTree_Internals = searchTreeEntryList.ToArray();
                selectionStack_Internals.Clear();
                selectionStack_Internals.Add(searchResultTree_Internals[0] as SearchTreeGroupEntry);
                if (GetChildren_Internals(activeTree_Internals, activeParent_Internals).Count >= 1) activeParent_Internals.SetSelectedIndex(0);
                else activeParent_Internals.SetSelectedIndex(-1);
            }
        }

        private void BaseRebuildSearch()
        {
            if (! hasSearch_Internals)
            {
                searchResultTree_Internals = null;
                if (selectionStack_Internals[selectionStack_Internals.Count - 1].name == "Search")
                {
                    selectionStack_Internals.Clear();
                    selectionStack_Internals.Add(tree_Internals[0] as SearchTreeGroupEntry);
                }
                animTarget_Internals = 1;
                lastTime_Internals = DateTime.Now.Ticks;
            }
            else
            {
                string[] strArray = search_Internals.ToLower().Split(' ');
                List<SearchTreeEntry> collection1 = new List<SearchTreeEntry>();
                List<SearchTreeEntry> collection2 = new List<SearchTreeEntry>();
                foreach (SearchTreeEntry searchTreeEntry in tree_Internals)
                {
                    if (searchTreeEntry is SearchTreeGroupEntry) continue;
                    string str1 = searchTreeEntry.name.ToLower().Replace(" ", "");
                    bool flag1 = true;
                    bool flag2 = false;
                    for (int index = 0; index < strArray.Length; ++index)
                    {
                        string str2 = strArray[index];
                        if (str1.Contains(str2))
                        {
                            if (index == 0 && str1.StartsWith(str2)) flag2 = true;
                        }
                        else
                        {
                            flag1 = false;
                            break;
                        }
                    }

                    if (! flag1) continue;
                    if (flag2) collection1.Add(searchTreeEntry);
                    else collection2.Add(searchTreeEntry);
                }
                collection1.Sort();
                collection2.Sort();
                List<SearchTreeEntry> searchTreeEntryList = new List<SearchTreeEntry>();
                searchTreeEntryList.Add(new SearchTreeGroupEntry(new GUIContent("Search")));
                searchTreeEntryList.AddRange(collection1);
                searchTreeEntryList.AddRange(collection2);
                searchResultTree_Internals = searchTreeEntryList.ToArray();
                selectionStack_Internals.Clear();
                selectionStack_Internals.Add(searchResultTree_Internals[0] as SearchTreeGroupEntry);
                if (GetChildren_Internals(activeTree_Internals, activeParent_Internals).Count >= 1) activeParent_Internals.SetSelectedIndex(0);
                else activeParent_Internals.SetSelectedIndex(-1);
            }
        }
    }
}