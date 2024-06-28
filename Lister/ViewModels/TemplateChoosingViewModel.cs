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


namespace Lister.ViewModels;

public class TemplateChoosingViewModel : ViewModelBase
{
    private ConverterToPdf converter;
    private SceneViewModel _sceneVM;
    private PersonChoosingViewModel _personChoosingVM;

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

            if ( valueIsSuitable )
            {
                this.RaiseAndSetIfChanged (ref cT, value, nameof (ChosenTemplate));
            }

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

        //if ( ! string.IsNullOrEmpty ( problems ) ) 
        //{
        //    int idOk = Winapi.MessageBox ( 0, problems, "", 0 );
        //}

        _sceneVM = sceneViewModel;
        converter = new ConverterToPdf ();
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


    internal void ClearAllPages ()
    {
        _sceneVM.ClearAllPages ();
    }


    internal Task<bool> GeneratePdf ( string fileToSave )
    {
        List <PageViewModel> pages = GetAllPages ();
        Task<bool> task = new Task<bool> (() => { return converter.ConvertToExtention (pages, fileToSave); });
        task.Start ();
        return task;
    }


    public void Print ()
    {
        List <PageViewModel> pages = GetAllPages ();
        string fileToSave = @"intermidiate.pdf";
        Task pdf = new Task (() => { converter.ConvertToExtention (pages, fileToSave); });
        pdf.Start ();
        pdf.ContinueWith
               (
                  savingTask =>
                  {
                      int length = converter.intermidiateFiles.Count;

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
            _personChoosingVM = App.services.GetRequiredService<PersonChoosingViewModel> ();
        }

        bool buildingIsPossible = ( ChosenTemplate != null );

        if ( buildingIsPossible )
        {
            BuildingIsPossible = _personChoosingVM.BuildingIsPossible;
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