using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emilia.Reflection.Editor;
using FuzzySharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Emilia.Kit.Editor
{
    public static class OpenScriptUtility
    {
        public static void OpenScript(object target)
        {
            MonoBehaviour behaviour = target as MonoBehaviour;
            if (behaviour != null)
            {
                MonoScript monoScript = MonoScript.FromMonoBehaviour(behaviour);
                if (monoScript != null)
                {
                    AssetDatabase.OpenAsset(monoScript);
                    return;
                }
            }

            ScriptableObject scriptableObject = target as ScriptableObject;
            if (scriptableObject != null)
            {
                MonoScript monoScript = MonoScript.FromScriptableObject(scriptableObject);
                if (monoScript != null)
                {
                    AssetDatabase.OpenAsset(monoScript);
                    return;
                }
            }

            Type type = target.GetType();
            OpenScriptCache cache = OpenScriptCache.Get();
            string fullName = $"{type.FullName}, {type.Assembly.GetName().Name}";
            bool hasCached = cache.typeInfos.ContainsKey(fullName);

            if (hasCached)
            {
                OpenScript(type);
            }
            else
            {
                if (EditorUtility.DisplayDialog("确认", "是否通过Roslyn打开脚本\n（此操作的时间会比较长）", "是", "否")) OpenScript(type);
            }
        }

        public static void OpenScript(Type type)
        {
            OpenScriptCache cache = OpenScriptCache.Get();
            string fullName = $"{type.FullName}, {type.Assembly.GetName().Name}";
            TypeInfo typeInfo = cache.typeInfos.GetValueOrDefault(fullName);

            if (typeInfo != null)
            {
                ScriptInfo scriptInfo = cache.scriptInfos.GetValueOrDefault(typeInfo.guid);
                if (scriptInfo != null)
                {
                    string path = AssetDatabase.GUIDToAssetPath(typeInfo.guid);
                    string fullPath = string.IsNullOrEmpty(path) ? null : Path.GetFullPath(path);

                    if (fullPath == null || ! File.Exists(fullPath))
                    {
                        FullRefresh(cache, fullName, type);
                        return;
                    }

                    long currentTicks = File.GetLastWriteTimeUtc(fullPath).Ticks;

                    if (scriptInfo.lastWriteTimeTicks == currentTicks)
                    {
                        MonoScript mono = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                        if (mono != null) AssetDatabase.OpenAsset(mono, typeInfo.line);
                        
                        return;
                    }

                    RefreshSingleScript(cache, scriptInfo, path, () => {
                        TypeInfo updated = cache.typeInfos.GetValueOrDefault(fullName);

                        if (updated == null) FullRefresh(cache, fullName, type);
                        else
                        {
                            string p = AssetDatabase.GUIDToAssetPath(updated.guid);
                            MonoScript mono = AssetDatabase.LoadAssetAtPath<MonoScript>(p);
                            if (mono != null) AssetDatabase.OpenAsset(mono, updated.line);
                        }

                    });
                    
                    return;
                }
            }

            FullRefresh(cache, fullName, type);
        }

        static void FullRefresh(OpenScriptCache cache, string fullName, Type type)
        {
            string targetAssemblyName = type.Assembly.GetName().Name;
            string targetTypeName = type.Name;

            RefreshScriptCache(fullName, targetAssemblyName, targetTypeName, () => {
                TypeInfo found = cache.typeInfos.GetValueOrDefault(fullName);
                if (found == null)
                {
                    Debug.LogError($"未找到脚本信息：{type}");
                    return;
                }

                string p = AssetDatabase.GUIDToAssetPath(found.guid);
                MonoScript mono = AssetDatabase.LoadAssetAtPath<MonoScript>(p);
                if (mono != null) AssetDatabase.OpenAsset(mono, found.line);
            });
        }

        private static ConcurrentDictionary<string, string[]> assemblyDefineCache = new();

        static void RefreshSingleScript(OpenScriptCache cache, ScriptInfo scriptInfo, string assetPath, Action onCompleted)
        {
            string fullPath = Path.GetFullPath(assetPath);
            string assemblyName = CompilationPipeline.GetAssemblyNameFromScriptPath(assetPath);

            if (string.IsNullOrEmpty(assemblyName))
            {
                onCompleted?.Invoke();
                return;
            }

            assemblyName = assemblyName.Replace(".dll", "");
            string csprojPath = $"{EditorAssetKit.dataParentPath}/{assemblyName}.csproj";

            if (File.Exists(csprojPath) == false)
            {
                onCompleted?.Invoke();
                return;
            }

            Task.Run(() => {
                string code = File.ReadAllText(fullPath);
                string[] defines = assemblyDefineCache.GetOrAdd(csprojPath, p => {
                    string text = File.ReadAllText(p);
                    return text.Split(new[] {"<DefineConstants>"}, StringSplitOptions.None)[1]
                        .Split(new[] {"</DefineConstants>"}, StringSplitOptions.None)[0]
                        .Split(';');
                });

                CSharpParseOptions options = new(preprocessorSymbols: defines);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(code, options);
                SyntaxNode root = tree.GetRoot();

                List<TypeInfo> typeInfos = new();
                string capturedAssemblyName = assemblyName;
                string capturedGuid = scriptInfo.guid;
                BuildTypeInfo(root, null, "", (name, line) => {
                    TypeInfo ti = new();
                    ti.guid = capturedGuid;
                    ti.typeFullName = $"{name}, {capturedAssemblyName}";
                    ti.line = line;
                    typeInfos.Add(ti);
                });

                scriptInfo.lastWriteTimeTicks = File.GetLastWriteTimeUtc(fullPath).Ticks;

                EditorApplication_Internals.CallDelayed_Internals(() => {
                    int oldCount = scriptInfo.typeInfos.Count;
                    for (int i = 0; i < oldCount; i++) cache.typeInfos.Remove(scriptInfo.typeInfos[i].typeFullName);

                    scriptInfo.typeInfos = typeInfos;
                    int newCount = typeInfos.Count;
                    for (int i = 0; i < newCount; i++) cache.typeInfos[typeInfos[i].typeFullName] = typeInfos[i];

                    OpenScriptCache.Save();
                    onCompleted?.Invoke();
                }, 0.001d);
            });
        }

        public static void RefreshScriptCache(string targetFullName, string targetAssemblyName, string targetTypeName, Action onCompleted = null)
        {
            OpenScriptCache openScriptCache = OpenScriptCache.Get();

            List<(ScriptInfo info, string assetPath)> refreshList = new();

            EditorUtility.DisplayProgressBar("刷新脚本缓存", "正在刷新脚本缓存", 0);

            string[] guids = AssetDatabase.FindAssets("t:MonoScript");
            HashSet<string> activeGuids = new(guids.Length);

            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                activeGuids.Add(guid);

                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fullPath = Path.GetFullPath(path);

                if (File.Exists(fullPath) == false) continue;

                long currentTicks = File.GetLastWriteTimeUtc(fullPath).Ticks;

                ScriptInfo scriptInfo = openScriptCache.scriptInfos.GetValueOrDefault(guid);
                if (scriptInfo == null)
                {
                    ScriptInfo createScriptInfo = new();
                    createScriptInfo.guid = guid;
                    createScriptInfo.lastWriteTimeTicks = currentTicks;
                    openScriptCache.scriptInfos[guid] = createScriptInfo;

                    refreshList.Add((createScriptInfo, path));
                }
                else if (scriptInfo.lastWriteTimeTicks != currentTicks)
                {
                    scriptInfo.lastWriteTimeTicks = currentTicks;
                    refreshList.Add((scriptInfo, path));
                }
            }

            List<string> staleKeys = new();
            foreach (string key in openScriptCache.scriptInfos.Keys)
            {
                if (activeGuids.Contains(key) == false) staleKeys.Add(key);
            }

            for (int i = 0; i < staleKeys.Count; i++)
            {
                string staleKey = staleKeys[i];
                ScriptInfo staleInfo = openScriptCache.scriptInfos[staleKey];
                for (int j = 0; j < staleInfo.typeInfos.Count; j++) openScriptCache.typeInfos.Remove(staleInfo.typeInfos[j].typeFullName);
                openScriptCache.scriptInfos.Remove(staleKey);
            }

            if (refreshList.Count == 0)
            {
                if (staleKeys.Count > 0) OpenScriptCache.Save();
                onCompleted?.Invoke();
                EditorUtility.ClearProgressBar();
                return;
            }

            bool hasTarget = targetFullName != null;

            if (hasTarget)
            {
                var sortedList = refreshList.Select(item => {
                        string fileName = Path.GetFileNameWithoutExtension(item.assetPath);
                        string assemblyName = CompilationPipeline.GetAssemblyNameFromScriptPath(item.assetPath)?.Replace(".dll", "");
                        bool isSameAssembly = assemblyName == targetAssemblyName;
                        int similarityScore = Fuzz.WeightedRatio(fileName, targetTypeName);
                        return (item.info, item.assetPath, isSameAssembly, similarityScore);
                    })
                    .OrderByDescending(x => x.isSameAssembly)
                    .ThenByDescending(x => x.similarityScore)
                    .ToList();

                int totalCount = sortedList.Count;
                ProcessSequentialRefresh(openScriptCache, sortedList, 0, totalCount, targetFullName, onCompleted);
            }
            else
            {
                List<ScriptInfo> successList = new();
                int totalCount = refreshList.Count;

                for (int i = 0; i < totalCount; i++)
                {
                    var (info, assetPath) = refreshList[i];
                    RefreshCache(openScriptCache, info, assetPath, () => OnCompleted(info, totalCount));
                }

                void OnCompleted(ScriptInfo scriptInfo, int total)
                {
                    successList.Add(scriptInfo);
                    float t = (float) successList.Count / total;
                    EditorUtility.DisplayProgressBar("刷新脚本缓存", $"正在刷新脚本缓存({successList.Count}/{total})", t);

                    if (successList.Count != total) return;

                    OpenScriptCache.Save();
                    onCompleted?.Invoke();
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        static void ProcessSequentialRefresh(
            OpenScriptCache cache,
            List<(ScriptInfo info, string assetPath, bool isSameAssembly, int similarityScore)> sortedList,
            int index,
            int totalCount,
            string targetFullName,
            Action onCompleted)
        {
            if (index >= sortedList.Count)
            {
                OpenScriptCache.Save();
                onCompleted?.Invoke();
                EditorUtility.ClearProgressBar();
                return;
            }

            var (info, assetPath, _, _) = sortedList[index];
            float t = (float) index / totalCount;
            EditorUtility.DisplayProgressBar("刷新脚本缓存", $"正在刷新脚本缓存({index}/{totalCount})", t);

            RefreshCache(cache, info, assetPath, () => {
                if (cache.typeInfos.ContainsKey(targetFullName))
                {
                    OpenScriptCache.Save();
                    onCompleted?.Invoke();
                    EditorUtility.ClearProgressBar();
                    return;
                }

                ProcessSequentialRefresh(cache, sortedList, index + 1, totalCount, targetFullName, onCompleted);
            });
        }

        static void RefreshCache(OpenScriptCache openScriptCache, ScriptInfo scriptInfo, string assetPath, Action onCompleted = null)
        {
            string fullPath = Path.GetFullPath(assetPath);
            string assemblyName = CompilationPipeline.GetAssemblyNameFromScriptPath(assetPath);

            if (string.IsNullOrEmpty(assemblyName))
            {
                EditorApplication.delayCall += () => onCompleted?.Invoke();
                return;
            }

            assemblyName = assemblyName.Replace(".dll", "");
            string csprojPath = $"{EditorAssetKit.dataParentPath}/{assemblyName}.csproj";

            if (File.Exists(csprojPath) == false)
            {
                EditorApplication.delayCall += () => onCompleted?.Invoke();
                return;
            }

            Task.Run(() => {
                string code = File.ReadAllText(fullPath);

                List<TypeInfo> typeInfos = new();

                string[] defines = assemblyDefineCache.GetOrAdd(csprojPath, p => {
                    string text = File.ReadAllText(p);
                    return text.Split(new[] {"<DefineConstants>"}, StringSplitOptions.None)[1]
                        .Split(new[] {"</DefineConstants>"}, StringSplitOptions.None)[0]
                        .Split(';');
                });

                CSharpParseOptions options = new(preprocessorSymbols: defines);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(code, options);
                SyntaxNode root = tree.GetRoot();

                string capturedAssemblyName = assemblyName;
                string capturedGuid = scriptInfo.guid;
                BuildTypeInfo(root, null, "", (fullName, line) => {
                    TypeInfo createTypeInfo = new();
                    createTypeInfo.guid = capturedGuid;
                    createTypeInfo.typeFullName = $"{fullName}, {capturedAssemblyName}";
                    createTypeInfo.line = line;
                    typeInfos.Add(createTypeInfo);
                });

                EditorApplication_Internals.CallDelayed_Internals(() => {
                    int oldCount = scriptInfo.typeInfos.Count;
                    for (int j = 0; j < oldCount; j++) openScriptCache.typeInfos.Remove(scriptInfo.typeInfos[j].typeFullName);

                    scriptInfo.typeInfos = typeInfos;
                    int newCount = typeInfos.Count;
                    for (int j = 0; j < newCount; j++) openScriptCache.typeInfos[typeInfos[j].typeFullName] = typeInfos[j];

                    onCompleted?.Invoke();
                }, 0.001d);
            });
        }

        static void BuildTypeInfo(SyntaxNode syntaxNode, SyntaxNode parent, string fullName, Action<string, int> onAddTypeInfo)
        {
            if (syntaxNode is NamespaceDeclarationSyntax namespaceSyntax)
            {
                fullName += $"{namespaceSyntax.Name}.";
            }
            else if (syntaxNode is ClassDeclarationSyntax classSyntax)
            {
                if (parent != null) fullName += "+";

                string className = classSyntax.Identifier.Text;
                if (classSyntax.TypeParameterList != null)
                {
                    int typeParameterCount = classSyntax.TypeParameterList.Parameters.Count;
                    if (typeParameterCount > 0) className += $"`{typeParameterCount}";
                }

                fullName += className;

                onAddTypeInfo.Invoke(fullName, GetLineContext(classSyntax));

                parent = classSyntax;
            }
            else if (syntaxNode is StructDeclarationSyntax structSyntax)
            {
                if (parent != null) fullName += "+";

                string structName = structSyntax.Identifier.Text;
                if (structSyntax.TypeParameterList != null)
                {
                    int typeParameterCount = structSyntax.TypeParameterList.Parameters.Count;
                    if (typeParameterCount > 0) structName += $"`{typeParameterCount}";
                }

                fullName += structName;
                onAddTypeInfo.Invoke(fullName, GetLineContext(structSyntax));

                parent = structSyntax;
            }
            else if (syntaxNode is InterfaceDeclarationSyntax interfaceSyntax)
            {
                if (parent != null) fullName += "+";

                string interfaceName = interfaceSyntax.Identifier.Text;
                if (interfaceSyntax.TypeParameterList != null)
                {
                    int typeParameterCount = interfaceSyntax.TypeParameterList.Parameters.Count;
                    if (typeParameterCount > 0) interfaceName += $"`{typeParameterCount}";
                }

                fullName += interfaceName;
                onAddTypeInfo.Invoke(fullName, GetLineContext(interfaceSyntax));

                parent = interfaceSyntax;
            }
            else if (syntaxNode is EnumDeclarationSyntax enumSyntax)
            {
                if (parent != null) fullName += "+";
                fullName += $"{enumSyntax.Identifier.Text}";
                onAddTypeInfo.Invoke(fullName, GetLineContext(enumSyntax));
                return;
            }
            else if (syntaxNode is DelegateDeclarationSyntax delegateSyntax)
            {
                if (parent != null) fullName += "+";

                string delegateName = delegateSyntax.Identifier.Text;
                if (delegateSyntax.TypeParameterList != null)
                {
                    int typeParameterCount = delegateSyntax.TypeParameterList.Parameters.Count;
                    if (typeParameterCount > 0) delegateName += $"`{typeParameterCount}";
                }

                fullName += delegateName;

                onAddTypeInfo.Invoke(fullName, GetLineContext(delegateSyntax));
                return;
            }

            foreach (SyntaxNode child in syntaxNode.ChildNodes()) BuildTypeInfo(child, parent, fullName, onAddTypeInfo);
        }

        static int GetLineContext(CSharpSyntaxNode syntaxNode) => syntaxNode.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
    }
}