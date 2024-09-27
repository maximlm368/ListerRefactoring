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
using System.Windows.Forms;
using System.Management;

namespace Lister.ViewModels
{
    public partial class SceneViewModel : ViewModelBase
    {
        private static readonly double _buttonWidth = 32;
        private static readonly double _countLabelWidth = 50;
        private static readonly double _extention = 130;
        private static readonly double _buttonBlockWidth = 50;
        private static readonly double _workAreaLeftMargin = -61;
        private static readonly string _extentionToolTip = "Развернуть";
        private static readonly string _shrinkingToolTip = "Свернуть";
        private static readonly string _saveTitle = "Сохранение документа";
        private static readonly string _suggestedFileNames = "Badge";
        private static readonly string _fileIsOpenMessage = "Файл открыт в другом приложении, закройте его.";
        public static int TappedPdfGenerationButton { get; set; }
        public static int TappedPrintButton { get; set; }

        private bool _blockIsExtended = false;
        private string _pdfFileName;
        private bool _isPdfGenerated;

        private double cW;
        internal double CountWidth
        {
            get { return cW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cW, value, nameof (CountWidth));
            }
        }

        private double bW;
        internal double ButtonWidth
        {
            get { return bW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bW, value, nameof (ButtonWidth));
            }
        }

        private double bBW;
        internal double ButtonBlockWidth
        {
            get { return bBW; }
            private set
            {
                this.RaiseAndSetIfChanged (ref bBW, value, nameof (ButtonBlockWidth));
            }
        }

        private Thickness wAM;
        internal Thickness WorkAreaMargin
        {
            get { return wAM; }
            private set
            {
                this.RaiseAndSetIfChanged (ref wAM, value, nameof (WorkAreaMargin));
            }
        }

        private string eT;
        internal string ExtentionTip
        {
            get { return eT; }
            private set
            {
                this.RaiseAndSetIfChanged (ref eT, value, nameof (ExtentionTip));
            }
        }

        private string eC;
        internal string ExtenderContent
        {
            get { return eC; }
            private set
            {
                this.RaiseAndSetIfChanged (ref eC, value, nameof (ExtenderContent));
            }
        }


        private void SetButtonBlock ( )
        {
            ButtonWidth = _buttonWidth;
            CountWidth = _countLabelWidth;
            ButtonBlockWidth = _buttonBlockWidth;
            WorkAreaMargin = new Thickness (_workAreaLeftMargin, 0);
            ExtentionTip = _extentionToolTip;
            ExtenderContent = "\uF054";
        }


        internal void ExtendButtons ()
        {
            if ( _blockIsExtended )
            {
                CountWidth -= _extention;
                ButtonWidth -= _extention;
                ButtonBlockWidth -= _extention;
                WorkAreaMargin = new Thickness (( WorkAreaMargin. Left + _extention ), 0);
                ExtentionTip = _extentionToolTip;
                ExtenderContent = "\uF054";
                _blockIsExtended = false;
            }
            else 
            {
                CountWidth += _extention;
                ButtonWidth += _extention;
                ButtonBlockWidth += _extention;
                WorkAreaMargin = new Thickness (( WorkAreaMargin. Left - _extention ), 0);
                ExtentionTip = _shrinkingToolTip;
                ExtenderContent = "\uF053";
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

                           mainViewModel.SetWaitingPdfOrPrint ();
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


        private bool GeneratePdf ( string fileToSave )
        {
            List<PageViewModel> pages = GetAllPages ();
            bool result = _converter.ConvertToExtention (pages, fileToSave);
            return result;
        }


        internal void GeneratePdfDuringWaiting ( )
        {
            TappedPdfGenerationButton = 0;

            Task generationTask = new Task 
            (
                () => 
                {
                    _isPdfGenerated = GeneratePdf (_pdfFileName);
                }
            );

            generationTask.ContinueWith
                (
                ( tsk ) =>
                {
                    if ( _isPdfGenerated == false )
                    {
                        Dispatcher.UIThread.InvokeAsync
                        (() =>
                        {
                            var messegeDialog = new MessageDialog ();
                            messegeDialog.Message = _fileIsOpenMessage;
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
                }
                );

            generationTask.Start ();
        }


        public void Print ()
        {
            ModernMainViewModel mainViewModel = App.services.GetRequiredService<ModernMainViewModel> ();

            TappedPrintButton = 1;

            mainViewModel.SetWaitingPdfOrPrint ();
        }


        public void PrintDuringWaiting ()
        {
            TappedPrintButton = 0;

            List <PageViewModel> pages = GetAllPages ();
            string fileToSave = @"intermidiate.pdf";

            Task pdf = new Task
            (
                () =>
                {
                    _converter.ConvertToExtention (pages, fileToSave);
                }
            );

            pdf.Start ();

            Task printTask = pdf.ContinueWith
            (
                savingTask =>
                {
                    Dispatcher.UIThread.InvokeAsync
                    (() =>
                    {
                        ModernMainViewModel modernMV = App.services.GetRequiredService<ModernMainViewModel> ();
                        modernMV.EndWaitingPdfOrPrint ();
                    });

                    if ( App.OsName == "Windows" )
                    {
                        var printerQuery = new ManagementObjectSearcher ("SELECT * from Win32_Printer");

                        foreach ( var printer in printerQuery.Get () )
                        {
                            var name = printer.GetPropertyValue ("Name");
                            var status = printer.GetPropertyValue ("Status");
                            var isDefault = printer.GetPropertyValue ("Default");
                            var isNetworkPrinter = printer.GetPropertyValue ("Network");

                            //Console.WriteLine ("{0} (Status: {1}, Default: {2}, Network: {3}",
                            //            name, status, isDefault, isNetworkPrinter);

                            int dfd = 0;
                        }

                        PrintDocument pD = new PrintDocument ();
                        pD.PrinterSettings.PrinterName = "";

                        





                        int length = _converter.intermidiateFiles.Count;
                        IStorageItem sItem = null;
                        //string printerName = "ORPO-7.mgkb.ru\Samsung ML-2160 Series";
                        //string printerName = "ORPO-7.mgkb.ru\Samsung ML-2160 Series";
                        string printerName = "Samsung ML-2160 Seriesqqqqq";

                        ProcessStartInfo info = new ()
                        {
                            FileName = fileToSave,
                            Verb = "Print",
                            UseShellExecute = true,
                            ErrorDialog = false,
                            CreateNoWindow = true,
                           // Arguments = "\"" + printerName + "\"",
                            WindowStyle = ProcessWindowStyle.Minimized
                        };

                        bool ? procIsExited = Process.Start (info)?.WaitForExit (20_000);
                    }
                    else if ( App.OsName == "Linux" )
                    {
                        string printCommand = "lp " + fileToSave;
                        ExecuteBashCommand (printCommand);
                    }
                }
            );
        }


        private static void ExecuteBashCommand ( string command )
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

                //string result = process.StandardOutput.ReadToEnd ();

                process.WaitForExit ();
            }
        }
    }
}