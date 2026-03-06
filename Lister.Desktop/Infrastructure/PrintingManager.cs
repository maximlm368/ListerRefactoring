using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Entities;
using Lister.Desktop.Windows.DialogMessageWindows.PrintDialog;

namespace Lister.Desktop.Infrastructure;

/// <summary>
/// Starts creation and printing asynchcronously while some waiting view is visible in graphic thread.
/// </summary>
public sealed partial class PrintingManager : ObservableObject
{
    private readonly PdfCreator _pdfCreator;
    private readonly Printer _printer;

    private bool _pdfGenerationSucceeded;
    internal bool PdfGenerationSuccesseeded
    {
        get => _pdfGenerationSucceeded;

        private set
        {
            _pdfGenerationSucceeded = value;
            OnPropertyChanged ();
        }
    }

    private bool _printingIsFinished;
    internal bool PrintingIsFinished
    {
        get => _printingIsFinished;

        private set
        {
            _printingIsFinished = value;
            OnPropertyChanged ();
        }
    }

    internal PrintingManager ( PdfCreator pdfCreator, Printer printer )
    {
        _pdfCreator = pdfCreator;
        _printer = printer;
    }

    internal void ActivatePdfCreation ( string fileToSave, List<Page> creatables )
    {
        Task<bool> pdfGeneration = new (
            () => _pdfCreator.CreateAndSave ( creatables, fileToSave )
        );

        pdfGeneration.ContinueWith ( ( tsk ) =>
            PdfGenerationSuccesseeded = tsk.Result
        );

        pdfGeneration.Start ();
    }

    internal void ActivatePrinting ( PrintAdjustingData printAdjusting, List<Page> printables )
    {
        if ( printAdjusting == null || string.IsNullOrWhiteSpace ( printAdjusting.PrinterName ) )
        {
            return;
        }

        Task printing = new (
            () => _printer.Print ( printables, _pdfCreator, printAdjusting.PrinterName, printAdjusting.CopiesAmount )
        );

        printing.ContinueWith (
            ( printingTask ) =>
            {
                Dispatcher.UIThread.Invoke (
                    () =>
                    {
                        PrintingIsFinished = true;
                    }
                );
            }
        );

        printing.Start ();
    }
}
