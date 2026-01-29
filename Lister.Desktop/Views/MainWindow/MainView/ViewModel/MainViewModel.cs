using Avalonia.Platform.Storage;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lister.Desktop.ModelMappings;
using Lister.Desktop.ModelMappings.BadgeVM;
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

public sealed partial class MainViewModel : ObservableObject
{
    internal static bool HasWaitingState { get; private set; }

    private static bool _printingShouldStart;
    private static bool _pdfGenerationShouldStart;

    private readonly string _suggestedFileNames;
    private readonly string _saveTitle;
    private readonly string _incorrectXSLX;
    private readonly string _buildingLimitIsExhaustedMessage;
    private readonly string _fileIsOpenMessage;
    private readonly string _osName;
    private readonly Printer _pdfPrinter;
    private readonly PrintDialogViewModel _printDialogViewModel;
    private readonly PersonSourceViewModel _personSourceViewModel;
    private readonly PersonChoosingViewModel _personChoosingViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor ( nameof ( BuildBadgesCommand ) )]
    private bool _buildingIsPossible = false;

    //private bool _buildButtonIsTapped = false;
    //internal bool BuildButtonIsTapped
    //{
    //    get
    //    {
    //        return _buildButtonIsTapped;
    //    }

    //    private set
    //    {
    //        _buildButtonIsTapped = value;

    //        if ( _buildButtonIsTapped )
    //        {
    //            OnPropertyChanged ();
    //        }
    //    }
    //}

    private readonly NavigationZoomViewModel _zoomNavigationViewModel;
    private readonly SceneViewModel _sceneViewModel;
    private readonly WaitingViewModel _waitingViewModel;
    private bool _buildButtonIsTapped = false;
    private PrintAdjustingData? _printAdjusting;

    internal string? PdfName { get; private set; } = string.Empty;

    public event Action<List<BadgeViewModel>>? EditionIsChosen;
    public event Action<string>? HasToShowMessage;
    public event Action<List<string>, string>? HasToShowTemplateErrors;
    public event Action<int, PrintAdjustingData>? HasToPreparePrinting;

    internal MainViewModel ( MainViewModelArgs args )
    {
        _osName = args.OsName;
        _suggestedFileNames = args.SuggestedFileNames;
        _saveTitle = args.SaveTitle;
        _incorrectXSLX = args.IncorrectXSLX;
        _buildingLimitIsExhaustedMessage = args.BuildingLimitExhaustedMessage;
        _fileIsOpenMessage = args.FileIsOpenMessage;

        _pdfPrinter = args.Printer;
        _printDialogViewModel = args.PrintDialog;
        _personChoosingViewModel = args.PersonChoosing;
        _personSourceViewModel = args.PersonSource;
        _zoomNavigationViewModel = args.NavigationZoom;
        _sceneViewModel = args.Scene;
        _waitingViewModel = args.Waiting;

        _pdfPrinter.PropertyChanged += PrinterChanged;
        _printDialogViewModel.PropertyChanged += PrintDialogChanged;
        _personSourceViewModel.PropertyChanged += PersonSourceChanged;
        _personChoosingViewModel.PropertyChanged += PersonChoosingChanged;
        _sceneViewModel.PropertyChanged += SceneChanged;
        _zoomNavigationViewModel.PropertyChanged += ZoomNavigationChanged;
    }

    private void PersonSourceChanged ( object? sender, PropertyChangedEventArgs args )
    {
        if ( args.PropertyName == "SourceFilePath" )
        {
            _personChoosingViewModel?.ResetPersons ();
        }
        else if ( args.PropertyName == "FileIsDeclined" )
        {
            RequireMessageWindow ( _personSourceViewModel?.FilePath + _incorrectXSLX );
        }
        else if ( args.PropertyName == "FileIsTooBig" )
        {
            string limit = _sceneViewModel?.GetLimit ().ToString () + ".";
            RequireMessageWindow ( _buildingLimitIsExhaustedMessage + limit );
        }
        else if ( args.PropertyName == "FileIsOpen" )
        {
            RequireMessageWindow ( _fileIsOpenMessage );
        }
    }

    private void PersonChoosingChanged ( object? sender, PropertyChangedEventArgs args )
    {
        if ( args.PropertyName == "SettingsIsComplated" )
        {
            BuildingIsPossible = sender is PersonChoosingViewModel personChoosingViewModel
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

    private void Build ( )
    {
        //if ( args.PropertyName == "BuildButtonIsTapped" )

        if ( true )
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
                RequireMessageWindow ( _buildingLimitIsExhaustedMessage + limit );
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
        if ( args.PropertyName == "IsClosing" )
        {
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

    internal void HandleDialogClosing ()
    {
        _waitingViewModel.Hide ();

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
            WaitWhileBuilding ();
        }
    }

    internal void Show ()
    {
        _sceneViewModel.Refresh ();
    }

    internal void WaitWhileBuilding ()
    {
        _waitingViewModel.Show ();
        HasWaitingState = true;
    }

    internal void EndWaiting ()
    {
        BuildingIsPossible = true;
        _waitingViewModel.Hide ();
        HasWaitingState = false;
    }

    private async void PreparePdfGeneration ()
    {
        if ( MainWindow.CommonStorageProvider == null )
        {
            return;
        }

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
                RequireMessageWindow ( _fileIsOpenMessage );

                return;
            }
        }

        _pdfGenerationShouldStart = true;
        WaitWhileBuilding ();
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
            RequireMessageWindow ( _fileIsOpenMessage );
        }
    }

    private void RequireMessageWindow ( string message )
    {
        _waitingViewModel.Darken ();
        HasToShowMessage?.Invoke ( message );
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

        _waitingViewModel.Darken ();
        HasToShowTemplateErrors?.Invoke ( message, errorSource );
    }

    private void PreparePrinting ()
    {
        _printAdjusting = new ();
        _waitingViewModel.Darken ();
        HasToPreparePrinting?.Invoke ( _sceneViewModel.AllPages.Count, _printAdjusting );
    }

    private void WaitForPrinting ()
    {
        _printingShouldStart = true;
        WaitWhileBuilding ();
    }

    internal void ProcessDocument ()
    {
        if ( SceneViewModel.EntireListBuildingIsChosen && HasWaitingState )
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

    [RelayCommand ( CanExecute = nameof ( CanBuild ) )]
    private void BuildBadges ()
    {
        _buildButtonIsTapped = true;
        _buildButtonIsTapped = false;

        Build ();
    }

    private bool CanBuild ()
    {
        return BuildingIsPossible;
    }
}
