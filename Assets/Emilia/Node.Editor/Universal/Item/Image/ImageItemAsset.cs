using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Node.Universal.Editor
{
    [HideMonoScript]
    public class ImageItemAsset : UniversalItemAsset
    {
        [HideInInspector]
        public byte[] imageData;

        [HideInInspector]
        public int imageWidth;

        [HideInInspector]
        public int imageHeight;

        [LabelText("图片名称")]
        public string imageName = "Image";

        [LabelText("背景颜色")]
        public Color backgroundColor = new Color(0.188f, 0.188f, 0.188f, 1f);

        public override string title => string.IsNullOrEmpty(imageName) ? "Image" : imageName;

        public bool hasImage => imageData != null && imageData.Length > 0 && imageWidth > 0 && imageHeight > 0;

        public void SetImage(ImageItemPayload payload)
        {
            imageData = payload.pngData;
            imageWidth = payload.width;
            imageHeight = payload.height;
            imageName = string.IsNullOrEmpty(payload.name) ? "Image" : payload.name;
        }
    }
}
