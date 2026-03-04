using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lister.Desktop.ModelMappings;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonSource.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.Scene;
using Lister.Desktop.Views.MainWindow.MainView.Parts.Scene.ViewModel;
using Lister.Desktop.Views.MainWindow.SharedComponents.ButtonsBlock.ViewModel;
using Lister.Desktop.Views.MainWindow.SharedComponents.Navigator.ViewModel;
using Lister.Desktop.Views.MainWindow.SharedComponents.Zoomer.ViewModel;
using Lister.Desktop.Views.MainWindow.WaitingView.ViewModel;
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
    private readonly string _fileIsOpenMessage;
    private readonly string _incorrectXSLX;
    private readonly string _buildLimitExhaustedMessage;
    private readonly string _osName;
    private readonly DocumentOutsider _pdfPrinter;
    private readonly PrintDialogViewModel _printDialog;
    internal PersonSourceViewModel PersonSource { get; private set; }
    internal PersonChoosingViewModel PersonChoosing { get; private set; }
    internal NavigatorViewModel? Navigator { get; private set; }
    internal ZoomerViewModel? Zoomer { get; private set; }
    internal ButtonsBlockViewModel? ButtonsBlock { get; set; }
    internal SceneViewModel Scene { get; private set; }
    internal WaitingViewModel Waiting { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor ( nameof ( BuildBadgesCommand ) )]
    private bool _buildingIsPossible = false;

    private bool _isBuildButtonTapped = false;
    private PrintAdjustingData? _printAdjusting;

    internal string? PdfName { get; private set; } = string.Empty;

    internal static event Action<string>? HasToShowMessage;
    internal static event Action<List<string>, string>? HasToShowTemplateErrors;
    internal static event Action<int, PrintAdjustingData>? HasToPreparePrinting;
    internal static event Func<FilePickerSaveOptions, Task<IStorageFile?>?>? FilePickerRequired;

    internal MainViewModel ( MainViewModelArgs args )
    {
        _osName = args.OsName;
        _suggestedFileNames = args.SuggestedFileNames;
        _saveTitle = args.SaveTitle;
        _incorrectXSLX = args.IncorrectXSLX;
        _buildLimitExhaustedMessage = args.BuildingLimitExhaustedMessage;
        _fileIsOpenMessage = args.FileIsOpenMessage;

        _pdfPrinter = args.Printer;
        _printDialog = args.PrintDialog;
        PersonChoosing = args.PersonChoosing;
        PersonSource = args.PersonSource;
        Scene = args.Scene;
        Waiting = args.Waiting;

        _pdfPrinter.PropertyChanged += PrinterChanged;
        PersonSource.PropertyChanged += PersonSourceChanged;
        PersonChoosing.PropertyChanged += PersonChoosingChanged;
        Scene.PropertyChanged += SceneChanged;
    }

    internal void SetZoomer ( ZoomerViewModel zoomer )
    {
        Zoomer = zoomer;

        Zoomer.ZoomedOn += ( step ) => 
        {
            Scene.ZoomOn ();
        };

        Zoomer.ZoomedOut += ( step ) =>
        {
            Scene.ZoomOut ();
        };
    }

    internal void SetNavigator ( NavigatorViewModel navigator )
    {
        Navigator = navigator;

        Navigator.CurrentNumberChanged += ( number ) => 
        {
            Scene.ShowPageWithNumber ( number );
        };
    }

    internal void SetButtonsBlock ( ButtonsBlockViewModel? block )
    {
        if ( block == null ) 
        {
            return;
        }

        ButtonsBlock = block;
        ButtonsBlock.ClearTapped += () =>
        {
            Scene?.ClearAllPages ();
        };

        ButtonsBlock.GeneratePdfRequired += PreparePdfGeneration;
        ButtonsBlock.PrintRequired += PreparePrinting;
    }

    private void PersonSourceChanged ( object? sender, PropertyChangedEventArgs args )
    {
        if ( args.PropertyName == "SourceFilePath" )
        {
            PersonChoosing?.ResetPersons ();
        }
        else if ( args.PropertyName == "FileIsDeclined" )
        {
            RequireMessageWindow ( PersonSource?.FilePath + _incorrectXSLX );
        }
        else if ( args.PropertyName == "FileIsTooBig" )
        {
            string limit = Scene?.GetLimit ().ToString () + ".";
            RequireMessageWindow ( _buildLimitExhaustedMessage + limit );
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
            if ( PersonChoosing.SickTemplateIsSet )
            {
                TemplateViewModel? template = PersonChoosing.ChosenTemplate;
                ShowTemplateErrors ( template?.CorrectnessMessage, template?.SourcePath );
            }
        }
        else if ( args.PropertyName == "FileNotFound" )
        {
            PersonSource.EmptySourcePath ();
        }
    }

    private void Build ()
    {
        if ( PersonChoosing.ChosenTemplate == null || string.IsNullOrWhiteSpace ( PersonChoosing.ChosenTemplate.Name ) )
        {
            return;
        }

        bool shouldShowMessage = PersonChoosing.ChosenTemplate.CorrectnessMessage != null
            && PersonChoosing.ChosenTemplate.CorrectnessMessage.Count > 0;

        if ( shouldShowMessage )
        {
            List<string>? message = PersonChoosing.ChosenTemplate.CorrectnessMessage;
            string templatePath = PersonChoosing.ChosenTemplate.SourcePath;
            ShowTemplateErrors ( message, templatePath );
            _isBuildButtonTapped = true;
        }
        else
        {
            InduceBadgeBuilding ();
        }
    }

    private void SceneChanged ( object? sender, PropertyChangedEventArgs args )
    {
        if ( args.PropertyName == "IsPagesNotEmpty" && !Scene.IsPagesNotEmpty )
        {
            Navigator?.ToZeroState ();
            Zoomer?.ToZeroState ();
        }
        else if ( args.PropertyName == "IsBuildSucceeded" )
        {
            if ( Scene.IsBuildSucceeded )
            {
                EndWaiting ();
                Zoomer?.EnableZoom ( );
                Navigator?.EnableNavigation ( Scene.PageCount, Scene.VisiblePageNumber );
            }
            else
            {
                EndWaiting ();
                string limit = Scene.GetLimit ().ToString () + ".";
                RequireMessageWindow ( _buildLimitExhaustedMessage + limit );
            }
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

    internal void HandlePrintPreparatinComplated ( )
    {
        HandleDialogClosing ();

        if ( _printAdjusting == null || _printAdjusting.IsCancelled )
        {
            return;
        }

        WaitForPrinting ();
    }

    internal void HandleDialogClosing ()
    {
        Waiting.Hide ();

        if ( _isBuildButtonTapped )
        {
            InduceBadgeBuilding ();
            _isBuildButtonTapped = false;
        }
    }

    private void InduceBadgeBuilding ()
    {
        if ( PersonChoosing.ChosenTemplate == null )
        {
            return;
        }

        Scene.Build ( PersonChoosing.ChosenTemplate.Name, PersonChoosing.ChosenPerson );

        if ( PersonChoosing.ChosenPerson == null )
        {
            Wait ();
        }
    }

    internal void Show ()
    {
        Scene.Refresh ();
    }

    internal void Wait ()
    {
        Waiting.ShowWithGif ();
        HasWaitingState = true;
    }

    internal void EndWaiting ()
    {
        Waiting.Hide ();
        BuildingIsPossible = true;
        HasWaitingState = false;
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

        IStorageFile? chosenFile = await FilePickerRequired?.Invoke ( options );
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
        Wait ();
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
        Waiting.Show ();
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

        Waiting.Show ();
        HasToShowTemplateErrors?.Invoke ( message, errorSource );
    }

    private void PreparePrinting ()
    {
        _printAdjusting = new ();
        Waiting.Show ();
        HasToPreparePrinting?.Invoke ( Scene.AllPages.Count, _printAdjusting );
    }

    private void WaitForPrinting ()
    {
        _printingShouldStart = true;
        Wait ();
    }

    internal void ProcessDocument ()
    {
        if ( SceneViewModel.EntireListBuildingIsChosen && HasWaitingState )
        {
            Scene.BuildAllBadges ();

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

    internal void CheckIfPersonSourceCorrect ()
    {
        PersonSource.DeclineKeepedFileIfIncorrect ();
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
        _isBuildButtonTapped = true;
        _isBuildButtonTapped = false;

        Build ();
    }

    private bool CanBuild ()
    {
        return BuildingIsPossible;
    }
}
