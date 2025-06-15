using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Example.RuntimeNode.Runtime
{
    [RuntimeNodeMenu("TestNode"), Serializable]
    public class TestRuntimeNodeAsset : RuntimeNodeAsset
    {
        public float testFloat = 0.0f;
        public string testString = "Hello, World!";
        public int testInt = 42;

        [Button]
        void Test()
        {
            Debug.Log("Hello, World!");
        }
    }
}