namespace Core.DocumentProcessor.Abstractions;

/// <summary>
/// Defines abstraction implementation of wich is needed to create and save pdf file.
/// </summary>
public interface IPdfCreator
{
    public bool CreateAndSave(List<Page> pages, string filePathToSave);

    public IEnumerable<byte[]> Create(List<Page> pages);
}
