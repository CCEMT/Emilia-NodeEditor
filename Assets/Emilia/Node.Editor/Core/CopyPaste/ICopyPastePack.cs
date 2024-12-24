namespace Emilia.Node.Editor
{
    public interface ICopyPastePack
    {
        bool CanDependency(ICopyPastePack pack);

        void Paste(CopyPasteContext copyPasteContext);
    }
}