using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    public static class WindowsClipboardImageUtility
    {
        private const uint CF_DIB = 8;
        private const uint CF_DIBV5 = 17;
        private const uint CF_HDROP = 15;
        private const uint BI_RGB = 0;
        private const uint BI_BITFIELDS = 3;

        public static bool TryCreatePayloadFromFilePaths(string[] paths, out ImageItemPayload payload)
        {
            payload = default;
            if (paths == null) return false;

            foreach (string path in paths)
            {
                if (ImageItemUtility.TryCreatePayloadFromFile(path, out payload)) return true;
            }

            return false;
        }

        public static bool TryCreatePayloadFromDib(byte[] dibData, string imageName, out ImageItemPayload payload)
        {
            payload = default;

#if UNITY_EDITOR_WIN
            if (dibData == null || dibData.Length < 40) return false;

            GCHandle handle = GCHandle.Alloc(dibData, GCHandleType.Pinned);
            try
            {
                return TryReadDib(handle.AddrOfPinnedObject(), dibData.Length, imageName, out payload);
            }
            finally
            {
                handle.Free();
            }
#else
            return false;
#endif
        }

        public static bool TryGetImage(out ImageItemPayload payload)
        {
            payload = default;

#if UNITY_EDITOR_WIN
            if (OpenClipboard(IntPtr.Zero) == false) return false;

            try
            {
                if (TryGetDib(CF_DIBV5, out payload)) return true;
                if (TryGetDib(CF_DIB, out payload)) return true;
                return TryGetFileDropImage(out payload);
            }
            finally
            {
                CloseClipboard();
            }
#else
            return false;
#endif
        }

        public static bool HasImage()
        {
#if UNITY_EDITOR_WIN
            if (IsClipboardFormatAvailable(CF_DIBV5) || IsClipboardFormatAvailable(CF_DIB)) return true;
            return HasSupportedFileDropImage();
#else
            return false;
#endif
        }

#if UNITY_EDITOR_WIN
        private static bool HasSupportedFileDropImage()
        {
            if (OpenClipboard(IntPtr.Zero) == false) return false;

            try
            {
                if (IsClipboardFormatAvailable(CF_HDROP) == false) return false;

                IntPtr handle = GetClipboardData(CF_HDROP);
                if (handle == IntPtr.Zero) return false;

                return TryGetFileDropPaths(handle, path => ImageItemUtility.IsSupportedImagePath(path));
            }
            finally
            {
                CloseClipboard();
            }
        }

        private static bool TryGetFileDropImage(out ImageItemPayload payload)
        {
            payload = default;
            if (IsClipboardFormatAvailable(CF_HDROP) == false) return false;

            IntPtr handle = GetClipboardData(CF_HDROP);
            if (handle == IntPtr.Zero) return false;

            return TryCreatePayloadFromFilePaths(GetFileDropPaths(handle), out payload);
        }

        private static bool TryGetFileDropPaths(IntPtr handle, Func<string, bool> handlePath)
        {
            uint fileCount = DragQueryFile(handle, 0xFFFFFFFF, null, 0);
            for (uint i = 0; i < fileCount; i++)
            {
                uint length = DragQueryFile(handle, i, null, 0);
                if (length == 0) continue;

                System.Text.StringBuilder pathBuilder = new System.Text.StringBuilder((int) length + 1);
                if (DragQueryFile(handle, i, pathBuilder, (uint) pathBuilder.Capacity) == 0) continue;
                if (handlePath(pathBuilder.ToString())) return true;
            }

            return false;
        }

        private static string[] GetFileDropPaths(IntPtr handle)
        {
            List<string> paths = new List<string>();
            uint fileCount = DragQueryFile(handle, 0xFFFFFFFF, null, 0);
            for (uint i = 0; i < fileCount; i++)
            {
                uint length = DragQueryFile(handle, i, null, 0);
                if (length == 0) continue;

                System.Text.StringBuilder pathBuilder = new System.Text.StringBuilder((int) length + 1);
                if (DragQueryFile(handle, i, pathBuilder, (uint) pathBuilder.Capacity) == 0) continue;
                paths.Add(pathBuilder.ToString());
            }

            return paths.ToArray();
        }

        private static bool TryGetDib(uint format, out ImageItemPayload payload)
        {
            payload = default;
            if (IsClipboardFormatAvailable(format) == false) return false;

            IntPtr handle = GetClipboardData(format);
            if (handle == IntPtr.Zero) return false;

            IntPtr data = GlobalLock(handle);
            if (data == IntPtr.Zero) return false;

            try
            {
                int dataLength = GetGlobalSize(handle);
                return TryReadDib(data, dataLength, "Clipboard Image", out payload);
            }
            finally
            {
                GlobalUnlock(handle);
            }
        }

        private static bool TryReadDib(IntPtr data, int dataLength, string imageName, out ImageItemPayload payload)
        {
            payload = default;

            if (HasBytes(dataLength, 0, 40) == false) return false;

            int headerSize = Marshal.ReadInt32(data, 0);
            if (headerSize < 40) return false;
            if (HasBytes(dataLength, 0, headerSize) == false) return false;

            int width = Marshal.ReadInt32(data, 4);
            int signedHeight = Marshal.ReadInt32(data, 8);
            short planes = Marshal.ReadInt16(data, 12);
            short bitCount = Marshal.ReadInt16(data, 14);
            uint compression = (uint) Marshal.ReadInt32(data, 16);

            if (planes != 1 || width <= 0 || signedHeight == 0) return false;
            if (compression != BI_RGB && compression != BI_BITFIELDS) return false;
            if (bitCount != 24 && bitCount != 32) return false;

            int height = Math.Abs(signedHeight);
            bool bottomUp = signedHeight > 0;
            int bytesPerPixel = bitCount / 8;
            int stride = ((width * bitCount + 31) / 32) * 4;
            int pixelOffset = headerSize;
            if (compression == BI_BITFIELDS && headerSize == 40) pixelOffset += bitCount == 32 ? 16 : 12;
            long requiredBytes = (long) pixelOffset + (long) stride * height;
            if (dataLength >= 0 && requiredBytes > dataLength) return false;
            IntPtr pixelData = IntPtr.Add(data, pixelOffset);

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                Color32[] pixels = new Color32[width * height];

                bool hasAlpha = false;
                for (int y = 0; y < height; y++)
                {
                    int sourceY = bottomUp ? y : height - 1 - y;
                    int rowOffset = sourceY * stride;

                    for (int x = 0; x < width; x++)
                    {
                        int sourceOffset = rowOffset + x * bytesPerPixel;
                        byte b = Marshal.ReadByte(pixelData, sourceOffset);
                        byte g = Marshal.ReadByte(pixelData, sourceOffset + 1);
                        byte r = Marshal.ReadByte(pixelData, sourceOffset + 2);
                        byte a = bytesPerPixel == 4 ? Marshal.ReadByte(pixelData, sourceOffset + 3) : (byte) 255;
                        if (a != 0) hasAlpha = true;

                        pixels[y * width + x] = new Color32(r, g, b, a);
                    }
                }

                if (bytesPerPixel == 4 && hasAlpha == false)
                {
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        Color32 pixel = pixels[i];
                        pixel.a = 255;
                        pixels[i] = pixel;
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply();
                payload = ImageItemUtility.CreatePayload(texture, imageName);
                return payload.isValid;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static bool HasBytes(int dataLength, int offset, int count)
        {
            if (dataLength < 0) return true;
            if (offset < 0 || count < 0) return false;
            return offset <= dataLength && count <= dataLength - offset;
        }

        private static int GetGlobalSize(IntPtr handle)
        {
            UIntPtr size = GlobalSize(handle);
            ulong length = size.ToUInt64();
            if (length == 0 || length > int.MaxValue) return -1;
            return (int) length;
        }

        [DllImport("user32.dll")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern UIntPtr GlobalSize(IntPtr hMem);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern uint DragQueryFile(IntPtr hDrop, uint iFile, System.Text.StringBuilder lpszFile, uint cch);
#endif
    }
}
