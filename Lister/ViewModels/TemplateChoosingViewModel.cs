using Avalonia;
using ContentAssembler;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Controls;
using Avalonia.Media;
using System.Windows.Input;
using System.Text;
using System.Net.WebSockets;
using System.ComponentModel;
using Lister.Views;
using Lister.Extentions;
using System.Collections.ObjectModel;
using static QuestPDF.Helpers.Colors;
using Avalonia.Controls.Shapes;
using DynamicData;
using ReactiveUI;
using Avalonia.Input;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Buffers.Binary;
using System.Reflection;
using Microsoft.Win32;
using ExtentionsAndAuxiliary;
using QuestPDF.Helpers;
using System;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Platform.Storage;
using System.Reactive.Linq;
using Avalonia.Remote.Protocol.Viewport;
using MessageBox.Avalonia.Views;


namespace Lister.ViewModels;

public class TemplateChoosingViewModel : ViewModelBase
{
    private static readonly string _fileIsOpenMessage = "Файл открыт в другом приложении, закройте его.";
    private static readonly string _title = "Ошибка";
    private static readonly string _saveTitle = "Сохранение документа";
    private static readonly string _suggestedFileNames = "MyPdf";
    private TemplateChoosingUserControl _view;
    private ConverterToPdf _converter;
    private SceneViewModel _sceneVM;
    private PersonChoosingViewModel _personChoosingVM;
    private ZoomNavigationViewModel _zoomNavigationVM;
    private List <TemplateName> _templateNames;

    private ObservableCollection <TemplateViewModel> tF;
    internal ObservableCollection <TemplateViewModel> Templates
    {
        get
        {
            return tF;
        }
        set
        {
            this.RaiseAndSetIfChanged (ref tF, value, nameof (Templates));
        }
    }

    private TemplateViewModel cT;
    internal TemplateViewModel ChosenTemplate
    {
        set
        {
            bool valueIsSuitable = ( value != null )   &&   ( value.Name != string.Empty );

            this.RaiseAndSetIfChanged (ref cT, value, nameof (ChosenTemplate));
            TryToEnableBadgeCreationButton ();
        }
        get
        {
            return cT;
        }
    }

    private bool isO;
    internal bool IsOpen
    {
        set
        {
            this.RaiseAndSetIfChanged (ref isO, value, nameof (isO));
        }
        get
        {
            return isO;
        }
    }

    private bool isC;
    internal bool BuildingIsPossible
    {
        set
        {
            if ( value ) 
            {
                if (ChosenTemplate == null) 
                {
                    value = false;
                }
            }

            this.RaiseAndSetIfChanged (ref isC, value, nameof (BuildingIsPossible));
        }
        get
        {
            return isC;
        }
    }

    private bool cE;
    internal bool ClearIsEnable
    {
        set
        {
            this.RaiseAndSetIfChanged (ref cE, value, nameof (ClearIsEnable));
        }
        get
        {
            return cE;
        }
    }

    private bool sE;
    internal bool SaveIsEnable
    {
        set
        {
            this.RaiseAndSetIfChanged (ref sE, value, nameof (SaveIsEnable));
        }
        get
        {
            return sE;
        }
    }

    private bool pE;
    internal bool PrintIsEnable
    {
        set
        {
            this.RaiseAndSetIfChanged (ref pE, value, nameof (PrintIsEnable));
        }
        get
        {
            return pE;
        }
    }


    public TemplateChoosingViewModel ( IUniformDocumentAssembler docAssembler, SceneViewModel sceneViewModel )
    {
        _templateNames = docAssembler.GetBadgeModels ();
        _sceneVM = sceneViewModel;
        _converter = new ConverterToPdf ();
    }


    internal void PassView ( TemplateChoosingUserControl view )
    {
        _view = view;
    }


    internal void ChangeAccordingTheme ( string theme )
    {
        SolidColorBrush foundColor = new SolidColorBrush (MainWindow.black);
        SolidColorBrush unfoundColor = new SolidColorBrush (new Color (100, 0, 0, 0));

        if ( theme == "Dark" ) 
        {
            foundColor  = new SolidColorBrush (MainWindow.white);
            unfoundColor = new SolidColorBrush (new Color (100, 255, 255, 255));
        }

        ObservableCollection <TemplateViewModel> templates = new ();

        foreach ( TemplateName name   in   _templateNames )
        {
            SolidColorBrush brush;

            if ( name.isFound )
            {
                brush = foundColor;
            }
            else
            {
                brush = unfoundColor;
            }

            templates.Add (new TemplateViewModel (name, brush));
        }

        Templates = templates;
    }


    internal void BuildBadges ()
    {
        if ( _zoomNavigationVM == null )
        {
            _zoomNavigationVM = App.services.GetRequiredService<ZoomNavigationViewModel> ();
        }

        Build ();
    }


