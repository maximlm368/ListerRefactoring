using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;
using ContentAssembler;
using System.Net.Http;
using System.IO;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Interactivity;
using SkiaSharp;
using System.Security.Authentication.ExtendedProtection;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Splat;
using System.Collections.ObjectModel;
using Lister.ViewModels;
using ContentAssembler;
using DataGateway;
using Avalonia.Markup.Xaml.Templates;
using Lister.Extentions;
using Avalonia.Layout;

namespace Lister.Views;

public partial class MainView : UserControl
{
    private const double coefficient = 1.1;

    internal MainViewModel viewModel { get; private set; }
    private List<VMBadge> incorrectBadges;
    private Task<IReadOnlyList<IStorageFile>> ? personsFile;
    private bool insertionKindListIsDropped = false;
    private bool templateListIsDropped = false;
    private bool singlePersonIsSelected = false;
    private bool entirePersonListIsSelected = false;
    private bool personSelectionGotFocus = false;
    private bool textStackIsMesuared = false;
    private Window owner;


    public MainView (Window owner,  IUniformDocumentAssembler docAssembler)
    {
        InitializeComponent();
        this.owner = owner;
        pageBorder.Width = 794;
        pageBorder.Height = 1123;
        Size pageSize = new Size(pageBorder.Width, pageBorder.Height);
        this.DataContext = new MainViewModel(docAssembler, pageSize);
        this.viewModel = ( MainViewModel ) this.DataContext;
        this.incorrectBadges = new List<VMBadge> ();
        scroller.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
    }


    internal void SetWidth (int screenSize)
    {
        viewModel.SetWidth (screenSize);
    }


    internal void DropListGotFocus ( object sender, PointerEventArgs args )
    {
        if ( insertionKindListIsDropped )
        {
            personInsertionKindChoosing.ZIndex = 0;
            dropInsertionKindList.Opacity = 0;
            insertionKindListIsDropped = false;
        }
        else
        {
            personInsertionKindChoosing.ZIndex = 2;
            dropInsertionKindList.Opacity = 1;
            insertionKindListIsDropped = true;
        }
    }


    internal void GeneratePdf( object sender, TappedEventArgs args ) 
    {
        viewModel.GeneratePdf ();
    }


    internal void Print ( object sender, TappedEventArgs args )
    {
        viewModel.Print ();
    }


    internal void OpenLikeExcel ( object sender, TappedEventArgs args )
    {
        string processFilePath = "";
        string beingEditedInLikeExcelFilePath = personsSourceFile.Text;

        var os = Environment.OSVersion.VersionString;
        bool currentOsIsWindow = os.Contains ("Windows") || os.Contains ("windows");

        if ( currentOsIsWindow )
        {
            processFilePath = "c:\\Program Files\\Microsoft Office\\Office15\\EXCEL.EXE";
        }

        ProcessStartInfo procInfo = new ProcessStartInfo ();
        procInfo.FileName = processFilePath;
        procInfo.Arguments = beingEditedInLikeExcelFilePath;
        Process.Start (procInfo);
    }


