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
using ContentAssembler;
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
    public partial class SceneViewModel : ViewModelBase
    {
        private static readonly double _buttonWidth = 32;
        private static readonly double _countLabelWidth = 50;
        private static readonly double _extention = 170;
        private static readonly double _buttonBlockWidth = 50;
        private static readonly double _workAreaLeftMargin = -61;
        private static readonly string _extentionToolTip = "Развернуть панель";
        private static readonly string _shrinkingToolTip = "Свернуть панель";
        private static readonly string _saveTitle = "Сохранение документа";
        private static readonly string _suggestedFileNames = "Badge";
        private static readonly string _fileIsOpenMessage = "Файл открыт в другом приложении, закройте его.";
        public static int TappedPdfGenerationButton { get; set; }
        public static int TappedPrintButton { get; set; }

        private PrintAdjustingData _printAdjusting;
        private bool _blockIsExtended = false;
        private string _pdfFileName;

        private double _hintWidth;
        internal double HintWidth
        {
            get { return _hintWidth; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _hintWidth, value, nameof (HintWidth));
            }
        }

        private double _butonWidth;
        internal double ButtonWidth
        {
            get { return _butonWidth; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _butonWidth, value, nameof (ButtonWidth));
            }
        }

        private double _buttonBlockWidht;
        internal double ButtonBlockWidth
        {
            get { return _buttonBlockWidht; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _buttonBlockWidht, value, nameof (ButtonBlockWidth));
            }
        }

        private Thickness _workAreaMargin;
        internal Thickness WorkAreaMargin
        {
            get { return _workAreaMargin; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _workAreaMargin, value, nameof (WorkAreaMargin));
            }
        }

        private string _extentionTip;
        internal string ExtentionTip
        {
            get { return _extentionTip; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _extentionTip, value, nameof (ExtentionTip));
            }
        }

        private string _extenderContent;
        internal string ExtenderContent
        {
            get { return _extenderContent; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _extenderContent, value, nameof (ExtenderContent));
            }
        }


        private void SetButtonBlock ( )
        {
            ButtonWidth = _buttonWidth;
            HintWidth = 0;
            ButtonBlockWidth = _buttonBlockWidth;
            WorkAreaMargin = new Thickness (_workAreaLeftMargin, 0);
            ExtentionTip = _extentionToolTip;
            ExtenderContent = "\uF061";
        }


        internal void ExtendButtons ()
        {
            if ( _blockIsExtended )
            {
                HintWidth = 0;
                ButtonWidth -= _extention;
                ButtonBlockWidth -= _extention;
                WorkAreaMargin = new Thickness (( WorkAreaMargin. Left + _extention ), 0);
                ExtentionTip = _extentionToolTip;
                ExtenderContent = "\uF061";
                _blockIsExtended = false;
            }
            else 
            {
                HintWidth = _extention;
                ButtonWidth += _extention;
                ButtonBlockWidth += _extention;
                WorkAreaMargin = new Thickness (( WorkAreaMargin. Left - _extention ), 0);
                ExtentionTip = _shrinkingToolTip;
                ExtenderContent = "\uF060";
                _blockIsExtended = true;
            }
        }


        internal void GeneratePdf ()
        {
            List <FilePickerFileType> fileExtentions = [];
            fileExtentions.Add (FilePickerFileTypes.Pdf);
            FilePickerSaveOptions options = new ();
            options.Title = _saveTitle;
            options.FileTypeChoices = new ReadOnlyCollection <FilePickerFileType> (fileExtentions);
            options.SuggestedFileName = _suggestedFileNames + GenerateNowDateString ();
            Task <IStorageFile> chosenFile = MainWindow.CommonStorageProvider.SaveFilePickerAsync (options);

            TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

            chosenFile.ContinueWith
                (
                   task =>
                   {
                       if ( task.Result != null )
                       {
                           _pdfFileName = task.Result.Path.ToString ();
                           int uriTypeLength = App.ResourceUriType. Length;
                           _pdfFileName = _pdfFileName.Substring (uriTypeLength, _pdfFileName.Length - uriTypeLength);
                           ModernMainViewModel mainViewModel = App.services.GetRequiredService<ModernMainViewModel> ();

                           TappedPdfGenerationButton = 1;

                           mainViewModel.SetWaiting ();
                       }
                   }, uiScheduler
                );
        }


        private string ExtractPathWithoutFileName ( string wholePath )
        {
            var builder = new StringBuilder ();
            string goalPath = string.Empty;

            for ( var index = wholePath.Length - 1; index >= 0; index-- )
            {
                bool fileNameIsAchieved = ( wholePath [index] == '/' ) || ( wholePath [index] == '\\' );

                if ( fileNameIsAchieved )
                {
                    goalPath = wholePath.Substring (0, index);
                    break;
                }
            }

            return goalPath;
        }


        private string GenerateNowDateString ()
        {
            DateTime now = DateTime.Now;

            string day = now.Day.ToString ();

            if ( Int32.Parse (day) < 10 )
            {
                day = "0" + day;
            }

            string month = now.Month.ToString ();

            if ( Int32.Parse (month) < 10 )
            {
                month = "0" + month;
            }

            string hour = now.Hour.ToString ();

            if ( Int32.Parse (hour) < 10 )
            {
                hour = "0" + hour;
            }

            string minute = now.Minute.ToString ();

            if ( Int32.Parse (minute) < 10 )
            {
                minute = "0" + minute;
            }

            string result = "_" + day + month + now.Year.ToString () + "_" + hour + minute;
            return result;
        }


        private bool GeneratePdf ( string fileToSave, List <PageViewModel> printables )
        {
            IEnumerable <byte []> intermediateBytes = null;

            Stopwatch sw = Stopwatch.StartNew ();
            
            bool result = _converter.ConvertToExtention (printables, fileToSave, out intermediateBytes);
            
            sw.Stop ();

            long spendTime = sw.ElapsedMilliseconds;
            
            return result;
        }


        internal void GeneratePdfDuringWaiting ( )
        {
            TappedPdfGenerationButton = 0;
            List <PageViewModel> pages = GetPrintablePages ();

            Task <bool> generationTask = new Task <bool>
            (
                () => 
                {
                    bool isPdfGenerated = GeneratePdf (_pdfFileName, pages);
                    return isPdfGenerated;
                }
            );

            generationTask.ContinueWith
                (( tsk ) =>
                {
                    if ( tsk.Result == false )
                    {
                        Dispatcher.UIThread.InvokeAsync
                        (() =>
                        {
                            ModernMainViewModel modernMV = App.services.GetRequiredService<ModernMainViewModel> ();
                            modernMV.EndWaitingPdfOrPrint ();

                            var messegeDialog = new MessageDialog (ModernMainView.Instance, _fileIsOpenMessage);
                            WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
                            waitingVM.HandleDialogOpenig ();
                            messegeDialog.ShowDialog (MainWindow.Window);
                        });
                    }
                    else
                    {
                        Dispatcher.UIThread.InvokeAsync
                        (() => 
                        {
                            ModernMainViewModel modernMV = App.services.GetRequiredService<ModernMainViewModel> ();
                            modernMV.EndWaitingPdfOrPrint ();
                        });

                        if ( App.OsName == "Windows" )
                        {
                            Process fileExplorer = new Process ();
                            fileExplorer.StartInfo.FileName = "explorer.exe";
                            _pdfFileName = ExtractPathWithoutFileName (_pdfFileName);
                            _pdfFileName = _pdfFileName.Replace ('/', '\\');
                            fileExplorer.StartInfo.Arguments = _pdfFileName;
                            fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                            fileExplorer.Start ();
                        }
                        else if ( App.OsName == "Linux" )
                        {
                            Process fileExplorer = new Process ();
                            fileExplorer.StartInfo.FileName = "nautilus";
                            _pdfFileName = ExtractPathWithoutFileName (_pdfFileName);
                            fileExplorer.StartInfo.Arguments = _pdfFileName;
                            fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                            fileExplorer.Start ();
                        }
                    }
                });

            generationTask.Start ();
        }


        public void Print ()
        {
            int pagesCount = GetPrintablePagesCount ();

            _printAdjusting = new PrintAdjustingData ();

            PrintDialog printDialog = new PrintDialog (pagesCount, _printAdjusting);

            printDialog.Closed += ( sender, args ) =>
            {
                if ( _printAdjusting.Cancelled )
                {
                    return;
                }

                WaitForPrinting ();
            };

            HandleDialogOpenig ();
            printDialog.ShowDialog (MainWindow.Window);
        }


        internal void HandleDialogOpenig ()
        {
            WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
            waitingVM.HandleDialogOpenig ();
        }


        internal void HandleDialogClosing ()
        {
            WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
            waitingVM.HandleDialogClosing ();
        }


        public void WaitForPrinting ()
        {
            ModernMainViewModel mainViewModel = App.services.GetRequiredService<ModernMainViewModel> ();
            TappedPrintButton = 1;
            mainViewModel.SetWaiting ();
        }


        public void PrintDuringWaiting ()
        {
            TappedPrintButton = 0;

            Task printing = new Task
            (() =>
            {
                List <PageViewModel> pages = GetPrintablePages ();
                List <PageViewModel> goalPages = new ();

                foreach ( int pageNumber   in   _printAdjusting.PageNumbers )
                {
                    PageViewModel page = pages [pageNumber];
                    goalPages.Add (page);
                }

                if ( goalPages.Count < 1 ) 
                {
                    return;
                }

                if ( App.OsName == "Windows" )
                {
                    PrintOnWindows (goalPages);
                }
                else if ( App.OsName == "Linux" ) 
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
                    ModernMainViewModel modernMV = App.services.GetRequiredService<ModernMainViewModel> ();
                    modernMV.EndWaitingPdfOrPrint ();
                });
            });

            printing.Start ();
        }


        private void PrintOnWindows ( List <PageViewModel> printables )
        {
            IEnumerable <byte []> intermediateBytes = null;

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

                    //pd.Print ();
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

                App.ExecuteBashCommand (bashPrintCommand);
            }
        }


        //private void ExecuteBashCommand ( string command )
        //{
        //    using ( Process process = new Process () )
        //    {
        //        process.StartInfo = new ProcessStartInfo
        //        {
        //            FileName = "/bin/bash",
        //            Arguments = $"-c \"{command}\"",
        //            RedirectStandardOutput = true,
        //            UseShellExecute = false,
        //            CreateNoWindow = true
        //        };

        //        process.Start ();

        //        //string result = process.StandardOutput.ReadToEnd ();

        //        process.WaitForExit ();
        //    }
        //}
    }
}