    internal void Build ()
    {
        if ( ChosenTemplate == null )
        {
            return;
        }

        if ( _personChoosingVM == null )
        {
            _personChoosingVM = App.services.GetRequiredService<PersonChoosingViewModel> ();
        }

        if ( _personChoosingVM.EntirePersonListIsSelected ) 
        {
            _view.SetCursorWait ();
            BuildAllBadges ();
        }
        else if( _personChoosingVM.SinglePersonIsSelected )
        {
            BuildSingleBadge ();
        }

        _view.SetCursorArrow ();
        EnableButtons ();
        _zoomNavigationVM.EnableZoom ();
        _zoomNavigationVM.SetEnablePageNavigation ();
    }


    internal void BuildAllBadges ()
    {
        if ( ChosenTemplate == null )
        {
            return;
        }

        _sceneVM.BuildBadges (ChosenTemplate. Name);
    }


    internal void BuildSingleBadge ()
    {
        if ( ChosenTemplate == null )
        {
            return;
        }

        _sceneVM.BuildSingleBadge (ChosenTemplate. Name);
    }


    internal void ClearBadges ( )
    {
        if ( ! PreventCommandExecution() )
        {
            return;
        }

        if ( _sceneVM == null )
        {
            _sceneVM = App.services.GetRequiredService<SceneViewModel> ();
        }

        _sceneVM.ClearAllPages ();
        DisableButtons();

        if ( _zoomNavigationVM == null )
        {
            _zoomNavigationVM = App.services.GetRequiredService<ZoomNavigationViewModel> ();
        }

        _zoomNavigationVM.DisableButtons ();
    }


    internal void DisableButtons ()
    {
        ClearIsEnable = false;
        SaveIsEnable = false;
        PrintIsEnable = false;
    }


    internal void EnableButtons ()
    {
        ClearIsEnable = true;
        SaveIsEnable = true;
        PrintIsEnable = true;
    }


    internal void GeneratePdff ()
    {
        List<FilePickerFileType> fileExtentions = [];
        fileExtentions.Add (FilePickerFileTypes.Pdf);
        FilePickerSaveOptions options = new ();
        options.Title = _saveTitle;
        options.FileTypeChoices = new ReadOnlyCollection<FilePickerFileType> (fileExtentions);
        options.SuggestedFileName = _suggestedFileNames;
        Task<IStorageFile> chosenFile = MainWindow.CommonStorageProvider.SaveFilePickerAsync (options);

        TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

        chosenFile.ContinueWith
            (
               task =>
               {
                   if ( task.Result != null )
                   {
                       string result = task.Result.Path.ToString ();
                       int uriTypeLength = App.ResourceUriType. Length;
                       result = result.Substring (uriTypeLength, result.Length - uriTypeLength);

                       Task<bool> pdf = GeneratePdff (result);

                       pdf.ContinueWith
                           (
                           task =>
                           {
                               if ( pdf.Result == false )
                               {
                                   var messegeDialog = new MessageDialog ();
                                   messegeDialog.Message = _fileIsOpenMessage;
                                   messegeDialog.ShowDialog (MainWindow._mainWindow);
                               }
                               else
                               {
                                   if ( App.OsName == "Windows" )
                                   {
                                       Process fileExplorer = new Process ();
                                       fileExplorer.StartInfo.FileName = "explorer.exe";
                                       result = ExtractPathWithoutFileName ( result );
                                       result = result.Replace ('/', '\\');
                                       fileExplorer.StartInfo.Arguments = result;
                                       fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                                       fileExplorer.Start ();
                                   }
                                   else if ( App.OsName == "Linux" ) 
                                   {
                                       Process fileExplorer = new Process ();
                                       fileExplorer.StartInfo.FileName = "Nautilus.bin";
                                       result = ExtractPathWithoutFileName ( result );
                                       fileExplorer.StartInfo.Arguments = result;
                                       fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                                       fileExplorer.Start ();
                                   }
                               }
                           }, uiScheduler);
                   }
               }
            );
    }


