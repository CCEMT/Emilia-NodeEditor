using System;

namespace Emilia.Kit.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class OpenAssetProcessorAttribute : Attribute
    {
        public int order { get; }

        public OpenAssetProcessorAttribute(int order = 0)
        {
            this.order = order;
        }
    }

    public interface IOpenAssetProcessor
    {
        bool CanProcess(OpenAssetContext context);
        void Process(OpenAssetContext context);
    }
}