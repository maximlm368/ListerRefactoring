using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Core.BadgesProvider;
using Lister.Views;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Helpers;
using ReactiveUI;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Management;
using System.Runtime.Intrinsics.Arm;

namespace Lister.ViewModels
{
    public partial class PdfPrinter : ReactiveObject
    {
        private ConverterToPdf _converter;
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

                this.RaiseAndSetIfChanged
                    (ref _pdfGenerationSucceeded, value, nameof (PdfGenerationSuccesseeded));
            }
        }

        private bool _prntingEnded;
        internal bool PrintingEnded
        {
            get { return _prntingEnded; }
            private set
            {
                if ( _prntingEnded == value )
                {
                    _prntingEnded = !_prntingEnded;
                }

                this.RaiseAndSetIfChanged
                    (ref _prntingEnded, value, nameof (PrintingEnded));
            }
        }


        public PdfPrinter ( ConverterToPdf converterToPdf ) 
        {
            _converter = converterToPdf;
        }


        private bool GeneratePdf ( string fileToSave, List <PageViewModel> printables )
        {
            IEnumerable<byte []> intermediateBytes = null;

            bool result = _converter.ConvertToExtention (printables, fileToSave, out intermediateBytes);

            return result;
        }


        internal void GeneratePdfDuringWaiting ( string fileToSave, List <PageViewModel> savablePages )
        {
            Task<bool> generationTask = new Task<bool>
            (
                () =>
                {
                    bool isPdfGenerated = GeneratePdf (fileToSave, savablePages);
                    return isPdfGenerated;
                }
            );

            generationTask.ContinueWith
            (( tsk ) =>
            {
                PdfGenerationSuccesseeded = tsk.Result;
            });

            generationTask.Start ();
        }


        internal void PrintDuringWaiting ( List <PageViewModel> printables, PrintAdjustingData printAdjusting, string osName )
        {
            _printAdjusting = printAdjusting;

            Task printing = new Task
            (() =>
            {
                List <PageViewModel> goalPages = new ();

                foreach ( int pageNumber   in   _printAdjusting.PageNumbers )
                {
                    PageViewModel page = printables [pageNumber];
                    goalPages.Add (page);
                }

                if ( goalPages.Count < 1 )
                {
                    return;
                }

                if ( osName == "Windows" )
                {
                    PrintOnWindows (goalPages);
                }
                else if ( osName == "Linux" )
                {
                    PrintOnLinux (goalPages);
                }
            });

            printing.ContinueWith
            (( printingTask ) =>
            {
                Dispatcher.UIThread.Invoke
                (() =>
                {
                    PrintingEnded = true;
                });
            });

            printing.Start ();
        }


        private void PrintOnWindows ( List <PageViewModel> printables )
        {
            IEnumerable<byte []> intermediateBytes = null;

            bool dataIsGenerated = _converter.ConvertToExtention (printables, null, out intermediateBytes);

            if ( dataIsGenerated )
            {
                foreach ( byte [] pageBytes   in   intermediateBytes )
                {
                    using Stream intermediateStream = new MemoryStream (pageBytes);
                    using PrintDocument pd = new System.Drawing.Printing.PrintDocument ();

                    pd.PrinterSettings.PrinterName = _printAdjusting.PrinterName;
                    pd.PrinterSettings.Copies = ( short ) _printAdjusting.CopiesAmount;

                    pd.PrintPage += ( sender, args ) =>
                    {
                        System.Drawing.Image img = System.Drawing.Image.FromStream (intermediateStream);
                        args.Graphics.DrawImage (img, args.Graphics.VisibleClipBounds);
                    };

                    pd.Print ();
                }
            }
        }


        private void PrintOnLinux ( List <PageViewModel> printables )
        {
            string pdfProxyName = @"./proxy.pdf";

            bool dataIsGenerated = GeneratePdf (pdfProxyName, printables);

            if ( dataIsGenerated )
            {
                string printer = _printAdjusting.PrinterName;
                string copies = _printAdjusting.CopiesAmount.ToString ();

                string bashPrintCommand = "lp -d " + printer + " -n " + copies + " " + pdfProxyName;

                ExecuteBashCommand (bashPrintCommand);
            }
        }


        private string ExecuteBashCommand ( string command )
        {
            using ( Process process = new Process () )
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.Start ();

                string result = process.StandardOutput.ReadToEnd ();

                process.WaitForExit ();

                return result;
            }
        }
    }
}