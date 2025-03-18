using Avalonia.Threading;
using Core.DocumentProcessor;
using View.DialogMessageWindows.PrintDialog;
using ReactiveUI;

namespace View.CoreModelReflection;

public partial class PdfPrinter : ReactiveObject
{
    private DocumentProcessor _model;
    private PrintAdjustingData _printAdjusting;
    

    private bool _pdfGenerationSucceeded;
    internal bool PdfGenerationSuccesseeded
    {
        get { return _pdfGenerationSucceeded; }
        private set
        {
            if ( _pdfGenerationSucceeded == value )
            {
                _pdfGenerationSucceeded = !_pdfGenerationSucceeded;
            }

            this.RaiseAndSetIfChanged (ref _pdfGenerationSucceeded, value, nameof (PdfGenerationSuccesseeded));
        }
    }

    private bool _prntingEnded;
    internal bool PrintingIsFinished
    {
        get { return _prntingEnded; }
        private set
        {
            if ( _prntingEnded == value )
            {
                _prntingEnded = !_prntingEnded;
            }

            this.RaiseAndSetIfChanged
                (ref _prntingEnded, value, nameof (PrintingIsFinished));
        }
    }


    internal PdfPrinter ( DocumentProcessor model ) 
    {
        _model = model;
    }


    internal void GeneratePdfDuringWaiting ( string fileToSave )
    {
        Task<bool> generationTask = new Task<bool>
        (
            () =>
            {
                return _model.CreatePdfAndSave ( fileToSave );
            }
        );

        generationTask.ContinueWith
        (( tsk ) =>
        {
            PdfGenerationSuccesseeded = tsk.Result;
        });

        generationTask.Start ();
    }


    internal void PrintDuringWaiting ( PrintAdjustingData printAdjusting, string osName )
    {
        _printAdjusting = printAdjusting;

        Task printing = new Task
        ( () =>
        {
            _model.Print ( _printAdjusting.PrinterName, _printAdjusting.PageNumbers, _printAdjusting.CopiesAmount );
        } );

        printing.ContinueWith
        ( ( printingTask ) =>
        {
            Dispatcher.UIThread.Invoke
            ( () =>
            {
                PrintingIsFinished = true;
            } );
        } );

        printing.Start ();
    }
}


