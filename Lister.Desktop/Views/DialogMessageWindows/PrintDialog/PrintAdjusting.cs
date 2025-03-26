namespace Lister.Desktop.Views.DialogMessageWindows.PrintDialog;

/// <summary>
/// Represents information got from dialog for printing.
/// </summary>
public sealed class PrintAdjustingData
{
    public string PrinterName { get; set; }
    public List<int> PageNumbers { get; set; }
    public int CopiesAmount { get; set; }
    public bool Cancelled { get; set; }
}