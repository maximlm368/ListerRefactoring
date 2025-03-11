﻿using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using View.App;
using View.CoreModelReflection;
using View.DialogMessageWindows.LargeMessage;
using View.DialogMessageWindows.Message;
using View.DialogMessageWindows.PrintDialog;
using View.DialogMessageWindows.PrintDialog.ViewModel;
using View.MainWindow.MainView.Parts.BuildButton.ViewModel;
using View.MainWindow.MainView.Parts.NavigationZoom.ViewModel;
using View.MainWindow.MainView.Parts.PersonChoosing.ViewModel;
using View.MainWindow.MainView.Parts.PersonSource.ViewModel;
using View.MainWindow.MainView.Parts.Scene.ViewModel;
using View.WaitingView.ViewModel;

namespace View.MainWindow.MainView.ViewModel;

public class MainViewModel : ReactiveObject
{
    private static bool _inWaitingState;
    public static bool InWaitingState
    {
        get { return _inWaitingState; }
        private set { _inWaitingState = value; }
    }

    private readonly string _suggestedFileNames;
    private readonly string _saveTitle;
    private readonly string _incorrectXSLX;
    private readonly string _fileIsTooBigMessage;
    private readonly string _buildingLimitIsExhaustedMessage;
    private readonly string _fileIsOpenMessage;

    public static bool MainViewIsWaiting { get; private set; }
    public static bool PrintingShouldStart { get; set; }
    internal string PdfFileName { get; private set; }
    public static bool PdfGenerationShouldStart { get; set; }

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
    private MainView _view;

    private bool _buildButtonIsTapped = false;
    private PrintAdjustingData _printAdjusting;


    public MainViewModel(string osName, string suggestedFileNames, string saveTitle, string incorrectXSLX
                         , string buildingLimitIsExhaustedMessage, string fileIsOpenMessage, string fileIsTooBigMessage)
    {
        _osName = osName;
        _suggestedFileNames = suggestedFileNames;
        _saveTitle = saveTitle;
        _incorrectXSLX = incorrectXSLX;
        _buildingLimitIsExhaustedMessage = buildingLimitIsExhaustedMessage;
        _fileIsOpenMessage = fileIsOpenMessage;
        _fileIsTooBigMessage = fileIsTooBigMessage;

        _pdfPrinter = ListerApp.services.GetRequiredService<PdfPrinter>();
        _printDialogViewModel = ListerApp.services.GetRequiredService<PrintDialogViewModel>();
        _personChoosingViewModel = ListerApp.services.GetRequiredService<PersonChoosingViewModel>();
        _personSourceViewModel = ListerApp.services.GetRequiredService<PersonSourceViewModel>();
        _badgesBuildingViewModel = ListerApp.services.GetRequiredService<BadgesBuildingViewModel>();
        _zoomNavigationViewModel = ListerApp.services.GetRequiredService<NavigationZoomViewModel>();
        _sceneViewModel = ListerApp.services.GetRequiredService<SceneViewModel>();
        _waitingViewModel = ListerApp.services.GetRequiredService<WaitingViewModel>();

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
            if (MainView.Instance == null)
            {
                return;
            }

            if (args.PropertyName == "FileIsDeclined")
            {
                string message = _personSourceViewModel.FilePath + _incorrectXSLX;
                MessageDialog messegeDialog = new MessageDialog( MainView.Instance, message );

                WaitingViewModel waitingVM = ListerApp.services.GetRequiredService<WaitingViewModel>();
                waitingVM.Darken();
                messegeDialog.ShowDialog( ListerApp.MainWindow );
            }
            else if (args.PropertyName == "FileIsTooBig")
            {
                string limit = _sceneViewModel.GetLimit().ToString() + ".";
                MessageDialog messegeDialog = new MessageDialog( MainView.Instance, _buildingLimitIsExhaustedMessage + limit );

                WaitingViewModel waitingVM = ListerApp.services.GetRequiredService<WaitingViewModel>();
                waitingVM.Darken();
                messegeDialog.ShowDialog( ListerApp.MainWindow );
            }
            else if (args.PropertyName == "FileIsOpen")
            {
                MessageDialog messegeDialog = new MessageDialog( MainView.Instance, _fileIsOpenMessage );
                WaitingViewModel waitingVM = ListerApp.services.GetRequiredService<WaitingViewModel>();
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
                new LargeMessageDialog( MainView.Instance, template.CorrectnessMessage, template.SourcePath );

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

                LargeMessageDialog messegeDialog = new LargeMessageDialog( MainView.Instance, message, path );

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
                                 new MessageDialog( MainView.Instance, _buildingLimitIsExhaustedMessage + limit );

                WaitingViewModel waitingVM = ListerApp.services.GetRequiredService<WaitingViewModel>();
                waitingVM.Darken();
                messegeDialog.ShowDialog( MainWin.Window );
            }
        }
        else if (args.PropertyName == "EditIncorrectsIsSelected")
        {
            MainView mainView = MainView.Instance;
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


    internal void PassView(MainView view)
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
                var messegeDialog = new MessageDialog( MainView.Instance, _fileIsOpenMessage );
                WaitingViewModel waitingVM = ListerApp.services.GetRequiredService<WaitingViewModel>();
                waitingVM.Darken();
                messegeDialog.ShowDialog( MainWin.Window );

                return;
            }
        }

        PdfGenerationShouldStart = true;
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
            MessageDialog messegeDialog = new MessageDialog( MainView.Instance, _fileIsOpenMessage );
            WaitingViewModel waitingVM = ListerApp.services.GetRequiredService<WaitingViewModel>();
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
        PrintingShouldStart = true;
        SetWaitingUpdatingLayout();
    }


    internal void LayoutUpdated()
    {
        if (SceneViewModel.EntireListBuildingIsChosen   &&   MainViewIsWaiting)
        {
            _sceneViewModel.BuildDuringWaiting();
            return;
        }
        else if (PdfGenerationShouldStart)
        {
            PdfGenerationShouldStart = false;
            _pdfPrinter.GeneratePdfDuringWaiting ( PdfFileName );
            return;
        }
        else if (PrintingShouldStart)
        {
            PrintingShouldStart = false;
            _pdfPrinter.PrintDuringWaiting( _printAdjusting, _osName );
            return;
        }
        else if (MainView.TappedGoToEditorButton == 1)
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


