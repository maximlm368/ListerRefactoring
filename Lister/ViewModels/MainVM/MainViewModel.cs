using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ColorTextBlock.Avalonia;
using ContentAssembler;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Lister.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Lister.ViewModels
{
    public class MainViewModel : ReactiveObject
    {
        //private readonly string _suggestedFileNames = "Badge";
        //private readonly string _saveTitle = "Сохранение документа";
        //private readonly string _incorrectXSLX = " - некорректный файл.";
        //private readonly string _buildingLimitIsExhaustedMessage = "Исчерпан лимит построений.";
        //private readonly string _fileIsOpenMessage = "Файл открыт в другом приложении. Закройте его и повторите выбор.";

        private readonly string _suggestedFileNames;
        private readonly string _saveTitle;
        private readonly string _incorrectXSLX;
        private readonly string _buildingLimitIsExhaustedMessage;
        private readonly string _fileIsOpenMessage;

        public static bool MainViewIsWaiting { get; private set; }
        public static bool PrintingShouldStart { get; set; }
        internal string PdfFileName { get; private set; }
        public static bool PdfGenerationShouldStart { get; set; }

        private FilePickerSaveOptions _filePickerOptions;
        private FilePickerSaveOptions FilePickerOptions => _filePickerOptions ??= new ()
        {
            Title = _saveTitle,
            FileTypeChoices = 
            [
                new FilePickerFileType ("Источники данных")
                {
                    Patterns = ["*.pdf"]
                }
            ],
            SuggestedFileName = _suggestedFileNames + GenerateNowDateString ()
        };

        private string _osName;

        private PdfPrinter _pdfPrinter;
        private PrintDialog _printDialog;
        private PrintDialogViewModel _printDialogViewModel;
        private PersonSourceViewModel _personSourceViewModel;
        private PersonChoosingViewModel _personChoosingViewModel;
        private BadgesBuildingViewModel _badgesBuildingViewModel;
        private PageNavigationZoomerViewModel _zoomNavigationViewModel;
        private SceneViewModel _sceneViewModel;
        private WaitingViewModel _waitingViewModel;
        private MainView _view;

        private bool _buildButtonIsTapped = false;
        private PrintAdjustingData _printAdjusting;


        public MainViewModel ( string osName, string suggestedFileNames, string saveTitle, string incorrectXSLX
                             , string buildingLimitIsExhaustedMessage, string fileIsOpenMessage )
        {
            _osName = osName;
            _suggestedFileNames = suggestedFileNames;
            _saveTitle = saveTitle;
            _incorrectXSLX = incorrectXSLX;
            _buildingLimitIsExhaustedMessage = buildingLimitIsExhaustedMessage;
            _fileIsOpenMessage = fileIsOpenMessage;

            _pdfPrinter = App.services.GetRequiredService<PdfPrinter> ();
            _printDialogViewModel = App.services.GetRequiredService<PrintDialogViewModel> ();
            _personChoosingViewModel = App.services.GetRequiredService<PersonChoosingViewModel> ();
            _personSourceViewModel = App.services.GetRequiredService<PersonSourceViewModel> ();
            _badgesBuildingViewModel = App.services.GetRequiredService<BadgesBuildingViewModel> ();
            _zoomNavigationViewModel = App.services.GetRequiredService<PageNavigationZoomerViewModel> ();
            _sceneViewModel = App.services.GetRequiredService<SceneViewModel> ();
            _waitingViewModel = App.services.GetRequiredService<WaitingViewModel> ();

            _pdfPrinter.PropertyChanged += PrinterChanged;
            _printDialogViewModel.PropertyChanged += PrintDialogChanged;
            _personSourceViewModel.PropertyChanged += PersonSourceChanged;
            _personChoosingViewModel.PropertyChanged += PersonChoosingChanged;
            _badgesBuildingViewModel.PropertyChanged += BuildButtonTapped;
            _sceneViewModel.PropertyChanged += SceneChanged;
            _zoomNavigationViewModel.PropertyChanged += ZoomNavigationChanged;
        }


        private void PersonSourceChanged ( object ? sender, PropertyChangedEventArgs args )
        {
            if ( args.PropertyName == "SourceFilePath" ) 
            {
                PersonSourceViewModel personSource = ( PersonSourceViewModel ) sender;
                _personChoosingViewModel.SetPersonsFromFile (personSource.SourceFilePath);
            }
            else if ( args.PropertyName == "FileIsDeclined" )
            {
                string message = _personSourceViewModel.FilePath + _incorrectXSLX;
                var messegeDialog = new MessageDialog (MainView.Instance, message);

                WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
                waitingVM.Darken ();
                messegeDialog.ShowDialog (App.MainWindow);
            }
            else if ( args.PropertyName == "FileIsOpen" )
            {
                if ( MainView.Instance == null ) 
                {
                    return;
                }

                var messegeDialog = new MessageDialog (MainView.Instance, _fileIsOpenMessage);
                WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
                waitingVM.Darken ();
                messegeDialog.ShowDialog (MainWindow.Window);
            }
        }


        private void PersonChoosingChanged ( object ? sender, PropertyChangedEventArgs args )
        {
            if ( args.PropertyName == "AllAreReady" )
            {
                PersonChoosingViewModel personChoosingViewModel = (PersonChoosingViewModel) sender;

                _badgesBuildingViewModel.TryToEnableBadgeCreation ( personChoosingViewModel.AllAreReady );
            }
            else if ( args.PropertyName == "SickTemplateIsSet" )
            {
                if (_personChoosingViewModel.SickTemplateIsSet) 
                {
                    TemplateViewModel template = _personChoosingViewModel.ChosenTemplate;

                    var messegeDialog =
                    new LargeMessageDialog (MainView.Instance, template.CorrectnessMessage, template.SourcePath);

                    _waitingViewModel.Darken ();
                    messegeDialog.ShowDialog (MainWindow.Window);
                    messegeDialog.Focusable = true;
                    messegeDialog.Focus ();
                }
            }
        }


        private void BuildButtonTapped ( object ? sender, PropertyChangedEventArgs args )
        {
            if ( args.PropertyName == "BuildButtonIsTapped" ) 
            {
                if ( string.IsNullOrWhiteSpace (_personChoosingViewModel.ChosenTemplate.Name) )
                {
                    return;
                }

                bool shouldShowMessage = ( _personChoosingViewModel.ChosenTemplate. CorrectnessMessage != null )
                                         &&   ( _personChoosingViewModel.ChosenTemplate. CorrectnessMessage. Count > 0 ) ;

                if ( shouldShowMessage )
                {
                    List<string> message = _personChoosingViewModel.ChosenTemplate.CorrectnessMessage;
                    string path = _personChoosingViewModel.ChosenTemplate.SourcePath;

                    var messegeDialog = new LargeMessageDialog (MainView.Instance, message, path);

                    _waitingViewModel.Darken ();
                    messegeDialog.ShowDialog (MainWindow.Window);
                    messegeDialog.Focusable = true;
                    messegeDialog.Focus ();

                    _buildButtonIsTapped = true;
                }
                else 
                {
                    InduceBadgeBuilding ();
                }
            }
        }


        private void SceneChanged ( object ? sender, PropertyChangedEventArgs args )
        {
            if ( args.PropertyName == "BadgesAreCleared" )
            {
                _zoomNavigationViewModel.ToZeroState ();
            }
            else if ( args.PropertyName == "BuildingIsOccured" )
            {
                if ( _sceneViewModel.BuildingIsOccured )
                {
                    EndWaiting ();
                    _zoomNavigationViewModel.EnableZoomIfPossible (_sceneViewModel.BuildingIsOccured);
                    _zoomNavigationViewModel.SetEnablePageNavigation (_sceneViewModel.PageCount
                                                                    , _sceneViewModel.VisiblePageNumber);
                }
                else 
                {
                    EndWaiting ();

                    var messegeDialog = new MessageDialog (MainView.Instance, _buildingLimitIsExhaustedMessage);

                    WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
                    waitingVM.Darken ();
                    messegeDialog.ShowDialog (MainWindow.Window);
                }
            }
            else if ( args.PropertyName == "EditIncorrectsIsSelected" )
            {
                MainView mainView = MainView.Instance;
                mainView.EditIncorrectBadges (_sceneViewModel.IncorrectBadges, _sceneViewModel.PrintableBadges
                                                                                , _sceneViewModel.AllPages [0]);
            }
            else if ( args.PropertyName == "PdfIsWanted" )
            {
                PreparePdfGeneration ();
            }
            else if ( args.PropertyName == "PrintingIsWanted" )
            {
                PreparePrinting ();
            }
        }


        private void PrinterChanged ( object? sender, PropertyChangedEventArgs args )
        {
            if ( args.PropertyName == "PdfGenerationSuccesseeded" )
            {
                EndWaiting ();
                ShowInFileExplorer ();
            }
            else if ( args.PropertyName == "PrintingEnded" ) 
            {
                EndWaiting ();
            }
        }


        private void PrintDialogChanged ( object? sender, PropertyChangedEventArgs args )
        {
            if ( args.PropertyName == "NeedClose" )
            {
                _printDialog.Close ();
                HandleDialogClosing ();

                if ( _printAdjusting.Cancelled )
                {
                    return;
                }

                WaitForPrinting ();
            }
        }


        private void ZoomNavigationChanged ( object ? sender, PropertyChangedEventArgs args )
        {
            if ( args.PropertyName == "ZoomDegree" )
            {
                _sceneViewModel.Zoom (_zoomNavigationViewModel.ZoomDegree);
            }
            else if ( args.PropertyName == "VisiblePageNumber" ) 
            {
                _sceneViewModel.ShowPageWithNumber (_zoomNavigationViewModel.VisiblePageNumber);
            }
        }


        internal void HandleDialogClosing ()
        {
            _waitingViewModel.HandleDialogClosing ();

            if ( _buildButtonIsTapped ) 
            {
                InduceBadgeBuilding ();
                _buildButtonIsTapped = false;
            }
        }


        internal void InduceBadgeBuilding ()
        {
            _sceneViewModel.Build (_personChoosingViewModel.ChosenTemplate.Name, _personChoosingViewModel.ChosenPerson);

            if ( _personChoosingViewModel.ChosenPerson == null )
            {
                SetWaitingUpdatingLayout ();
            }
        }


        internal void ResetIncorrects ( )
        {
            _sceneViewModel.ResetIncorrects ();
            _personChoosingViewModel.RefreshTemplateChoosingAppearence ();
        }


        internal void PassView ( MainView view )
        {
            _view = view;
            WaitingView wv = _view. waiting;
        }


        internal void SetWaitingUpdatingLayout ( )
        {
            _waitingViewModel.Show ();
            MainViewIsWaiting = true;
        }


        internal void EndWaiting ()
        {
            _badgesBuildingViewModel.BuildingIsPossible = true;
            _waitingViewModel.Hide ();
            MainViewIsWaiting = false;
        }


        private async void PreparePdfGeneration ()
        {
            List <FilePickerFileType> fileExtentions = [];
            fileExtentions.Add (FilePickerFileTypes.Pdf);
            FilePickerSaveOptions options = new ();
            options.Title = _saveTitle;
            options.FileTypeChoices = new ReadOnlyCollection <FilePickerFileType> (fileExtentions);
            options.SuggestedFileName = _suggestedFileNames + GenerateNowDateString ();
            IStorageFile chosenFile = await MainWindow.CommonStorageProvider.SaveFilePickerAsync (FilePickerOptions);

            bool savingCancelled = ( chosenFile == null );

            if ( savingCancelled ) 
            {
                return;
            }

            PdfFileName = chosenFile.Path.ToString ();
            int uriTypeLength = App.ResourceUriType.Length;
            PdfFileName = PdfFileName.Substring (uriTypeLength, PdfFileName.Length - uriTypeLength);

            PdfGenerationShouldStart = true;
            SetWaitingUpdatingLayout ();
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


        private void ShowInFileExplorer () 
        {
            if ( _pdfPrinter.PdfGenerationSuccesseeded )
            {
                if ( App.OsName == "Windows" )
                {
                    Process fileExplorer = new Process ();
                    fileExplorer.StartInfo.FileName = "explorer.exe";
                    string pdfFileName = ExtractPathWithoutFileName (PdfFileName);
                    pdfFileName = pdfFileName.Replace ('/', '\\');
                    fileExplorer.StartInfo.Arguments = pdfFileName;
                    fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                    fileExplorer.Start ();
                }
                else if ( App.OsName == "Linux" )
                {
                    Process fileExplorer = new Process ();
                    fileExplorer.StartInfo.FileName = "nautilus";
                    string pdfFileName = ExtractPathWithoutFileName (PdfFileName);
                    fileExplorer.StartInfo.Arguments = pdfFileName;
                    fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                    fileExplorer.Start ();
                }
            }
            else
            {
                var messegeDialog = new MessageDialog (MainView.Instance, _fileIsOpenMessage);
                WaitingViewModel waitingVM = App.services.GetRequiredService<WaitingViewModel> ();
                _waitingViewModel.Darken ();
                messegeDialog.ShowDialog (MainWindow.Window);
            }
        }


        private void PreparePrinting ()
        {
            _printAdjusting = new ();

            _printDialog = new PrintDialog (_sceneViewModel.GetPrintablePagesCount(), _printAdjusting);

            //printDialog.Closed += ( sender, args ) =>
            //{
            //    if ( _printAdjusting.Cancelled )
            //    {
            //        return;
            //    }

            //    WaitForPrinting ();
            //};

            _waitingViewModel.Darken ();
            _printDialog.ShowDialog (MainWindow.Window);
        }


        private void WaitForPrinting ()
        {
            PrintingShouldStart = true;
            SetWaitingUpdatingLayout ();
        }









        internal void LayoutUpdated ( )
        {
            if ( SceneViewModel.EntireListBuildingIsChosen   &&   MainViewIsWaiting )
            {
                _sceneViewModel.BuildDuringWaiting ();
                return;
            }
            else if ( PdfGenerationShouldStart )
            {
                PdfGenerationShouldStart = false;
                _pdfPrinter.GeneratePdfDuringWaiting (PdfFileName, _sceneViewModel.GetPrintablePages());
                return;
            }
            else if ( PrintingShouldStart )
            {
                PrintingShouldStart = false;
                _pdfPrinter.PrintDuringWaiting (_sceneViewModel.GetPrintablePages (), _printAdjusting, _osName);
                return;
            }
            else if ( MainView.TappedGoToEditorButton == 1 )
            {
                _view.SwitchToEditor ();
            }
        }


        internal void OnLoaded ( )
        {
            _personSourceViewModel.DeclineKeepedFileIfIncorrect ();
        }


        private string ExtractPathWithoutFileName ( string wholePath )
        {
            var builder = new StringBuilder ();
            string goalPath = string.Empty;

            for ( var index = wholePath.Length - 1;   index >= 0;   index-- )
            {
                bool fileNameIsAchieved = ( wholePath [index] == '/' )
                                     ||   ( wholePath [index] == '\\' );

                if ( fileNameIsAchieved )
                {
                    goalPath = wholePath.Substring (0, index);
                    break;
                }
            }

            return goalPath;
        }
    }


}


