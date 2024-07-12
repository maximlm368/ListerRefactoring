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
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
using static Lister.ViewModels.MainViewModel;
using System.Reflection;
using Microsoft.Win32;
using ExtentionsAndAuxiliary;
using QuestPDF.Helpers;
using System;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Platform.Storage;
using System.Reactive.Linq;
using static SkiaSharp.HarfBuzz.SKShaper;


namespace Lister.ViewModels;

public class TemplateChoosingViewModel : ViewModelBase
{
    private static readonly string _fileIsOpenMessage = "Файл открыт в другом приложении, закройте его.";
    private ConverterToPdf _converter;
    private SceneViewModel _sceneVM;
    private PersonChoosingViewModel _personChoosingVM;
    private ZoomNavigationViewModel _zoomNavigationVM;

    private List <TemplateViewModel> tF;
    internal List <TemplateViewModel> Templates
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

            //if ( valueIsSuitable )
            //{
                
            //}
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
        List <TemplateName> templateNames = docAssembler.GetBadgeModels ();
        List <TemplateViewModel> templates = new List <TemplateViewModel> ();

        foreach ( TemplateName name   in   templateNames ) 
        {
            SolidColorBrush brush;

            if ( name.isFound ) 
            {
                brush = new SolidColorBrush (new Color (255, 0, 0, 0));
            }
            else 
            {
                brush = new SolidColorBrush (new Color (100, 0, 0, 0));
            }

            templates.Add (new TemplateViewModel (name, brush));
        }

        Templates = templates;
        _sceneVM = sceneViewModel;
        _converter = new ConverterToPdf ();
    }


    internal void BuildBadgess ()
    {
        if ( _zoomNavigationVM == null )
        {
            _zoomNavigationVM = App.services.GetRequiredService<ZoomNavigationViewModel> ();
        }

        Build ();
        _zoomNavigationVM.EnableZoom ();
        _zoomNavigationVM.SetEnablePageNavigation ();
        ClearIsEnable = true;
        SaveIsEnable = true;
        PrintIsEnable = true;
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
            BuildBadges ();
        }
        else if( _personChoosingVM.SinglePersonIsSelected )
        {
            BuildSingleBadge ();
        }
    }


    internal void BuildBadges ()
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
        _sceneVM.ClearAllPages ();
        ClearIsEnable = false;
        SaveIsEnable = false;
        PrintIsEnable = false;

        if ( _zoomNavigationVM == null )
        {
            _zoomNavigationVM = App.services.GetRequiredService<ZoomNavigationViewModel> ();
        }

        _zoomNavigationVM.DisableButtons ();

        if ( _sceneVM == null )
        {
            _sceneVM = App.services.GetRequiredService<SceneViewModel> ();
        }

        _sceneVM.ClearBuilt ();
    }


    internal void GeneratePdf ( )
    {
        List<FilePickerFileType> fileExtentions = [];
        fileExtentions.Add (FilePickerFileTypes.Pdf);
        FilePickerSaveOptions options = new ();
        options.Title = "Open Text File";
        options.FileTypeChoices = new ReadOnlyCollection <FilePickerFileType> (fileExtentions);
        Task<IStorageFile> chosenFile = MainWindow.CommonStorageProvider.SaveFilePickerAsync (options);

        TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

        chosenFile.ContinueWith
            (
               task =>
               {
                   if ( task.Result != null )
                   {
                       string result = task.Result.Path.ToString ();
                       result = result.Substring (8, result.Length - 8);
                       Task<bool> pdf = GeneratePdf (result);

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
                                   Process fileExplorer = new Process ();
                                   fileExplorer.StartInfo.FileName = "explorer.exe";
                                   result = result.ExtractPathWithoutFileName ();
                                   result = result.Replace ('/', '\\');
                                   fileExplorer.StartInfo.Arguments = result;
                                   fileExplorer.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                                   fileExplorer.Start ();
                               }
                           }, uiScheduler);
                   }
               }
            );
    }


    internal Task<bool> GeneratePdf ( string fileToSave )
    {
        List <PageViewModel> pages = GetAllPages ();
        Task<bool> task = new Task<bool> (() => { return _converter.ConvertToExtention (pages, fileToSave); });
        task.Start ();
        return task;
    }


    public void Print ()
    {
        List <PageViewModel> pages = GetAllPages ();
        string fileToSave = @"intermidiate.pdf";
        Task pdf = new Task (() => { _converter.ConvertToExtention (pages, fileToSave); });
        pdf.Start ();
        pdf.ContinueWith
               (
                  savingTask =>
                  {
                      int length = _converter.intermidiateFiles.Count;

                      ProcessStartInfo info = new ()
                      {
                          FileName = fileToSave,
                          Verb = "Print",
                          UseShellExecute = true,
                          ErrorDialog = false,
                          CreateNoWindow = true,
                          WindowStyle = ProcessWindowStyle.Minimized
                      };

                      Process.Start (info)?.WaitForExit (20_000);
                      File.Delete (fileToSave);
                  }
               );
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