using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Document;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.Scene;

/// <summary>
/// Starts creation and printing asynchcronously while some waiting view is visible in graphic thread.
/// </summary>
public sealed partial class Printer : ObservableObject
{
    private readonly DocumentProcessor _model;

    private bool _pdfGenerationSucceeded;
    internal bool PdfGenerationSuccesseeded
    {
        get 
        { 
            return _pdfGenerationSucceeded; 
        }

        private set
        {
            _pdfGenerationSucceeded = value;
            OnPropertyChanged ();
        }
    }

    private bool _printingIsFinished;
    internal bool PrintingIsFinished
    {
        get 
        { 
            return _printingIsFinished; 
        }

        private set
        {
            _printingIsFinished = value;
            OnPropertyChanged ();
        }
    }


    internal Printer ( DocumentProcessor model )
    {
        _model = model;
    }

    internal void GeneratePdfDuringWaiting ( string fileToSave )
    {
        Task<bool> generationTask = new (
            () =>
            {
                return _model.CreateAndSavePdf ( fileToSave );
            }
        );

        generationTask.ContinueWith ( 
            ( tsk ) =>
            {
                PdfGenerationSuccesseeded = tsk.Result;
            } 
        );

        generationTask.Start ();
    }

    internal void PrintDuringWaiting ( PrintAdjustingData printAdjusting )
    {
        Task printing = new (   
            () =>
            {
                _model.Print ( printAdjusting.PrinterName, printAdjusting.PageNumbers, printAdjusting.CopiesAmount );
            } 
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
