﻿using System;
using System.Collections.Generic;
using System.Linq;
using Emilia.BehaviorTree.Attributes;
using Emilia.Node.Editor;
using Emilia.Variables;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEditor;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public class VariableTypeFilterAttributeDrawer : OdinAttributeDrawer<VariableTypeFilterAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            VariableTypeFilterAttribute attribute = this.Attribute;

            if (typeof(Variable).IsAssignableFrom(this.Property.ValueEntry.TypeOfValue) == false) return;

            EditorGraphView graphView = EditorGraphView.focusedGraphView;
            if (graphView == null) return;

            EditorUniversalGraphAsset universalGraphAsset = graphView.graphAsset as EditorUniversalGraphAsset;
            if (universalGraphAsset == null) return;

            Variable variable = this.Property.ValueEntry.WeakSmartValue as Variable;
            Type type = GetVariableType(universalGraphAsset, attribute);
            if (type == null) return;
            if (type != variable.GetType()) this.Property.ValueEntry.WeakSmartValue = Activator.CreateInstance(type);
            CallNextDrawer(label);
        }

        private Type GetVariableType(EditorUniversalGraphAsset universalGraphAsset, VariableTypeFilterAttribute attribute)
        {
            if (attribute.type != null)
            {
                IList<Type> types = TypeCache.GetTypesDerivedFrom(typeof(Variable<>).MakeGenericType(attribute.type));
                Type variableType = types.FirstOrDefault();
                if (variableType != null) return variableType;
            }

            if (attribute.getTypeExpression == null) return null;

            ValueResolver<Type> typeValueResolver = ValueResolver.Get<Type>(Property.Parent, attribute.getTypeExpression);
            if (typeValueResolver.HasError == false)
            {
                Type type = typeValueResolver.GetValue();
                if (type != null)
                {
                    IList<Type> types = TypeCache.GetTypesDerivedFrom(typeof(Variable<>).MakeGenericType(type));
                    Type variableType = types.FirstOrDefault();
                    if (variableType != null) return variableType;
                }
            }

            ValueResolver<string> keyValueResolver = ValueResolver.Get<string>(Property, attribute.getTypeExpression);
            if (keyValueResolver.HasError) return null;
            string key = keyValueResolver.GetValue();
            if (key == null) return null;
            EditorParameter editorParameter = universalGraphAsset.editorParametersManage.parameters.FirstOrDefault((x) => x.key == key);
            if (editorParameter != null) return editorParameter.value.GetType();

            return null;
        }
    }
}