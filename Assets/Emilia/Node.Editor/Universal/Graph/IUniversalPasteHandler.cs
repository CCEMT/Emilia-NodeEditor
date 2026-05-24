namespace Emilia.Node.Universal.Editor
{
    /// <summary>
    /// Handles custom paste behavior for a selected universal graph element.
    /// </summary>
    public interface IUniversalPasteHandler
    {
        bool CanPaste();

        bool TryPaste();
    }
}
