using Avalonia.Platform.Storage;
using Lister.Desktop.CoreModelReflections;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Lister.Desktop.App;
using Lister.Desktop.Views.MainWindow.MainView;
using Lister.Desktop.Views.DialogMessageWindows.LargeMessage;
using Lister.Desktop.Views.DialogMessageWindows.Message;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog;
using Lister.Desktop.Views.DialogMessageWindows.PrintDialog.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.BuildButton.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.NavigationZoom.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonSource.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.Parts.Scene.ViewModel;
using Lister.Desktop.Views.WaitingView.ViewModel;

namespace Lister.Desktop.Views.MainWindow.MainView.ViewModel;

internal class MainViewModel : ReactiveObject
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
    private FilePickerSaveOptions _filePickerOptions;
    private FilePickerSaveOptions FilePickerOptions => _filePickerOptions ??= new()
    {
        Title = _saveTitle,
        FileTypeChoices =
        [
            new FilePickerFileType ("Источники данных")
            {
                Patterns = ["*.pdf"]
            }
        ],
        SuggestedFileName = _suggestedFileNames + GenerateNowDateString()
    };

    private string _osName;

    private PdfPrinter _pdfPrinter;
    private PrintDialog _printDialog;
    private PrintDialogViewModel _printDialogViewModel;
    private PersonSourceViewModel _personSourceViewModel;
    private PersonChoosingViewModel _personChoosingViewModel;
    private BadgesBuildingViewModel _badgesBuildingViewModel;
    private NavigationZoomViewModel _zoomNavigationViewModel;
    private SceneViewModel _sceneViewModel;
    private WaitingViewModel _waitingViewModel;
    private MainVieww _view;
    private bool _buildButtonIsTapped = false;
    private PrintAdjustingData _printAdjusting;

    internal string PdfFileName { get; private set; }


    internal MainViewModel(string osName, string suggestedFileNames, string saveTitle, string incorrectXSLX
                         , string buildingLimitIsExhaustedMessage, string fileIsOpenMessage, string fileIsTooBigMessage)
    {
        _osName = osName;
        _suggestedFileNames = suggestedFileNames;
        _saveTitle = saveTitle;
        _incorrectXSLX = incorrectXSLX;
        _buildingLimitIsExhaustedMessage = buildingLimitIsExhaustedMessage;
        _fileIsOpenMessage = fileIsOpenMessage;
        _fileIsTooBigMessage = fileIsTooBigMessage;

        _pdfPrinter = ListerApp.Services.GetRequiredService<PdfPrinter>();
        _printDialogViewModel = ListerApp.Services.GetRequiredService<PrintDialogViewModel>();
        _personChoosingViewModel = ListerApp.Services.GetRequiredService<PersonChoosingViewModel>();
        _personSourceViewModel = ListerApp.Services.GetRequiredService<PersonSourceViewModel>();
        _badgesBuildingViewModel = ListerApp.Services.GetRequiredService<BadgesBuildingViewModel>();
        _zoomNavigationViewModel = ListerApp.Services.GetRequiredService<NavigationZoomViewModel>();
        _sceneViewModel = ListerApp.Services.GetRequiredService<SceneViewModel>();
        _waitingViewModel = ListerApp.Services.GetRequiredService<WaitingViewModel>();

        _pdfPrinter.PropertyChanged += PrinterChanged;
        _printDialogViewModel.PropertyChanged += PrintDialogChanged;
        _personSourceViewModel.PropertyChanged += PersonSourceChanged;
        _personChoosingViewModel.PropertyChanged += PersonChoosingChanged;
        _badgesBuildingViewModel.PropertyChanged += BuildButtonTapped;
        _sceneViewModel.PropertyChanged += SceneChanged;
        _zoomNavigationViewModel.PropertyChanged += ZoomNavigationChanged;
    }


    private void PersonSourceChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "SourceFilePath")
        {
            PersonSourceViewModel personSource = (PersonSourceViewModel)sender;
            _personChoosingViewModel.SetPersonsFromFile( personSource.SourceFilePath );
        }
        else
        {
            if (MainVieww.Instance == null)
            {
                return;
            }

            if (args.PropertyName == "FileIsDeclined")
            {
                string message = _personSourceViewModel.FilePath + _incorrectXSLX;
                MessageDialog messegeDialog = new MessageDialog( MainVieww.Instance, message );
                WaitingViewModel waitingVM = ListerApp.Services.GetRequiredService<WaitingViewModel>();
                waitingVM.Darken();
                messegeDialog.ShowDialog( ListerApp.MainWindow );
            }
            else if (args.PropertyName == "FileIsTooBig")
            {
                string limit = _sceneViewModel.GetLimit().ToString() + ".";
                MessageDialog messegeDialog = new MessageDialog( MainVieww.Instance, _buildingLimitIsExhaustedMessage + limit );
                WaitingViewModel waitingVM = ListerApp.Services.GetRequiredService<WaitingViewModel>();
                waitingVM.Darken();

                if ( ListerApp.MainWindow != null ) 
                {
                    messegeDialog.ShowDialog ( ListerApp.MainWindow );
                }
            }
            else if (args.PropertyName == "FileIsOpen")
            {
                MessageDialog messegeDialog = new MessageDialog( MainVieww.Instance, _fileIsOpenMessage );
                WaitingViewModel waitingVM = ListerApp.Services.GetRequiredService<WaitingViewModel>();
                waitingVM.Darken();
                messegeDialog.ShowDialog( MainWin.Window );
            }
        }
    }


    private void PersonChoosingChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "AllAreReady")
        {
            PersonChoosingViewModel personChoosingViewModel = (PersonChoosingViewModel)sender;

            _badgesBuildingViewModel.TryToEnableBadgeCreation( personChoosingViewModel.AllAreReady );
        }
        else if (args.PropertyName == "SickTemplateIsSet")
        {
            if (_personChoosingViewModel.SickTemplateIsSet)
            {
                TemplateViewModel template = _personChoosingViewModel.ChosenTemplate;

                LargeMessageDialog messegeDialog =
                new LargeMessageDialog( MainVieww.Instance, template.CorrectnessMessage, template.SourcePath );

                _waitingViewModel.Darken();
                messegeDialog.ShowDialog( MainWin.Window );
                messegeDialog.Focusable = true;
                messegeDialog.Focus();
            }
        }
        else if (args.PropertyName == "FileNotFound")
        {
            _personSourceViewModel.EmptySourcePath();
        }
    }


    private void BuildButtonTapped(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "BuildButtonIsTapped")
        {
            if (string.IsNullOrWhiteSpace( _personChoosingViewModel.ChosenTemplate.Name ))
            {
                return;
            }

            bool shouldShowMessage = _personChoosingViewModel.ChosenTemplate.CorrectnessMessage != null
                                     && _personChoosingViewModel.ChosenTemplate.CorrectnessMessage.Count > 0;

            if (shouldShowMessage)
            {
                List<string> message = _personChoosingViewModel.ChosenTemplate.CorrectnessMessage;
                string path = _personChoosingViewModel.ChosenTemplate.SourcePath;

                LargeMessageDialog messegeDialog = new LargeMessageDialog( MainVieww.Instance, message, path );

                _waitingViewModel.Darken();
                messegeDialog.ShowDialog( MainWin.Window );
                messegeDialog.Focusable = true;
                messegeDialog.Focus();

                _buildButtonIsTapped = true;
            }
            else
            {
                InduceBadgeBuilding();
            }
        }
    }


    private void SceneChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "BadgesAreCleared")
        {
            _zoomNavigationViewModel.ToZeroState();
        }
        else if (args.PropertyName == "BuildingIsOccured")
        {
            if (_sceneViewModel.BuildingIsOccured)
            {
                EndWaiting();
                _zoomNavigationViewModel.EnableZoomIfPossible( _sceneViewModel.BuildingIsOccured );
                _zoomNavigationViewModel.SetEnablePageNavigation( _sceneViewModel.PageCount
                                                                , _sceneViewModel.VisiblePageNumber );
            }
            else
            {
                EndWaiting();
                string limit = _sceneViewModel.GetLimit().ToString() + ".";
                MessageDialog messegeDialog =
                                 new MessageDialog( MainVieww.Instance, _buildingLimitIsExhaustedMessage + limit );

                WaitingViewModel waitingVM = ListerApp.Services.GetRequiredService<WaitingViewModel>();
                waitingVM.Darken();
                messegeDialog.ShowDialog( MainWin.Window );
            }
        }
        else if (args.PropertyName == "EditIncorrectsIsSelected")
        {
            MainVieww mainView = MainVieww.Instance;
            mainView.EditIncorrectBadges( _sceneViewModel.ProcessableBadges, _sceneViewModel.VisiblePage );
        }
        else if (args.PropertyName == "PdfIsWanted")
        {
            PreparePdfGeneration();
        }
        else if (args.PropertyName == "PrintingIsWanted")
        {
            PreparePrinting();
        }
    }


    private void PrinterChanged (object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "PdfGenerationSuccesseeded")
        {
            EndWaiting();
            ShowInFileExplorer();
        }
        else if (args.PropertyName == "PrintingIsFinished" )
        {
            EndWaiting();
        }
    }


    private void PrintDialogChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "NeedClose")
        {
            _printDialog.Close();
            HandleDialogClosing();

            if (_printAdjusting.Cancelled)
            {
                return;
            }

            WaitForPrinting();
        }
    }


    private void ZoomNavigationChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "ZoomDegree")
        {
            _sceneViewModel.Zoom( _zoomNavigationViewModel.ZoomDegree );
        }
        else if (args.PropertyName == "VisiblePageNumber")
        {
            _sceneViewModel.ShowPageWithNumber( _zoomNavigationViewModel.VisiblePageNumber );
        }
    }


    internal void HandleDialogClosing()
    {
        _waitingViewModel.HandleDialogClosing();

        if (_buildButtonIsTapped)
        {
            InduceBadgeBuilding();
            _buildButtonIsTapped = false;
        }
    }


    internal void InduceBadgeBuilding()
    {
        _sceneViewModel.Build( _personChoosingViewModel.ChosenTemplate.Name, _personChoosingViewModel.ChosenPerson );

        if (_personChoosingViewModel.ChosenPerson == null)
        {
            SetWaitingUpdatingLayout();
        }
    }


    internal void RefreshAppearences()
    {
        _sceneViewModel.ResetIncorrects();
        _personChoosingViewModel.RefreshTemplateChoosingAppearence();
    }


    internal void PassView(MainVieww view)
    {
        _view = view;
        WaitingView.WaitingView wv = _view.waiting;
    }


    internal void SetWaitingUpdatingLayout()
    {
        _waitingViewModel.Show();
        MainViewIsWaiting = true;

        InWaitingState = true;
    }


    internal void EndWaiting()
    {
        _badgesBuildingViewModel.BuildingIsPossible = true;
        _waitingViewModel.Hide();
        MainViewIsWaiting = false;

        InWaitingState = false;
    }


    private async void PreparePdfGeneration()
    {
        List<FilePickerFileType> fileExtentions = [];
        fileExtentions.Add( FilePickerFileTypes.Pdf );
        FilePickerSaveOptions options = new();
        options.Title = _saveTitle;
        options.FileTypeChoices = new ReadOnlyCollection<FilePickerFileType>( fileExtentions );
        options.SuggestedFileName = _suggestedFileNames + GenerateNowDateString();
        IStorageFile chosenFile = await MainWin.CommonStorageProvider.SaveFilePickerAsync( options );

        bool savingCancelled = chosenFile == null;

        if (savingCancelled)
        {
            return;
        }

        PdfFileName = chosenFile.Path.ToString();
        int uriTypeLength = ListerApp.ResourceUriType.Length;
        PdfFileName = PdfFileName.Substring( uriTypeLength, PdfFileName.Length - uriTypeLength );

        FileInfo fileInfo = new FileInfo( PdfFileName );

        if (fileInfo.Exists)
        {
            try
            {
                FileStream stream = fileInfo.OpenWrite();
                stream.Close();
            }
            catch (IOException ex)
            {
                var messegeDialog = new MessageDialog( MainVieww.Instance, _fileIsOpenMessage );
                WaitingViewModel waitingVM = ListerApp.Services.GetRequiredService<WaitingViewModel>();
                waitingVM.Darken();
                messegeDialog.ShowDialog( MainWin.Window );

                return;
            }
        }

        _pdfGenerationShouldStart = true;
        SetWaitingUpdatingLayout();
    }


    private string GenerateNowDateString()
    {
        DateTime now = DateTime.Now;

        string day = now.Day.ToString();

        if (int.Parse( day ) < 10)
        {
            day = "0" + day;
        }

        string month = now.Month.ToString();

        if (int.Parse( month ) < 10)
        {
            month = "0" + month;
        }

        string hour = now.Hour.ToString();

        if (int.Parse( hour ) < 10)
        {
            hour = "0" + hour;
        }

        string minute = now.Minute.ToString();

        if (int.Parse( minute ) < 10)
        {
            minute = "0" + minute;
        }

        string result = "_" + day + month + now.Year.ToString() + "_" + hour + minute;
        return result;
    }


    private void ShowInFileExplorer()
    {
        if (_pdfPrinter.PdfGenerationSuccesseeded)
        {
            string procName = string.Empty;

            Process fileExplorer = new Process();
            string pdfFileName = ExtractPathWithoutFileName( PdfFileName );

            if (_osName == "Windows")
            {
                procName = "explorer.exe";
                pdfFileName = pdfFileName.Replace( '/', '\\' );
            }
            else if (_osName == "Linux")
            {
                procName = "nautilus";
            }

            fileExplorer.StartInfo.FileName = procName;
            fileExplorer.StartInfo.Arguments = pdfFileName;
            fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            fileExplorer.Start();
        }
        else
        {
            MessageDialog messegeDialog = new MessageDialog( MainVieww.Instance, _fileIsOpenMessage );
            WaitingViewModel waitingVM = ListerApp.Services.GetRequiredService<WaitingViewModel>();
            _waitingViewModel.Darken();
            messegeDialog.ShowDialog( MainWin.Window );
        }
    }


    private void PreparePrinting()
    {
        _printAdjusting = new();

        _printDialog = PrintDialog.GetPreparedDialog( _sceneViewModel.AllPages.Count, _printAdjusting );

        _waitingViewModel.Darken();
        _printDialog.ShowDialog( MainWin.Window );
    }


    private void WaitForPrinting()
    {
        _printingShouldStart = true;
        SetWaitingUpdatingLayout();
    }


    internal void LayoutUpdated()
    {
        if (SceneViewModel.EntireListBuildingIsChosen   &&   MainViewIsWaiting)
        {
            _sceneViewModel.BuildDuringWaiting();
            return;
        }
        else if (_pdfGenerationShouldStart)
        {
            _pdfGenerationShouldStart = false;
            _pdfPrinter.GeneratePdfDuringWaiting ( PdfFileName );
            return;
        }
        else if (_printingShouldStart)
        {
            _printingShouldStart = false;
            _pdfPrinter.PrintDuringWaiting( _printAdjusting, _osName );
            return;
        }
        else if (MainVieww.TappedGoToEditorButton == 1)
        {
            _view.SwitchToEditor();
        }
    }


    internal void OnLoaded()
    {
        _personSourceViewModel.DeclineKeepedFileIfIncorrect();
    }


    private string ExtractPathWithoutFileName(string wholePath)
    {
        var builder = new StringBuilder();
        string goalPath = string.Empty;

        for (var index = wholePath.Length - 1; index >= 0; index--)
        {
            bool fileNameIsAchieved = wholePath[index] == '/'
                                 || wholePath[index] == '\\';

            if (fileNameIsAchieved)
            {
                goalPath = wholePath.Substring( 0, index );
                break;
            }
        }

        return goalPath;
    }
}


