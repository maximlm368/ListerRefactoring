using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Document;
using Lister.Core.Document.AbstractServices;
using Lister.Desktop.ExecutersForCoreAbstractions.DocumentProcessor;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.Scene;

/// <summary>
/// Starts creation and printing asynchcronously while some waiting view is visible in graphic thread.
/// </summary>
public sealed partial class DocumentOutsider : ObservableObject
{
    private readonly DocumentProcessor _model;
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


    internal DocumentOutsider ( DocumentProcessor model )
    {
        _model = model;
    }

    internal void GeneratePdfDuringWaiting ( string fileToSave, List<Page> creatables )
    {
        //Task<bool> generationTask = new ( () => _model.CreateAndSavePdf ( fileToSave ) );

        Task<bool> generationTask = new ( () => _pdfCreator.CreateAndSave ( creatables, fileToSave ) );
        generationTask.ContinueWith ( ( tsk ) => PdfGenerationSuccesseeded = tsk.Result );
        generationTask.Start ();
    }

    internal void PrintDuringWaiting ( PrintAdjustingData printAdjusting, List<Page> printables )
    {
        //Task printing = new ( () => _model.Print ( printAdjusting.PrinterName, printAdjusting.PageNumbers, printAdjusting.CopiesAmount ) );

        Task printing = new ( () => _printer.Print ( printables, _pdfCreator, printAdjusting.PrinterName, printAdjusting.CopiesAmount ) );

        printing.ContinueWith (
            ( printingTask ) => {
                Dispatcher.UIThread.Invoke (
                    () => {
                        PrintingIsFinished = true;
                    }
                );
            }
        );

        printing.Start ();
    }
}
