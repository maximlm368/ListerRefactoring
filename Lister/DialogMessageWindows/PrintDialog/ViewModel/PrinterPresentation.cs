using ReactiveUI;

namespace View.DialogMessageWindows.PrintDialog.ViewModel;


internal class PrinterPresentation : ReactiveObject
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