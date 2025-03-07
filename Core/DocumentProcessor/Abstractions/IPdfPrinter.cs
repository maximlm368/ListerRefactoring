namespace Core.DocumentProcessor.Abstractions;

public interface IPdfPrinter
{
    public void Print(List<Page> printables, IPdfCreator creator, string printerName, int copiesAmount);
}
