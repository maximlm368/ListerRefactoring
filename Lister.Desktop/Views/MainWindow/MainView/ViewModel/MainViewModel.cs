using Avalonia.Platform.Storage;
using Lister.Desktop.ModelMappings;
using Lister.Desktop.ModelMappings.BadgeVM;
using Lister.Desktop.Views.DialogMessageWindows.LargeMessage;
using Lister.Desktop.Views.DialogMessageWindows.Message;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.BuildButton.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.NavigationZoom.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonSource.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.Scene;
using Lister.Desktop.Views.MainWindow.MainView.Parts.Scene.ViewModel;
using Lister.Desktop.Views.MainWindow.WaitingView.ViewModel;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Lister.Desktop.Views.MainWindow.MainView.ViewModel;

public sealed class MainViewModel : ReactiveObject
{
    private static bool _inWaitingState;
    internal static bool InWaitingState
    {
        get { return _inWaitingState; }
        private set { _inWaitingState = value; }
    }

    internal static bool MainViewIsWaiting { get; private set; }

    private static bool _printingShouldStart;
    private static bool _pdfGenerationShouldStart;

    private readonly string _suggestedFileNames;
    private readonly string _saveTitle;
    private readonly string _incorrectXSLX;
    private readonly string _fileIsTooBigMessage;
    private readonly string _buildingLimitIsExhaustedMessage;
    private readonly string _fileIsOpenMessage;
    //private FilePickerSaveOptions _filePickerOptions;
    //private FilePickerSaveOptions FilePickerOptions => _filePickerOptions ??= new ()
    //{
    //    Title = _saveTitle,
    //    FileTypeChoices =
    //    [
    //        new FilePickerFileType ("Источники данных")
    //        {
    //            Patterns = ["*.pdf"]
    //        }
    //    ],
    //    SuggestedFileName = _suggestedFileNames + GenerateNowDateString ()
    //};
    private readonly string _osName;
    private readonly Printer _pdfPrinter;
    private PrintDialog? _printDialog;
    private readonly PrintDialogViewModel _printDialogViewModel;
    private readonly PersonSourceViewModel _personSourceViewModel;
    private readonly PersonChoosingViewModel _personChoosingViewModel;
    private readonly BadgesBuildingViewModel _badgesBuildingViewModel;
    private readonly NavigationZoomViewModel _zoomNavigationViewModel;
    private readonly SceneViewModel _sceneViewModel;
    private readonly WaitingViewModel _waitingViewModel;
    private bool _buildButtonIsTapped = false;
    private PrintAdjustingData? _printAdjusting;

    internal string? PdfName { get; private set; } = string.Empty;

    public delegate void EditionIsChosenHandler ( List<BadgeViewModel> processableBadges );
    public event EditionIsChosenHandler? EditionIsChosen;

    internal MainViewModel ( MainViewModelArgs args )
    {
        _osName = args.OsName;
        _suggestedFileNames = args.SuggestedFileNames;
        _saveTitle = args.SaveTitle;
        _incorrectXSLX = args.IncorrectXSLX;
        _buildingLimitIsExhaustedMessage = args.BuildingLimitExhaustedMessage;
        _fileIsOpenMessage = args.FileIsOpenMessage;
        _fileIsTooBigMessage = args.FileIsTooBigMessage;

        _pdfPrinter = args.Printer;
        _printDialogViewModel = args.PrintDialogViewModel;
        _personChoosingViewModel = args.PersonChoosingViewModel;
        _personSourceViewModel = args.PersonSourceViewModel;
        _badgesBuildingViewModel = args.BadgesBuildingViewModel;
        _zoomNavigationViewModel = args.NavigationZoomViewModel;
        _sceneViewModel = args.SceneViewModel;
        _waitingViewModel = args.WaitingViewModel;

        _pdfPrinter.PropertyChanged += PrinterChanged;
        _printDialogViewModel.PropertyChanged += PrintDialogChanged;
        _personSourceViewModel.PropertyChanged += PersonSourceChanged;
        _personChoosingViewModel.PropertyChanged += PersonChoosingChanged;
        _badgesBuildingViewModel.PropertyChanged += BuildButtonTapped;
        _sceneViewModel.PropertyChanged += SceneChanged;
        _zoomNavigationViewModel.PropertyChanged += ZoomNavigationChanged;
    }

    private void PersonSourceChanged ( object? sender, PropertyChangedEventArgs args )
    {
        if ( args.PropertyName == "SourceFilePath" )
        {
            _personChoosingViewModel.ResetPersons ();
        }
        else if ( args.PropertyName == "FileIsDeclined" )
        {
            ShowMessageWindow ( _personSourceViewModel.FilePath + _incorrectXSLX );
        }
        else if ( args.PropertyName == "FileIsTooBig" )
        {
            string limit = _sceneViewModel.GetLimit ().ToString () + ".";
            ShowMessageWindow ( _buildingLimitIsExhaustedMessage + limit );
        }
        else if ( args.PropertyName == "FileIsOpen" )
        {
            ShowMessageWindow ( _fileIsOpenMessage );
        }
    }

