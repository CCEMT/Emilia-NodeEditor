using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Kit
{
    public static class JsonSerializableUtility
    {
        public static string ToJson<T>(T target)
        {
            var serializablePack = new SerializablePack<T>();
            serializablePack.serializableObject = target;
            return JsonUtility.ToJson(serializablePack);
        }

        public static T FromJson<T>(string json)
        {
            try
            {
                var serializablePack = JsonUtility.FromJson<SerializablePack<T>>(json);
                return serializablePack.serializableObject;
            }
            catch
            {
                return default;
            }
        }
    }

    [Serializable]
    public class SerializablePack<T> : ISerializationCallbackReceiver
    {
        [SerializeField]
        private byte[] objectInfoBytes;

        [SerializeField]
        private List<Object> unityObjects = new List<Object>();

        [NonSerialized]
        public T serializableObject;

        public void OnBeforeSerialize()
        {
            if (this.serializableObject == null) return;
            unityObjects.Clear();
            objectInfoBytes = TagSerializationUtility.IgnoreTagSerializeValue(serializableObject, DataFormat.Binary, out unityObjects, SerializeTagDefine.DefaultIgnoreTag);
        }

        public void OnAfterDeserialize()
        {
            serializableObject = SerializationUtility.DeserializeValue<T>(objectInfoBytes, DataFormat.Binary, unityObjects);
        }
    }
}