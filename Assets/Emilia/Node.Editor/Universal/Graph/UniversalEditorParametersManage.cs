﻿using System;
using System.Collections.Generic;
using Emilia.Variables;

namespace Emilia.Node.Universal.Editor
{
    public class UniversalEditorParametersManage : EditorParametersManage
    {
        public override IList<Type> filterTypes => new List<Type>() {
            typeof(VariableSingle),
            typeof(VariableInt32),
            typeof(VariableString),
            typeof(VariableVector2),
            typeof(VariableVector3),
            typeof(VariableObject),
        };
    }
}