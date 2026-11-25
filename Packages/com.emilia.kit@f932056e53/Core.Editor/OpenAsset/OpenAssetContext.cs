using System;
using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Emilia.Kit.Editor
{
    public sealed class OpenAssetOptions
    {
        private readonly Dictionary<string, object> _parameters = new();

        public IReadOnlyDictionary<string, object> parameters => this._parameters;

        public OpenAssetOptions SetParameter(string key, object value)
        {
            if (string.IsNullOrEmpty(key)) return this;

            this._parameters[key] = value;
            return this;
        }

        public bool TryGetParameter<T>(string key, out T value) => OpenAssetContext.TryGetParameter(this._parameters, key, out value);
    }

    public sealed class OpenAssetContext
    {
        private readonly Dictionary<string, object> _parameters;

        public Object source { get; private set; }
        public Object asset { get; private set; }
        public string assetPath { get; private set; }
        public AssetImporter assetImporter { get; private set; }
        public int instanceId { get; private set; }
        public int lineNumber { get; private set; }
        public IReadOnlyDictionary<string, object> parameters => this._parameters;

        public OpenAssetContext(
            Object source,
            Object asset,
            string assetPath,
            AssetImporter assetImporter,
            int instanceId,
            int lineNumber,
            IReadOnlyDictionary<string, object> parameters = null)
        {
            this.source = source;
            this.asset = asset;
            this.assetPath = assetPath ?? string.Empty;
            this.assetImporter = assetImporter;
            this.instanceId = instanceId;
            this.lineNumber = lineNumber;
            this._parameters = parameters == null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(parameters);
        }

        public bool HasParameter(string key) => string.IsNullOrEmpty(key) == false && this._parameters.ContainsKey(key);

        public bool TryGetParameter<T>(string key, out T value) => TryGetParameter(this._parameters, key, out value);

        public static bool TryGetParameter<T>(IReadOnlyDictionary<string, object> parameters, string key, out T value)
        {
            value = default;
            if (parameters == null || string.IsNullOrEmpty(key)) return false;
            if (parameters.TryGetValue(key, out object rawValue) == false) return false;

            if (rawValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            if (rawValue == null)
            {
                Type targetType = typeof(T);
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null) return false;

                value = default;
                return true;
            }

            return false;
        }
    }
}