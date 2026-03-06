using CommunityToolkit.Mvvm.ComponentModel;

namespace Lister.Desktop.Windows.DialogMessageWindows.PrintDialog.ViewModel;

/// <summary>
/// Represents visible data of printer.
/// </summary>
public sealed class PrinterPresentation : ObservableObject
{
    private string _stringPresentation = string.Empty;
    public string StringPresentation
    {
        get =>_stringPresentation;
        
        private set
        {
            _stringPresentation = value;
            OnPropertyChanged ();
        }
    }

    public PrinterPresentation ( string printerName )
    {
        StringPresentation = printerName;
    }
}