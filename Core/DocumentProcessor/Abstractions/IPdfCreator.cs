namespace Core.DocumentProcessor.Abstractions;

public interface IPdfCreator
{
    public bool CreateAndSave(List<Page> pages, string filePathToSave);

    public IEnumerable<byte[]> Create(List<Page> pages);
}