//var printerQuery = new ManagementObjectSearcher ("SELECT * from Win32_Printer");

//var printers = printerQuery.Get ();
//ManagementBaseObject goalPrinter = null;
//int counter = 0;

//foreach ( var printer in printers )
//{
//    if ( counter == 6 )
//    {
//        goalPrinter = printer;
//    }

//    var name = printer.GetPropertyValue ("Name");
//    var status = printer.GetPropertyValue ("Status");
//    var isDefault = printer.GetPropertyValue ("Default");
//    var isNetworkPrinter = printer.GetPropertyValue ("Network");

//    //Console.WriteLine ("{0} (Status: {1}, Default: {2}, Network: {3}",
//    //            name, status, isDefault, isNetworkPrinter);

//    int dfd = 0;
//    counter++;
//}



//int length = _converter.intermidiateFiles.Count;
//IStorageItem sItem = null;
////string printerName = "ORPO-7.mgkb.ru\Samsung ML-2160 Series";
////string printerName = "ORPO-7.mgkb.ru\Samsung ML-2160 Series";
//string printerName = "Samsung ML-2160 Seriesqqqqq";

//ProcessStartInfo info = new ()
//{
//    FileName = fileToSave,
//    Verb = "Print",
//    UseShellExecute = true,
//    ErrorDialog = false,
//    CreateNoWindow = true,
//   // Arguments = "\"" + printerName + "\"",
//    WindowStyle = ProcessWindowStyle.Minimized
//};

//bool ? procIsExited = Process.Start (info)?.WaitForExit (20_000);                


//ModernMainViewModel mainViewModel = App.services.GetRequiredService<ModernMainViewModel> ();
//TappedPrintButton = 1;
//mainViewModel.SetWaitingPdfOrPrint ();