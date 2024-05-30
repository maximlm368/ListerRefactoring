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

    private List<FileInfo> templatesField;
    internal List<FileInfo> Templates
    {
        get
        {
            return templatesField;
        }
        set
        {
            this.RaiseAndSetIfChanged (ref templatesField, value, nameof (Templates));
        }
    }

    private FileInfo cT;
    internal FileInfo ChosenTemplate
    {
        set
        {
            bool valueIsSuitable = ( value.Name != string.Empty ) && ( value != null );

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


    public TemplateChoosingViewModel ( IUniformDocumentAssembler docAssembler, ContentAssembler.Size pageSize, 
                                       SceneViewModel sceneViewModel ) 
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

        string fileName = ChosenTemplate. FullName.ExtractFileNameFromPath ();
        _sceneVM.BuildBadges (fileName);
    }


    internal void BuildSingleBadge ()
    {
        if ( ChosenTemplate == null )
        {
            return;
        }

        string fileName = ChosenTemplate.FullName.ExtractFileNameFromPath ();
        _sceneVM.BuildSingleBadge (fileName);
    }


    internal void ClearAllPages ()
    {
        _sceneVM.ClearAllPages ();
    }


    internal Task<bool> GeneratePdf ( string fileToSave )
    {
        List <BadgeViewModel> badges = GetAllBadges ();
        Task<bool> task = new Task<bool> (() => { return converter.ConvertToExtention (badges, fileToSave); });
        task.Start ();
        return task;
    }


    public void Print ()
    {
        List <BadgeViewModel> badges = GetAllBadges ();
        string fileToSave = @"intermidiate.pdf";
        Task pdf = new Task (() => { converter.ConvertToExtention (badges, fileToSave); });
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


    private List <BadgeViewModel> GetAllBadges ()
    {
        List <BadgeViewModel> allBadges = _sceneVM.GetAllBadges();
        return allBadges;
    }
}