using System;
using System.Reflection;

namespace Emilia.Node.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class VariableKeySelectorAttribute : Attribute
    {
        public static string GetDescription(string key)
        {
#if UNITY_EDITOR
            return (string) Assembly.Load("Emilia.Node.Universal.Editor").GetType("Emilia.Node.Universal.Editor.VariableKeyUtility").GetMethod("GetDescription").Invoke(null, new object[] {key});
#else
            return "";
#endif
        }
    }
}