    private void PersonChoosingChanged ( object? sender, PropertyChangedEventArgs args )
    {
        if ( args.PropertyName == "SettingsIsComplated" )
        {
            _badgesBuildingViewModel.BuildingIsPossible = sender is PersonChoosingViewModel personChoosingViewModel
                && personChoosingViewModel.SettingsIsComplated;
        }
        else if ( args.PropertyName == "SickTemplateIsSet" )
        {
            if ( _personChoosingViewModel.SickTemplateIsSet )
            {
                TemplateViewModel? template = _personChoosingViewModel.ChosenTemplate;
                ShowTemplateErrors ( template?.CorrectnessMessage, template?.SourcePath );
            }
        }
        else if ( args.PropertyName == "FileNotFound" )
        {
            _personSourceViewModel.EmptySourcePath ();
        }
    }

    private void BuildButtonTapped ( object? sender, PropertyChangedEventArgs args )
    {
        if ( args.PropertyName == "BuildButtonIsTapped" )
        {
            if ( _personChoosingViewModel.ChosenTemplate == null
                 || string.IsNullOrWhiteSpace ( _personChoosingViewModel.ChosenTemplate.Name )
            )
            {
                return;
            }

            bool shouldShowMessage = _personChoosingViewModel.ChosenTemplate.CorrectnessMessage != null
                && _personChoosingViewModel.ChosenTemplate.CorrectnessMessage.Count > 0;

            if ( shouldShowMessage )
            {
                List<string>? message = _personChoosingViewModel.ChosenTemplate.CorrectnessMessage;
                string templatePath = _personChoosingViewModel.ChosenTemplate.SourcePath;
                ShowTemplateErrors ( message, templatePath );
                _buildButtonIsTapped = true;
            }
            else
            {
                InduceBadgeBuilding ();
            }
        }
    }

