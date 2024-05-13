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
using static QuestPDF.Helpers.Colors;
using DynamicData;

namespace Lister.Views;

public partial class MainView : UserControl
{
    private const double coefficient = 1.1;

    internal MainViewModel viewModel { get; private set; }
    private List<VMBadge> incorrectBadges;
    //private Task<IReadOnlyList<IStorageFile>> ? personsFile;
    private bool personListIsDropped = false;
    private bool templateListIsDropped = false;
    private bool singlePersonIsSelected = false;
    private bool entirePersonListIsSelected = false;
    private bool templateIsSelected = false;
    private bool personSelectionGotFocus = false;
    private bool textStackIsMesuared = false;
    private Window owner;
    private Person selectedPerson;


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


    internal void GeneratePdf( object sender, TappedEventArgs args ) 
    {
        List<FilePickerFileType> fileExtentions = [];
        //fileExtentions.Add (new FilePickerFileType ("pdf"));
        fileExtentions.Add (FilePickerFileTypes.Pdf);

        FilePickerSaveOptions options = new ();
        options.Title = "Open Text File";
        options.FileTypeChoices = new ReadOnlyCollection<FilePickerFileType> (fileExtentions);
        var window = TopLevel.GetTopLevel (this);
        Task<IStorageFile> chosenFile = window.StorageProvider.SaveFilePickerAsync (options);
        TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ();

        chosenFile.ContinueWith
            (
               task =>
               {
                   if ( task.Result != null ) 
                   {
                       string result = task.Result.Path.ToString ();
                       result = result.Substring (8, result.Length - 8);
                       viewModel.GeneratePdf (result);
                   }
               }
               , uiScheduler
            );
    }


    internal void Print ( object sender, TappedEventArgs args )
    {
        viewModel.Print ();
    }


    internal void OpenEditor ( object sender, TappedEventArgs args )
    {
        string filePath = personsSourceFile.Text;

        if ( string.IsNullOrWhiteSpace(filePath) ) 
        {
            return;
        }

        ProcessStartInfo procInfo = new ProcessStartInfo () 
        {
            FileName = filePath,
            UseShellExecute = true
        };
        try 
        {
            Process.Start (procInfo);
        }
        catch ( System.ComponentModel.Win32Exception ex ) 
        {
        }
    }


    internal void ChooseFile ( object sender, TappedEventArgs args )
    {
        ChooseFile ( );
    }


    internal void ChooseFile ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();

        if ( key != "Q" )
        {
            return;
        }

