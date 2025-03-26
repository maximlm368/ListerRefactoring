using ReactiveUI;

namespace Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;

/// <summary>
/// Represents visible data of printer.
/// </summary>
internal sealed class PrinterPresentation : ReactiveObject
{
    private string _stringPresentation;
    internal string StringPresentation
    {
        get { return _stringPresentation; }
        private set
        {
            this.RaiseAndSetIfChanged ( ref _stringPresentation, value, nameof ( StringPresentation ) );
        }
    }


    internal PrinterPresentation ( string printerName )
    {
        StringPresentation = printerName;
    }
}