    private void SceneChanged ( object? sender, PropertyChangedEventArgs args )
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
                _zoomNavigationViewModel.EnableZoomIfPossible ( _sceneViewModel.BuildingIsOccured );
                _zoomNavigationViewModel.SetEnablePageNavigation ( _sceneViewModel.PageCount, _sceneViewModel.VisiblePageNumber );
            }
            else
            {
                EndWaiting ();
                string limit = _sceneViewModel.GetLimit ().ToString () + ".";
                ShowMessageWindow ( _buildingLimitIsExhaustedMessage + limit );
            }
        }
        else if ( args.PropertyName == "EditIsSelected" )
        {
            EditionIsChosen?.Invoke ( _sceneViewModel.ProcessableBadges );
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
        else if ( args.PropertyName == "PrintingIsFinished" )
        {
            EndWaiting ();
        }
    }

    private void PrintDialogChanged ( object? sender, PropertyChangedEventArgs args )
    {
        if ( args.PropertyName == "NeedClose" )
        {
            _printDialog?.Close ();
            HandleDialogClosing ();

            if ( _printAdjusting == null || _printAdjusting.IsCancelled )
            {
                return;
            }

            WaitForPrinting ();
        }
    }

    private void ZoomNavigationChanged ( object? sender, PropertyChangedEventArgs args )
    {
        if ( args.PropertyName == "ZoomDegree" )
        {
            _sceneViewModel.Zoom ( _zoomNavigationViewModel.ZoomDegree );
        }
        else if ( args.PropertyName == "VisiblePageNumber" )
        {
            _sceneViewModel.ShowPageWithNumber ( _zoomNavigationViewModel.VisiblePageNumber );
        }
    }

    private void HandleDialogClosing ()
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
        if ( _personChoosingViewModel.ChosenTemplate == null )
        {
            return;
        }

        _sceneViewModel.Build ( _personChoosingViewModel.ChosenTemplate.Name, _personChoosingViewModel.ChosenPerson );

        if ( _personChoosingViewModel.ChosenPerson == null )
        {
            WaitingWhileBuilding ();
        }
    }

    internal void RefreshAppearences ()
    {
        _sceneViewModel.ResetIncorrects ();
    }

    internal void WaitingWhileBuilding ()
    {
        _waitingViewModel.Show ();
        MainViewIsWaiting = true;
        InWaitingState = true;
    }

    internal void EndWaiting ()
    {
        _badgesBuildingViewModel.BuildingIsPossible = true;
        _waitingViewModel.Hide ();
        MainViewIsWaiting = false;

        InWaitingState = false;
    }

    private async void PreparePdfGeneration ()
    {
        List<FilePickerFileType> fileExtentions = [];
        fileExtentions.Add ( FilePickerFileTypes.Pdf );

        FilePickerSaveOptions options = new ()
        {
            Title = _saveTitle,
            FileTypeChoices = new ReadOnlyCollection<FilePickerFileType> ( fileExtentions ),
            SuggestedFileName = _suggestedFileNames + GenerateNowDateString ()
        };

        IStorageFile? chosenFile = await MainWindow.CommonStorageProvider.SaveFilePickerAsync ( options );
        PdfName = chosenFile?.Path.AbsolutePath;
        bool savingCancelled = chosenFile == null;

        if ( savingCancelled || PdfName == null )
        {
            return;
        }

        FileInfo fileInfo;

        try
        {
            fileInfo = new ( PdfName );
        }
        catch ( UnauthorizedAccessException )
        {
            return;
        }

        if ( fileInfo.Exists )
        {
            try
            {
                FileStream stream = fileInfo.OpenWrite ();
                stream.Close ();
            }
            catch ( IOException )
            {
                ShowMessageWindow ( _fileIsOpenMessage );

                return;
            }
        }

        _pdfGenerationShouldStart = true;
        WaitingWhileBuilding ();
    }

    private static string GenerateNowDateString ()
    {
        DateTime now = DateTime.Now;

        string day = now.Day.ToString ();

        if ( int.Parse ( day ) < 10 )
        {
            day = "0" + day;
        }

        string month = now.Month.ToString ();

        if ( int.Parse ( month ) < 10 )
        {
            month = "0" + month;
        }

        string hour = now.Hour.ToString ();

        if ( int.Parse ( hour ) < 10 )
        {
            hour = "0" + hour;
        }

        string minute = now.Minute.ToString ();

        if ( int.Parse ( minute ) < 10 )
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
            string procName = string.Empty;

            Process fileExplorer = new ();
            string pdfFileName = ExtractPathWithoutFileName ( PdfName );

            if ( _osName == "Windows" )
            {
                procName = "explorer.exe";
                pdfFileName = pdfFileName.Replace ( '/', '\\' );
            }
            else if ( _osName == "Linux" )
            {
                procName = "nautilus";
            }

            fileExplorer.StartInfo.FileName = procName;
            fileExplorer.StartInfo.Arguments = pdfFileName;
            fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            fileExplorer.Start ();
        }
        else
        {
            ShowMessageWindow ( _fileIsOpenMessage );
        }
    }

    private void ShowMessageWindow ( string message )
    {
        MessageWindow messegeWindow = new ( message );
        MainWindow.Window.ModalWindow = messegeWindow;
        messegeWindow.Closed += ( s, a ) => { HandleDialogClosing (); };
        _waitingViewModel.Darken ();
        messegeWindow.ShowDialog ( MainWindow.Window );
    }

    private void ShowTemplateErrors ( List<string>? message, string? errorSource )
    {
        if ( message == null
            || message.Count < 1
            || errorSource == null
        )
        {
            return;
        }

        LargeMessageDialog messegeDialog = new ( message, errorSource );
        messegeDialog.Closed += ( s, a ) => { HandleDialogClosing (); };
        _waitingViewModel.Darken ();
        messegeDialog.ShowDialog ( MainWindow.Window );
        messegeDialog.Focusable = true;
        messegeDialog.Focus ();
    }

    private void PreparePrinting ()
    {
        _printAdjusting = new ();
        _printDialog = new ( _sceneViewModel.AllPages.Count, _printAdjusting );
        _waitingViewModel.Darken ();
        _printDialog.ShowDialog ( MainWindow.Window );
    }

    private void WaitForPrinting ()
    {
        _printingShouldStart = true;
        WaitingWhileBuilding ();
    }

    internal void ProcessDocument ()
    {
        if ( SceneViewModel.EntireListBuildingIsChosen && MainViewIsWaiting )
        {
            _sceneViewModel.BuildDuringWaiting ();

            return;
        }
        else if ( _pdfGenerationShouldStart )
        {
            _pdfGenerationShouldStart = false;

            if ( string.IsNullOrWhiteSpace ( PdfName ) )
            {
                EndWaiting ();

                return;
            }

            _pdfPrinter.GeneratePdfDuringWaiting ( PdfName );

            return;
        }
        else if ( _printingShouldStart )
        {
            _printingShouldStart = false;

            if ( _printAdjusting == null ) 
            {
                EndWaiting ();

                return;
            }

            _pdfPrinter.PrintDuringWaiting ( _printAdjusting );
        }
    }

    internal void OnLoaded ()
    {
        _personSourceViewModel.DeclineKeepedFileIfIncorrect ();
    }

    private static string ExtractPathWithoutFileName ( string? wholePath )
    {
        string result = string.Empty;

        if ( wholePath == null )
        {
            return result;
        }

        for ( var index = wholePath.Length - 1; index >= 0; index-- )
        {
            bool fileNameIsAchieved = wholePath [index] == '/' || wholePath [index] == '\\';

            if ( fileNameIsAchieved )
            {
                result = wholePath [..index];

                break;
            }
        }

        return result;
    }
}