        ChooseFile ();
    }
    //internal void ChooseFile ( object sender , KeyEventArgs args )
    //{
    //    string str = args.Key.ToString ( );
    //    int sas = 0;
    //    ChooseFile ( );
    //}


    private void ChooseFile ( )
    {
        FilePickerFileType csvFileType = new FilePickerFileType ( "Csv" )
        {
            Patterns = new [ ] { "*.csv" } ,
            AppleUniformTypeIdentifiers = new [ ] { "public.image" } ,
            MimeTypes = new [ ] { "image/*" }
        };

        List<FilePickerFileType> fileExtentions = [ ];
        fileExtentions.Add ( csvFileType );
        FilePickerOpenOptions options = new FilePickerOpenOptions ( );
        options.FileTypeFilter = new ReadOnlyCollection<FilePickerFileType> ( fileExtentions );
        options.Title = "Open Text File";
        options.AllowMultiple = false;
        var window = TopLevel.GetTopLevel ( this );
        Task<IReadOnlyList<IStorageFile>> chosenFile = null;
        chosenFile = window.StorageProvider.OpenFilePickerAsync ( options );
        TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext ( );

        chosenFile.ContinueWith
            (
               task =>
               {
                   if ( task.Result.Count > 0 )
                   {
                       string result = task.Result [ 0 ].Path.ToString ( );
                       MainViewModel vm = viewModel;
                       result = result.Substring ( 8 , result.Length - 8 );
                       vm.sourceFilePath = result;

                       if ( vm.sourceFilePath != string.Empty ) 
                       {
                           editSourceFile.IsEnabled = true;
                           setEntirePersonList.IsEnabled = true;
                       }
                   }
               }
               , uiScheduler
            );
    }


    internal void AcceptEntirePersonList(object sender, TappedEventArgs args) 
    {
        entirePersonListIsSelected = true;
        singlePersonIsSelected = false;
        EnableBadgeCreationButton ();
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

        zoomOn.IsEnabled = true;
        zoomOut.IsEnabled = true;

        SetEnablePageNavigation ();

        clearBadges.IsEnabled = true;
        save.IsEnabled = true;
        print.IsEnabled = true;
    }


    private void SetEnablePageNavigation () 
    {
        int pageCount = viewModel.GetPageCount ();

        if ( pageCount > 1 )
        {
            if ( ( viewModel.visiblePageNumber > 1 ) && ( viewModel.visiblePageNumber == pageCount ) )
            {
                firstPage.IsEnabled = true;
                previousPage.IsEnabled = true;
                nextPage.IsEnabled = false;
                lastPage.IsEnabled = false;
            }
            else if ( ( viewModel.visiblePageNumber > 1 ) && ( viewModel.visiblePageNumber < pageCount ) )
            {
                firstPage.IsEnabled = true;
                previousPage.IsEnabled = true;
                nextPage.IsEnabled = true;
                lastPage.IsEnabled = true;
            }
            else if ( ( viewModel.visiblePageNumber == 1 ) && ( pageCount == 1 ) )
            {
                firstPage.IsEnabled = false;
                previousPage.IsEnabled = false;
                nextPage.IsEnabled = false;
                lastPage.IsEnabled = false;
            }
            else if ( ( viewModel.visiblePageNumber == 1 ) && ( pageCount > 1 ) )
            {
                firstPage.IsEnabled = false;
                previousPage.IsEnabled = false;
                nextPage.IsEnabled = true;
                lastPage.IsEnabled = true;
            }
        }
    }


    internal void ClearBadges ( object sender, TappedEventArgs args )
    {
        viewModel.ClearAllPages ();
        zoomOn.IsEnabled = false;
        zoomOut.IsEnabled = false;
        clearBadges.IsEnabled = false;
        save.IsEnabled = false;
        print.IsEnabled = false;
        firstPage.IsEnabled = false;
        previousPage.IsEnabled = false;
        nextPage.IsEnabled = false;
        lastPage.IsEnabled = false;
    }


    internal void DropDownOrPickUpPersonList ( object sender , TappedEventArgs args )
    {
        if ( personListIsDropped )
        {
            personList.Height = 0;
            personListIsDropped = false;
        }
        else
        {
            personList.Height = 154;
            personListIsDropped = true;
            personList.Focus ( NavigationMethod.Tab );
        }
    }


    internal void DropDownOrPickUpPersonListViaKey ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();
        bool keyIsNotEnter = key != "Return";

        if ( keyIsNotEnter )
        {
            return;
        }

        if ( personListIsDropped )
        {
            if ( selectedPerson != null )
            {
                personTyping.Text = selectedPerson.StringPresentation;
            }

            personList.Height = 0;
            personListIsDropped = false;
        }
        else
        {
            personList.Height = 154;
            personListIsDropped = true;
        }
    }
    //internal void DropDownOrPickUpPersonListViaKey ( object sender , KeyEventArgs args )
    //{
    //    string key = args.Key.ToString ();
    //    bool keyIsNotEnter = key != "Return";

    //    if(keyIsNotEnter)
    //    {
    //        return;
    //    }

    //    if ( personListIsDropped )
    //    {
    //        personList.Height = 0;
    //        personListIsDropped = false;
    //    }
    //    else
    //    {
    //        personList.Height = 150;
    //        personListIsDropped = true;
    //        //personList.Focus ( NavigationMethod.Tab );
    //    }
    //}


    internal void ClosePersonList ( object sender , TappedEventArgs args )
    {
        if ( personListIsDropped )
        {
            personList.Height = 0;
            personListIsDropped = false;
        }
    }


    internal void HandleTemplateChoosing ( object sender, SelectionChangedEventArgs args )
    {
        ComboBox comboBox = ( ComboBox ) sender;
        viewModel.chosenTemplate = ( FileInfo ) comboBox.SelectedItem;
        templateIsSelected = true;
        EnableBadgeCreationButton ();
    }


    internal void HandlePersonChoosingViaTapping ( object sender , TappedEventArgs args )
    {
        if ( personListIsDropped )
        {
            if ( selectedPerson != null )
            {
                personTyping.Text = selectedPerson.StringPresentation;
            }

            personList.Height = 0;
            personListIsDropped = false;
            singlePersonIsSelected = true;
            entirePersonListIsSelected = false;
            EnableBadgeCreationButton ();
        }
    }


    internal void HandleSelectionChanged ( object sender, SelectionChangedEventArgs args )
    {
        Person previousSelectedPerson = selectedPerson;
        selectedPerson = ( Person ) personList.SelectedItem;
        singlePersonIsSelected = true;
        entirePersonListIsSelected = false;
    }


    private void EnableBadgeCreationButton () 
    {
        bool itsTimeToEnable = ( singlePersonIsSelected || entirePersonListIsSelected ) && templateIsSelected;
        if ( itsTimeToEnable ) 
        {
            buildBadges.IsEnabled = true;       
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


    internal void HandlePersonListReduction ( object sender, TextChangedEventArgs args )
    {
        TextBox textBox = ( TextBox ) sender;
        string partOfName = textBox.Text.ToLower ();
        List<Person> people = viewModel.people;
        ObservableCollection<Person> foundVisiblePeople = new ObservableCollection<Person> ();

        foreach ( Person person   in   people)
        {
            if ( person.StringPresentation.ToLower () == partOfName )
            {
                RecoverVisiblePeople ();
                return;
            }

            string entireName = person.StringPresentation;
            string entireNameInLowCase = entireName.ToLower ();

            if ( entireNameInLowCase.Contains (partOfName) && entireNameInLowCase != partOfName )
            {
                foundVisiblePeople.Add (person);
            }
        }

        viewModel.visiblePeople = foundVisiblePeople;
    }
    //internal void HandlePersonListReduction ( object sender, TextChangedEventArgs args )
    //{
    //    TextBox textBox = ( TextBox ) sender;

    //    if ( textBox.IsFocused )
    //    {
    //        string partOfName = textBox.Text.ToLower ();
    //        List<Person> people = viewModel. people;
    //        ObservableCollection<Person> visiblePeople = viewModel. visiblePeople;
    //        ObservableCollection<Person> foundVisiblePeople = new ObservableCollection<Person> ();

    //        for ( int personCounter = 0;   personCounter < people.Count;   personCounter++ )
    //        {
    //            Person person = people [personCounter];
    //            string entireName = person.StringPresentation;
    //            string entireNameInLowCase = entireName.ToLower ();

    //            if ( entireNameInLowCase.Contains (partOfName) )
    //            {
    //                foundVisiblePeople.Add (people [personCounter]);
    //            }
    //        }

    //        viewModel. visiblePeople = foundVisiblePeople;
    //    }
    //}


    private void RecoverVisiblePeople ()
    {
        List<Person> people = viewModel. people;
        viewModel. visiblePeople = new ();
        viewModel.visiblePeople.AddRange (people);
    }


    internal void ToNextPage ( object sender, TappedEventArgs args )
    {
        viewModel.VisualiseNextPage ();
        SetEnablePageNavigation ();
    }


    internal void ToPreviousPage ( object sender, TappedEventArgs args )
    {
        viewModel.VisualisePreviousPage ();
        SetEnablePageNavigation ();
    }


    internal void ToLastPage ( object sender, TappedEventArgs args )
    {
        viewModel.VisualiseLastPage ();
        SetEnablePageNavigation ();
    }


    internal void ToFirstPage ( object sender, TappedEventArgs args )
    {
        viewModel.VisualiseFirstPage ();
        SetEnablePageNavigation ();
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
            int pageNumber = (int) UInt32.Parse (textBox.Text);
            viewModel.VisualisePageWithNumber (pageNumber);
        }
        catch (System.FormatException e){ }
    }


    internal void Seal ( object sender, TextChangedEventArgs args )
    {
        
    }
}






