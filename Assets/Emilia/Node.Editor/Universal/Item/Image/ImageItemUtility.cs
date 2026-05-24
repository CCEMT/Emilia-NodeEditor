using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Emilia.Node.Universal.Editor
{
    public static class ImageItemUtility
    {
        public const float MaxInitialWidth = 420f;
        public const float MaxInitialHeight = 320f;
        public const float MinInitialWidth = 120f;
        public const float MinInitialHeight = 80f;

        public static bool TryCreatePayloadFromDrag(out ImageItemPayload payload)
        {
            foreach (Object objectReference in DragAndDrop.objectReferences)
            {
                Texture2D texture = objectReference as Texture2D;
                if (texture == null) continue;
                payload = CreatePayload(texture, texture.name);
                return payload.isValid;
            }

            foreach (string path in DragAndDrop.paths)
            {
                if (TryCreatePayloadFromFile(path, out payload)) return true;
            }

            payload = default;
            return false;
        }

        public static bool CanCreatePayloadFromDrag()
        {
            if (DragAndDrop.objectReferences != null)
            {
                foreach (Object objectReference in DragAndDrop.objectReferences)
                {
                    if (objectReference is Texture2D) return true;
                }
            }

            if (DragAndDrop.paths != null)
            {
                foreach (string path in DragAndDrop.paths)
                {
                    if (IsSupportedImagePath(path)) return true;
                }
            }

            return false;
        }

        public static bool TryCreatePayloadFromFile(string path, out ImageItemPayload payload)
        {
            payload = default;
            if (IsSupportedImagePath(path) == false) return false;
            if (File.Exists(path) == false) return false;

            byte[] data = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (texture.LoadImage(data) == false) return false;
                payload = CreatePayload(texture, Path.GetFileNameWithoutExtension(path));
                return payload.isValid;
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        public static ImageItemPayload CreatePayload(Texture2D texture, string imageName)
        {
            if (texture == null) return default;

            Texture2D readableTexture = CreateReadableCopy(texture);
            try
            {
                byte[] pngData = readableTexture.EncodeToPNG();
                return new ImageItemPayload(pngData, texture.width, texture.height, string.IsNullOrEmpty(imageName) ? "Image" : imageName);
            }
            finally
            {
                if (readableTexture != texture) Object.DestroyImmediate(readableTexture);
            }
        }

        public static Texture2D CreateTexture(byte[] pngData)
        {
            if (pngData == null || pngData.Length == 0) return null;

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (texture.LoadImage(pngData)) return texture;

            Object.DestroyImmediate(texture);
            return null;
        }

        public static Vector2 CalculateInitialSize(int imageWidth, int imageHeight)
        {
            if (imageWidth <= 0 || imageHeight <= 0) return new Vector2(220f, 140f);

            float scale = Math.Min(MaxInitialWidth / imageWidth, MaxInitialHeight / imageHeight);
            scale = Math.Min(1f, scale);

            Vector2 size = new Vector2(imageWidth * scale, imageHeight * scale);
            if (size.x < MinInitialWidth || size.y < MinInitialHeight)
            {
                float minScale = Math.Min(MinInitialWidth / size.x, MinInitialHeight / size.y);
                size *= minScale;
            }

            return new Vector2((float) Math.Round(size.x), (float) Math.Round(size.y));
        }

        public static bool IsSupportedImagePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            string extension = Path.GetExtension(path);
            return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase);
        }

        private static Texture2D CreateReadableCopy(Texture2D source)
        {
            try
            {
                source.GetPixel(0, 0);
                return source;
            }
            catch
            {
                RenderTexture renderTexture = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
                RenderTexture previous = RenderTexture.active;

                try
                {
                    Graphics.Blit(source, renderTexture);
                    RenderTexture.active = renderTexture;

                    Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                    readableTexture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                    readableTexture.Apply();
                    return readableTexture;
                }
                finally
                {
                    RenderTexture.active = previous;
                    RenderTexture.ReleaseTemporary(renderTexture);
                }
            }
        }
    }
}