    internal void GeneratePdf ()
    {
        List<FilePickerFileType> fileExtentions = [];
        fileExtentions.Add (FilePickerFileTypes.Pdf);
        FilePickerSaveOptions options = new ();
        options.Title = _saveTitle;
        options.FileTypeChoices = new ReadOnlyCollection<FilePickerFileType> (fileExtentions);
        options.SuggestedFileName = _suggestedFileNames + GenerateNowDateString ();
        Task <IStorageFile> chosenFile = MainWindow.CommonStorageProvider.SaveFilePickerAsync (options);

        TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

        chosenFile.ContinueWith
            (
               task =>
               {
                   if ( task.Result != null )
                   {
                       string result = task.Result.Path.ToString ();
                       int uriTypeLength = App.ResourceUriType.Length;
                       result = result.Substring (uriTypeLength, result.Length - uriTypeLength);
                      
                       bool pdf = GeneratePdf (result);

                       if ( pdf == false )
                       {
                           var messegeDialog = new MessageDialog ();
                           messegeDialog.Message = _fileIsOpenMessage;
                           messegeDialog.ShowDialog (MainWindow._mainWindow);
                       }
                       else
                       {
                           if ( App.OsName == "Windows" )
                           {
                               Process fileExplorer = new Process ();
                               fileExplorer.StartInfo.FileName = "explorer.exe";
                               result = ExtractPathWithoutFileName (result);
                               result = result.Replace ('/', '\\');
                               fileExplorer.StartInfo.Arguments = result;
                               fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                               fileExplorer.Start ();
                           }
                           else if ( App.OsName == "Linux" )
                           {
                               Process fileExplorer = new Process ();
                               fileExplorer.StartInfo.FileName = "nautilus";
                               result = ExtractPathWithoutFileName (result);
                               fileExplorer.StartInfo.Arguments = result;
                               fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                               fileExplorer.Start ();
                           }
                       }
                   }
               }, uiScheduler);
    }


    private string GenerateNowDateString ( )
    {
        DateTime now = DateTime.Now;
        string result = " " + now.Day.ToString () + " " + now.Month.ToString () + " " + now.Year.ToString () + "_" 
                      + now.Hour.ToString () + " " + now.Minute.ToString ();
        return result;
    }


    internal Task<bool> GeneratePdff ( string fileToSave )
    {
        List <PageViewModel> pages = GetAllPages ();
        Task<bool> task = new Task<bool> (() => { return _converter.ConvertToExtention (pages, fileToSave); });
        task.Start ();
        return task;
    }


    internal bool GeneratePdf ( string fileToSave )
    {
        List <PageViewModel> pages = GetAllPages ();
        bool result = _converter.ConvertToExtention (pages, fileToSave);
        return result;
    }


    public void Print ()
    {
        List <PageViewModel> pages = GetAllPages ();
        string fileToSave = @"intermidiate.pdf";
        Task pdf = new Task (() => { _converter.ConvertToExtention (pages, fileToSave); });
        pdf.Start ();
        Task printTask = pdf.ContinueWith
               (
                  savingTask =>
                  {
                      if ( App.OsName == "Windows" )
                      {
                          int length = _converter.intermidiateFiles.Count;
                          IStorageItem sItem = null;

                          ProcessStartInfo info = new ()
                          {
                              FileName = fileToSave,
                              Verb = "Print",
                              UseShellExecute = true,
                              ErrorDialog = false,
                              CreateNoWindow = true,
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


    private bool PreventCommandExecution ()
    {
        bool result = true;

        if ( MainWindow.EventTimer == null )
        {
            return false;
        }

        MainWindow.EventTimer.Stop ();
        TimeSpan expandTime = MainWindow.EventTimer.Elapsed;

        if ( expandTime.Milliseconds < 50 )
        {
            return false;
        }

        return result;
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


    private List <PageViewModel> GetAllPages ()
    {
        List<PageViewModel> pages = _sceneVM.GetAllPages ();
        List<PageViewModel> dimensionallyOriginals = new List<PageViewModel> ();

        foreach ( PageViewModel page in pages )
        {
            dimensionallyOriginals.Add (page.GetDimendionalOriginal ());
        }

        return dimensionallyOriginals;
    }


    private void TryToEnableBadgeCreationButton ()
    {
        if ( _personChoosingVM == null )
        {
            _personChoosingVM = App.services.GetRequiredService <PersonChoosingViewModel> ();
        }

        bool buildingIsPossible = ( ChosenTemplate != null )   &&   _personChoosingVM.BuildingIsPossible;

        if ( buildingIsPossible )
        {
            BuildingIsPossible = true;
        }
        else 
        {
            BuildingIsPossible = false;
        }
    }
}



public class TemplateViewModel : ViewModelBase
{
    private TemplateName TemplateName { get; set; }

    private string name;
    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            this.RaiseAndSetIfChanged (ref name, value, nameof (Name));
        }
    }

    private SolidColorBrush cL;
    public SolidColorBrush Color
    {
        get
        {
            return cL;
        }
        set
        {
            this.RaiseAndSetIfChanged (ref cL, value, nameof (Color));
        }
    }


    public TemplateViewModel ( TemplateName templateName, SolidColorBrush color )
    {
        TemplateName = templateName;
        Name = templateName.name;
        Color = color;
    }
}


//_chosen.Background = new SolidColorBrush (new Color (255, 255, 255, 255));