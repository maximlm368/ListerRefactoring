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


namespace Lister.ViewModels;

public class TemplateChoosingViewModel : ViewModelBase
{
    private ConverterToPdf converter;
    private SceneViewModel _sceneVM;

    private List<string> tF;
    internal List<string> Templates
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

    private string cT;
    internal string ChosenTemplate
    {
        set
        {
            bool valueIsSuitable = ( value != null )   &&   ( value != string.Empty );

            if ( valueIsSuitable )
            {
                this.RaiseAndSetIfChanged (ref cT, value, nameof (ChosenTemplate));
            }
        }
        get
        {
            return cT;
        }
    }


    public TemplateChoosingViewModel ( IUniformDocumentAssembler docAssembler, SceneViewModel sceneViewModel ) 
    {
        Templates = docAssembler.GetBadgeModels ();
        _sceneVM = sceneViewModel;
        converter = new ConverterToPdf ();
    }


    internal void BuildBadges ()
    {
        if ( ChosenTemplate == null )
        {
            return;
        }

        _sceneVM.BuildBadges (ChosenTemplate);
    }


    internal void BuildSingleBadge ()
    {
        if ( ChosenTemplate == null )
        {
            return;
        }

        _sceneVM.BuildSingleBadge (ChosenTemplate);
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
}