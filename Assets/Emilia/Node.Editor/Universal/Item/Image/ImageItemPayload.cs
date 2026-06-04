namespace Emilia.Node.Universal.Editor
{
    public readonly struct ImageItemPayload
    {
        public readonly byte[] pngData;
        public readonly int width;
        public readonly int height;
        public readonly string name;

        public ImageItemPayload(byte[] pngData, int width, int height, string name)
        {
            this.pngData = pngData;
            this.width = width;
            this.height = height;
            this.name = name;
        }

        public bool isValid => pngData != null && pngData.Length > 0 && width > 0 && height > 0;
    }
}