    internal void ChooseFile ( object sender, TappedEventArgs args )
    {
        FilePickerOpenOptions options = new FilePickerOpenOptions ();
        List<FilePickerFileType> fileExtentions = new List<FilePickerFileType> ();
        fileExtentions.Add (new FilePickerFileType ("csv"));

        options.FileTypeFilter = new ReadOnlyCollection<FilePickerFileType> (fileExtentions);
        var window = TopLevel.GetTopLevel (this);
        personsFile = window.StorageProvider.OpenFilePickerAsync (new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();
        personsFile.ContinueWith
            (
               task =>
               {
                   string result = task.Result [0].Path.ToString ();
                   personsSourceFile.Text = result;
                   //mainViewModel.personsSourceFilePath = result;
               }
               , uiScheduler
            );
    }


    internal void AcceptEntirePersonList(object sender, TappedEventArgs args) 
    {
        entirePersonListIsSelected = true;
        singlePersonIsSelected = false;
    }


    internal void BuildBadges(object sender, TappedEventArgs args)
    {
        if ( singlePersonIsSelected ) 
        {
            viewModel.BuildSingleBadge();
        }
        if ( entirePersonListIsSelected )
        {
            viewModel.BuildBadges();
        }
    }


    internal void ClearBadges ( object sender, TappedEventArgs args )
    {
        viewModel.ClearAllPages ();
    }


    internal void DropDownOrPickUpListOfPersons(object sender, TappedEventArgs args)
    {
        if (insertionKindListIsDropped)
        {
            personInsertionKindChoosing.ZIndex = 0;
            dropInsertionKindList.Opacity = 0;
            insertionKindListIsDropped = false;
        }
        else
        {
            personInsertionKindChoosing.ZIndex = 2;
            dropInsertionKindList.Opacity = 1;
            insertionKindListIsDropped = true;
        }
    }


    internal void DropDownOrPickUpTemplatesList(object sender, TappedEventArgs args)
    {
        if (templateListIsDropped) 
        {
            dropTemplateList.Opacity = 0;
            templateListIsDropped = false;
        }
        else 
        {
            dropTemplateList.Opacity = 1;
            templateListIsDropped = true;
        }
    }


    internal void PickUpTemplatesList(object sender, TappedEventArgs args)
    {
        if (templateListIsDropped)
        {
            dropTemplateList.Opacity = 0;
            templateListIsDropped = false;
            ListBox listBox = (ListBox)sender;
            viewModel.chosenTemplate = (FileInfo) listBox.SelectedItem;
            
            if (viewModel.chosenTemplate == null) 
            { 
                startFace2.Text = "there is null"; 
            }
            else 
            {
                startFace2.Text = viewModel.chosenTemplate.Name;
            }
        }
    }


    internal void PickUpPersonList (object sender, TappedEventArgs args)
    {
        if (insertionKindListIsDropped)
        {
            personInsertionKindChoosing.ZIndex = 0;
            dropInsertionKindList.Opacity = 0;
            insertionKindListIsDropped = false;
            ListBox listBox = (ListBox) sender;
            viewModel.chosenPerson = (VMPerson) listBox.SelectedValue;
            singlePersonIsSelected = true;
            entirePersonListIsSelected = false;
        }
    }


    internal void EditIncorrectBadges (object sender, TappedEventArgs args) 
    {
        if (incorrectBadges.Count > 0) 
        {
            VMBadge beingEdited = incorrectBadges [0];
            owner.Content = new BadgeEditionView (beingEdited);
        }
    }


    internal void HandleTapingOnPersonSelectionTextBox ( object sender, TextChangedEventArgs args )
    {
        TextBox textBox = ( TextBox ) sender;

        if ( textBox.IsFocused )
        {
            string partOfName = textBox.Text.ToLower ();
            List<VMPerson> people = viewModel.people;
            ObservableCollection<VMPerson> visiblePeople = viewModel.visiblePeople;
            ObservableCollection<VMPerson> foundVisiblePeople = new ObservableCollection<VMPerson> ();

            for ( int personCounter = 0; personCounter < people.Count; personCounter++ )
            {
                string entireNameInLowCase = people [personCounter].entireName.ToLower ();

                if ( entireNameInLowCase.Contains (partOfName) )
                {
                    foundVisiblePeople.Add (people [personCounter]);
                }
            }

            viewModel.visiblePeople = foundVisiblePeople;
        }
    }


    internal void ToNextPage ( object sender, TappedEventArgs args )
    {
        viewModel.VisualiseNextPage ();
    }


    internal void ToPreviousPage ( object sender, TappedEventArgs args )
    {
        viewModel.VisualisePreviousPage ();
    }


    internal void ToLastPage ( object sender, TappedEventArgs args )
    {
        viewModel.VisualiseLastPage ();
    }


    internal void ToFirstPage ( object sender, TappedEventArgs args )
    {
        viewModel.VisualiseFirstPage ();
    }


    internal void ZoomOn ( object sender, TappedEventArgs args )
    {
        viewModel.ZoomOnDocument ();
    }


    internal void ZoomOut ( object sender, TappedEventArgs args )
    {
        viewModel.ZoomOutDocument ();
    }


    internal void SetNewScale ( object sender, TextChangedEventArgs args )
    {
        TextBox textBox = ( TextBox ) sender;
        string text = textBox.Text;
        bool textExists = (text != null) && (text != string.Empty);

        if ( textExists ) 
        {
            if ( text.Contains('%') ) 
            {
                text = text.Remove(text.Length - 1);
            }

            try 
            {
                int scale = Int32.Parse (text);

            }
            catch( FormatException ex ) 
            {}
        }
        
    }


    internal void StepOnPage ( object sender, TextChangedEventArgs args )
    {
        TextBox textBox = ( TextBox ) sender;
      
        try
        {
            int pageNumber = Int32.Parse (textBox.Text);
            viewModel.VisualisePageWithNumber (pageNumber);
        }
        catch (System.FormatException e){ }
    }


    internal void Seal ( object sender, TextChangedEventArgs args )
    {
        
    }
}


class DataOfTextBlock 
{
    internal double Height { get; set; }
    internal double Width { get; set; }
    internal string Text { get; set; }
}




