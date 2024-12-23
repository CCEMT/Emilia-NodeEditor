using System;
using System.Collections.Generic;
using System.Reflection;
using Emilia.Kit.Editor;
using Emilia.Node.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalOperateMenuHandle : OperateMenuHandle<EditorUniversalGraphAsset>
    {
        public override void InitializeCache()
        {
            EditorUniversalGraphAsset editorUniversalGraphAsset = smartValue.graphAsset as EditorUniversalGraphAsset;

            smartValue.operateMenu.actionInfoCache.AddRange(OperateMenuActionUtility.GetAction(editorUniversalGraphAsset.operateMenuTags));
            CollectMenuAttribute();
        }

        protected void CollectMenuAttribute()
        {
            Type assetType = smartValue.graphAsset.GetType();
            MethodInfo[] methods = assetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            for (var i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                MenuAttribute menuAttribute = method.GetCustomAttribute<MenuAttribute>();
                if (menuAttribute == null) continue;

                GeneralOperateMenuAction action = new GeneralOperateMenuAction();

                if (string.IsNullOrEmpty(menuAttribute.isOnExpression) == false)
                {
                    action.isOnCallback = () => {
                        ValueResolver<bool> isOnResolver = ValueResolver.Get<bool>(smartValue.graphAsset.propertyTree.RootProperty, menuAttribute.isOnExpression);
                        if (isOnResolver.HasError) Debug.LogError($"Method {assetType.FullName}.{method.Name} has invalid isOnMethod");
                        bool result = isOnResolver.GetValue();
                        return result;
                    };
                }

                if (string.IsNullOrEmpty(menuAttribute.actionValidityMethod) == false) SetValidityCallback(method, menuAttribute, action);

                if (method.GetParameters().Length == 0)
                {
                    action.executeCallback = (_) => ReflectUtility.Invoke(smartValue.graphAsset, method.Name);
                }
                else if (method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(OperateMenuContext))
                {
                    action.executeCallback = (context) => ReflectUtility.Invoke(smartValue.graphAsset, method.Name, new object[] {context});
                }
                else
                {
                    Debug.LogError($"Method {assetType.FullName}.{method.Name} has invalid parameters for menu action");
                }

                OperateMenuActionInfo actionInfo = action.ToActionInfo(menuAttribute.name, menuAttribute.category, menuAttribute.priority);
                smartValue.operateMenu.actionInfoCache.Add(actionInfo);
            }

            void SetValidityCallback(MethodInfo method, MenuAttribute menuAttribute, GeneralOperateMenuAction action)
            {
                MethodInfo validityMethod = assetType.GetMethod(menuAttribute.actionValidityMethod);
                if (validityMethod == null)
                {
                    Debug.LogError($"Method {assetType.FullName}.{method.Name} has invalid {menuAttribute.actionValidityMethod}");
                }
                else
                {
                    if (validityMethod.ReturnType != typeof(OperateMenuActionValidity))
                    {
                        Debug.LogError($"Method {assetType.FullName}.{method.Name} has invalid return type for {menuAttribute.actionValidityMethod}");
                    }

                    if (validityMethod.GetParameters().Length == 0)
                    {
                        action.validityCallback = (_) => (OperateMenuActionValidity) validityMethod.Invoke(smartValue.graphAsset, null);
                    }
                    else if (validityMethod.GetParameters().Length == 1 && validityMethod.GetParameters()[0].ParameterType == typeof(OperateMenuContext))
                    {
                        action.validityCallback = (context) => (OperateMenuActionValidity) validityMethod.Invoke(smartValue.graphAsset, new object[] {context});
                    }
                    else
                    {
                        Debug.LogError($"Method {assetType.FullName}.{method.Name} has invalid parameters for {menuAttribute.actionValidityMethod}");
                    }
                }
            }
        }

        public override void CollectMenuItems(List<OperateMenuItem> menuItems, OperateMenuContext context)
        {
            CollectAction(menuItems, context);
            CollectItemMenu(menuItems, context);
        }

        private void CollectAction(List<OperateMenuItem> menuItems, OperateMenuContext context)
        {
            OperateMenuActionContext actionContext = new OperateMenuActionContext();
            actionContext.graphView = context.graphView;
            actionContext.mousePosition = context.evt.mousePosition;

            int amount = smartValue.operateMenu.actionInfoCache.Count;
            for (int i = 0; i < amount; i++)
            {
                OperateMenuActionInfo item = smartValue.operateMenu.actionInfoCache[i];
                OperateMenuActionValidity validity = item.action.GetValidity(context);

                if (validity == OperateMenuActionValidity.NotApplicable) continue;

                OperateMenuItem menuItem = new OperateMenuItem();
                menuItem.menuName = item.name;
                menuItem.category = item.category;
                menuItem.priority = item.priority;
                menuItem.state = validity;
                menuItem.isOn = item.action.isOn;
                menuItem.onAction = () => item.action.Execute(actionContext);

                menuItems.Add(menuItem);
            }
        }

        private void CollectItemMenu(List<OperateMenuItem> menuItems, OperateMenuContext context)
        {
            List<CreateItemMenuInfo> types = context.graphView.createItemMenu.CollectItemMenus();

            Vector2 mousePosition = context.evt.mousePosition;

            int amount = types.Count;
            for (int i = 0; i < amount; i++)
            {
                CreateItemMenuInfo createItem = types[i];

                OperateMenuItem menuItem = new OperateMenuItem();
                string fullPath = $"CreateItem/{createItem.path}";
                OperateMenuUtility.PathToNameAndCategory(fullPath, out menuItem.menuName, out menuItem.category);

                menuItem.priority = 1000 + i + 1;
                menuItem.onAction = () => context.graphView.itemSystem.CreateItem(createItem.itemAssetType, mousePosition);

                menuItems.Add(menuItem);
            }
        }
    }
}