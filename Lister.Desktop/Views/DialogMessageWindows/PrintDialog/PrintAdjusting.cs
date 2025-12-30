namespace Lister.Desktop.Views.DialogMessageWindows.PrintDialog;

/// <summary>
/// Represents information that is got from dialog for printing.
/// </summary>
public sealed class PrintAdjustingData
{
    public string? PrinterName { get; set; }
    public List<int>? PageNumbers { get; set; }
    public int CopiesAmount { get; set; } = 1;
    public bool IsCancelled { get; set; }
}