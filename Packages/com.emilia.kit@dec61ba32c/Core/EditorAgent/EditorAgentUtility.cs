#if UNITY_EDITOR
using UnityEngine;

namespace Emilia.Kit
{
    public static class EditorAgentUtility
    {
        public static T GetAgent<T>() where T : IAgent
        {
            EditorAgent[] editorAgents = Object.FindObjectsByType<EditorAgent>(FindObjectsSortMode.None);

            int amount = editorAgents.Length;
            for (int i = 0; i < amount; i++)
            {
                EditorAgent editorAgent = editorAgents[i];
                T agent = editorAgent.GetAgent<T>();
                if (agent != null) return agent;
            }

            return default(T);
        }
    }
}
#endif