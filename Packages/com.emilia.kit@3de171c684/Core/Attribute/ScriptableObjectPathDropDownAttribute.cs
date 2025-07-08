using System;
using System.Diagnostics;

namespace Emilia.Kit
{
    [Conditional("UNITY_EDITOR"), AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ScriptableObjectPathDropDownAttribute : Attribute
    {
        private Type _type;

        public Type type => this._type;

        public ScriptableObjectPathDropDownAttribute(Type type)
        {
            this._type = type;
        }
    }
}