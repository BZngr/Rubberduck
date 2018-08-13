using Rubberduck.VBEditor.SafeComWrappers.Abstract;

namespace Rubberduck.VBEditor.SourceCodeHandling
{
    public interface ITempSourceFileHandler
    {
        string Export(IVBComponent component);
        void ImportAndCleanUp(IVBComponent component, string fileName);

        string Read(IVBComponent component);
    }
}
