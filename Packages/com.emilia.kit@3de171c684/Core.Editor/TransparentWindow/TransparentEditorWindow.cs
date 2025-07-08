using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Emilia.Kit.Editor
{
    public class TransparentEditorWindow : EditorWindow
    {
        protected Texture2D backgroundTexture;

        public virtual void OnOpen()
        {
            position = GUIHelper.GetEditorWindowRect();
            ShowPopup();
            Focus();
        }

        protected virtual void OnEnable()
        {
            backgroundTexture = CaptureScreen();
        }

        protected virtual void OnDisable()
        {
            if (this.backgroundTexture == null) return;
            DestroyImmediate(this.backgroundTexture);
            this.backgroundTexture = null;
        }

        protected virtual void OnGUI()
        {
            if (this.backgroundTexture != null)
            {
                Rect windowRect = new Rect(0, 0, position.width, position.height);
                GUI.DrawTexture(windowRect, this.backgroundTexture, ScaleMode.StretchToFill);
            }
        }

        private static Texture2D CaptureScreen()
        {
            Rect mainWindowPosition = GUIHelper.GetEditorWindowRect();
            int width = (int) mainWindowPosition.width;
            int height = (int) mainWindowPosition.height;

            Texture2D captureTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

            Color[] pixels = InternalEditorUtility.ReadScreenPixel(mainWindowPosition.position, width, height);
            captureTexture.SetPixels(pixels);
            captureTexture.Apply();
            return captureTexture;
        }
    }
}