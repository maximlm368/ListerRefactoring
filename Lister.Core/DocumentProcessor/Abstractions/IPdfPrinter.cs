namespace Lister.Core.DocumentProcessor.Abstractions;

/// <summary>
/// Defines abstraction implementation of wich is needed to print result badges.
/// </summary>
public interface IPdfPrinter
{
    public void Print(List<Page> printables, IPdfCreator creator, string printerName, int copiesAmount);
}
