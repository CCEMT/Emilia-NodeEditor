#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Emilia.Kit
{
    [Serializable]
    public abstract class ProjectLocalSetting<T> where T : ProjectLocalSetting<T>, new()
    {
        private const string SettingKey = "##Emilia.Kit.ProjectLocalSetting";
        private const string SettingDirectory = "Library/Emilia/LocalSetting";
        private const string InvalidFileNameChars = "<>:\"/\\|?*";
        private const int MaxReadableFileNameLength = 96;
        private const int FileNameHashLength = 12;

        private static readonly UTF8Encoding Utf8WithoutBom = new UTF8Encoding(false, true);

        public static string key => $"{SettingKey}.{typeof(T).FullName}";
        private static string filePath => Path.Combine(EditorAssetKit.dataParentPath, SettingDirectory, GetFileName());

        private static T _instance;

        public static T instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = Load();

                if (_instance != null) return _instance;

                _instance = new T();
                SaveValue(_instance);

                return _instance;
            }
        }

        public static void Save()
        {
            SaveValue(instance);
        }

        private static T Load()
        {
            if (File.Exists(filePath) == false) return null;

            string json = File.ReadAllText(filePath, Utf8WithoutBom);
            T value = OdinSerializableUtility.FromJsonString<T>(json);
            if (value != null) return value;

            string backupPath = BackupCorruptFile();
            Debug.LogWarning($"Failed to deserialize project local setting '{typeof(T).FullName}'. " +
                             $"The invalid file was backed up to '{backupPath}'.");
            return null;
        }

        private static void SaveValue(T value)
        {
            string json = OdinSerializableUtility.ToJsonString(value);
            string directoryPath = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directoryPath) == false) Directory.CreateDirectory(directoryPath);

            string temporaryPath = $"{filePath}.{Guid.NewGuid():N}.tmp";

            try
            {
                File.WriteAllText(temporaryPath, json, Utf8WithoutBom);

                if (File.Exists(filePath)) File.Replace(temporaryPath, filePath, null);
                else File.Move(temporaryPath, filePath);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }

        private static string BackupCorruptFile()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ");
            string backupPath = $"{filePath}.corrupt.{timestamp}";
            int suffix = 1;

            while (File.Exists(backupPath))
            {
                backupPath = $"{filePath}.corrupt.{timestamp}.{suffix}";
                suffix++;
            }

            File.Copy(filePath, backupPath);
            return backupPath;
        }

        private static string GetFileName()
        {
            string typeName = typeof(T).FullName ?? typeof(T).Name;
            StringBuilder builder = new StringBuilder(typeName.Length);

            for (int i = 0; i < typeName.Length; i++)
            {
                char character = typeName[i];
                bool isInvalid = char.IsControl(character) || InvalidFileNameChars.IndexOf(character) >= 0;
                builder.Append(isInvalid ? '_' : character);
            }

            if (builder.Length > MaxReadableFileNameLength) builder.Length = MaxReadableFileNameLength;

            return $"{builder}__{GetTypeHash(typeName)}.json";
        }

        private static string GetTypeHash(string typeName)
        {
            string assemblyName = typeof(T).Assembly.GetName().Name ?? string.Empty;
            byte[] input = Encoding.UTF8.GetBytes($"{assemblyName}:{typeName}");
            byte[] hash;

            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(input);
            }

            StringBuilder builder = new StringBuilder(FileNameHashLength);

            for (int i = 0; i < FileNameHashLength / 2; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
#endif
