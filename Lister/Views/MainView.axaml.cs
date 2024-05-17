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
using System.Runtime.InteropServices;
using ExtentionsAndAuxiliary;

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
    private bool openedViaButton = false;
    private bool cursorIsOverPersonList = false;
    private bool selectionIsChanged = false;
    private Window owner;
    private Person selectedPerson;
    private ushort maxScalability;
    private ushort minScalability;
    private short scalabilityDepth;
    private readonly short scalabilityStep;
    private short maxDepth;
    private short minDepth;
    private double personContainerHeight;
    private double maxPersonListHeight;
    private double minPersonListHeight;


    public MainView ( Window owner, IUniformDocumentAssembler docAssembler )
    {
        InitializeComponent ();
        this.owner = owner;
        pageBorder.Width = 794;
        pageBorder.Height = 1123;
        Size pageSize = new Size (pageBorder.Width, pageBorder.Height);
        this.DataContext = new MainViewModel (docAssembler, pageSize);
        this.viewModel = ( MainViewModel ) this.DataContext;
        this.incorrectBadges = new List<VMBadge> ();
        scroller.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto;
        scalabilityDepth = 0;
        scalabilityStep = 25;
        maxDepth = 5;
        minDepth = -5;
        personContainerHeight = 38.5;
        maxPersonListHeight = personContainerHeight * 4;
        minPersonListHeight = 5;
    }


    internal void SetWidth ( int screenSize )
    {
        viewModel.SetWidth (screenSize);
    }


    internal Size GetCustomComboboxDimensions ()
    {
        double height = personTyping.DesiredSize.Height + personList.Height;
        double width = personList.Width;
        Size result = new Size (width, height);
        return result;
    }


    internal void CloseCustomCombobox ()
    {

        if ( personListIsDropped && !openedViaButton && !cursorIsOverPersonList )
        {
            personList.Height = 0;
            personListIsDropped = false;

        }

        openedViaButton = false;
    }


    internal void GeneratePdf ( object sender, TappedEventArgs args )
    {
        List<FilePickerFileType> fileExtentions = [];
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
                       Task<bool> pdf = viewModel.GeneratePdf (result);
                       pdf.ContinueWith
                           (
                           task =>
                           {
                               if ( pdf.Result == false )
                               {
                                   string message = "Выбраный файл открыт в другом приложении. Закройте его и повторите.";

                                   int idOk = Winapi.MessageBox (0, message, "", 0);
                                   //GeneratePdf (result);
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
                           }
                           );
                   }
               }
            );
    }


    private void GeneratePdf ( string result ) 
    {
        Task<bool> pdf = viewModel.GeneratePdf (result);

        pdf.ContinueWith
                           (
                           task =>
                           {
                               if ( pdf.Result == false )
                               {
                                   string message = "Выбраный файл открыт в другом приложении. Закройте его и повторите.";
                                   int idOk = Winapi.MessageBox (0, message, "", 0);
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
                           }
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
        TryToEnableBadgeCreationButton ();
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


    internal void HandleTemplateChoosing ( object sender, SelectionChangedEventArgs args )
    {
        ComboBox comboBox = ( ComboBox ) sender;
        viewModel.chosenTemplate = ( FileInfo ) comboBox.SelectedItem;
        templateIsSelected = true;
        TryToEnableBadgeCreationButton ();
    }


    internal void DropDownOrPickUpPersonListViaFocus ( object sender, GotFocusEventArgs args )
    {
        if ( personListIsDropped )
        {
            personList.Height = 0;
            personListIsDropped = false;
        }
    }


    internal void DropDownOrPickUpPersonList ( object sender, TappedEventArgs args )
    {
        if ( personListIsDropped )
        {
            personList.Height = 0;
            personListIsDropped = false;
        }
        else
        {
            personTyping.Focus (NavigationMethod.Tab);
            personList.Height = CalculatePersonListHeight ();
            personListIsDropped = true;

            if ( openedViaButton )
            {
                openedViaButton = false;
            }
            else 
            {
                openedViaButton = true;
            }
            
        }
    }


    internal void DropDownOrPickUpPersonListViaKey ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();
        bool keyIsEnter = key == "Return";

        if ( keyIsEnter )
        {
            if ( personListIsDropped )
            {
                personList.Height = 0;
                personListIsDropped = false;
            }
            else
            {
                personList.Height = CalculatePersonListHeight ();
                personListIsDropped = true;
                openedViaButton = false;
            }

            return;
        }

        //bool keyIsTab = key == "Tab";

        //if ( keyIsTab )
        //{
        //    personTyping.Focus (NavigationMethod.Tab);
        //    return;
        //}
    }


    internal void ClosePersonList ( object sender , TappedEventArgs args )
    {
        if ( personListIsDropped )
        {
            personList.Height = 0;
            personListIsDropped = false;
        }
    }


    internal void HandlePersonChoosingViaTapping ( object sender, TappedEventArgs args )
    {
        //personChoosingIsTapped = true;
        

        if ( personListIsDropped   &&   selectionIsChanged )
        {
            personList.Height = 0;
            personListIsDropped = false;
            selectionIsChanged = false;
        }

        int fdf = 0;
    }


    internal void HandleSelectionChanged ( object sender, SelectionChangedEventArgs args )
    {
        viewModel.chosenPerson = ( Person ) personList.SelectedItem;
        Person person = ( Person ) personList.SelectedItem;

        if (person != null) 
        {
            personTyping.Text = person.StringPresentation;
            singlePersonIsSelected = true;
            entirePersonListIsSelected = false;
            TryToEnableBadgeCreationButton ();
            selectionIsChanged = true;
        }

        //if ( personListIsDropped   &&   ! personChoosingIsTapped )
        //{
        //    personList.Height = 0;
        //    personListIsDropped = false;
        //    personChoosingIsTapped = false;
        //}

        int fdf = 0;
    }


    internal void CursorIsOverPersonList ( object sender, PointerEventArgs args )
    {
        cursorIsOverPersonList = true;
    }


    internal void CursorIsOutOfPersonList ( object sender, PointerEventArgs args )
    {
        cursorIsOverPersonList = false;
    }


    private double CalculatePersonListHeight ( )
    {
        int personCount = viewModel. visiblePeople. Count;
        double personListHeight = personContainerHeight * personCount;

        if( personListHeight > maxPersonListHeight )
        {
            personListHeight = maxPersonListHeight;
        }

        if ( personListHeight < minPersonListHeight )
        {
            personListHeight = minPersonListHeight;
        }

        return personListHeight;
    }


    private void TryToEnableBadgeCreationButton ()
    {
        bool itsTimeToEnable = ( singlePersonIsSelected || entirePersonListIsSelected ) && templateIsSelected;
        if ( itsTimeToEnable )
        {
            buildBadges.IsEnabled = true;
        }
    }


    private bool IsKeyUnipacting ( string key )
    {
        bool keyIsUnimpacting = key == "Tab";
        keyIsUnimpacting = keyIsUnimpacting || ( key == "Left" );
        keyIsUnimpacting = keyIsUnimpacting || ( key == "Up" );
        keyIsUnimpacting = keyIsUnimpacting || ( key == "Right" );
        keyIsUnimpacting = keyIsUnimpacting || ( key == "Down" );
        keyIsUnimpacting = keyIsUnimpacting || ( key == "Return" );
        return keyIsUnimpacting;
    }


    internal void EditIncorrectBadges (object sender, TappedEventArgs args) 
    {
        if (incorrectBadges.Count > 0) 
        {
            VMBadge beingEdited = incorrectBadges [0];
            owner.Content = new BadgeEditionView (beingEdited);
        }
    }


    internal void HandlePersonListReduction ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();
        bool keyIsUnimpacting = IsKeyUnipacting (key);

        if ( keyIsUnimpacting )
        {
            return;
        }

        //if ( key == "Tab" ) 
        //{
        //    personTyping.Focus (NavigationMethod.Tab);
        //}

        buildBadges.IsEnabled = false;
        clearBadges.IsEnabled = false;
        save.IsEnabled = false;
        print.IsEnabled = false;

        TextBox textBox = ( TextBox ) sender;
        string str = textBox.Text;

        if ( str != null )
        {
            string partOfName = textBox.Text.ToLower ( );

            List<Person> people = viewModel.people;
            ObservableCollection<Person> foundVisiblePeople = new ObservableCollection<Person> ( );

            foreach ( Person person in people )
            {
                if ( person.StringPresentation.ToLower ( ) == partOfName )
                {
                    RecoverVisiblePeople ( );
                    return;
                }

                string entireName = person.StringPresentation;
                string entireNameInLowCase = entireName.ToLower ( );

                if ( entireNameInLowCase.Contains ( partOfName ) && entireNameInLowCase != partOfName )
                {
                    foundVisiblePeople.Add ( person );
                }
            }

            viewModel.visiblePeople = foundVisiblePeople;
            personList.Height = CalculatePersonListHeight ( );
            personListIsDropped = true;
        }
    }


    internal void SetFocusOnPersonTyping ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();
        bool keyIsUnimpacting = key != "Tab";
        bool personListIsFocused = personList.IsFocused;

        if ( keyIsUnimpacting  && ! personListIsFocused )
        {
            return;
        }

        personTyping.Focus (NavigationMethod.Tab);
    }


    private void RecoverVisiblePeople ()
    {
        List<Person> people = viewModel. people;
        viewModel. visiblePeople = new ();
        viewModel.visiblePeople.AddRange (people);
    }


    private void HideCoveredButtons ()
    {
        int peopleCount = viewModel.visiblePeople.Count;

        if ( peopleCount == 1 ) 
        {
            buildBadges.IsEnabled = false;
            clearBadges.IsEnabled = false;
            save.IsEnabled = false;
            print.IsEnabled = false;
        }

        if ( peopleCount > 1 )
        {
            buildBadges.IsEnabled = false;
            clearBadges.IsEnabled = false;
            save.IsEnabled = false;
            print.IsEnabled = false;
        }

    }


    private void ShowButtons () 
    {
        buildBadges.IsEnabled = false;
        clearBadges.IsEnabled = false;
        save.IsEnabled = false;
        print.IsEnabled = false;



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
        if (scalabilityDepth < maxDepth) 
        {

            viewModel.ZoomOnDocument (scalabilityStep);
            scalabilityDepth++;
        }

        if ( scalabilityDepth == maxDepth ) 
        {
            zoomOn.IsEnabled = false;
        }

        if ( ! zoomOut.IsEnabled )
        {
            zoomOut.IsEnabled = true;
        }
    }


    internal void ZoomOut ( object sender, TappedEventArgs args )
    {
        if ( scalabilityDepth > minDepth )
        {
            viewModel.ZoomOutDocument (scalabilityStep);
            scalabilityDepth--;
        }

        if ( scalabilityDepth == minDepth )
        {
            zoomOut.IsEnabled = false;
        }

        if ( ! zoomOn.IsEnabled )
        {
            zoomOn.IsEnabled = true;
        }
    }


    internal void SetNewScale ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();
        string scale = scalabilityGrade.Text;
        bool scaleIsCorrect = (scale != null)  &&  (IsScaleStringCorrect(scale));

        if (scaleIsCorrect) 
        {
            if (IsKeyCorrect(key)) 
            {
            
            }


        }

        TextBox textBox = ( TextBox ) sender;
        string text = textBox.Text;
        bool textExists = (text != null) && (text != string.Empty);

        if ( textExists ) 
        {
            if ( text.Contains('%') ) 
            {
                text = text.Remove(text.Length - 1);
            }

            //try
            //{
            //    int scale = ( int ) UInt32.Parse (text);
            //    string procent = "%";

            //    if ( text.Contains (' ') )
            //    {
            //        int spaceIndex = text.IndexOf (' ');
            //    }
            //}
            //catch ( FormatException ex )
            //{ }
        }
        
    }


    private bool IsScaleStringCorrect ( string beingProcessed ) 
    {
        bool scaleIsCorrect = beingProcessed.Length > 2;
        scaleIsCorrect = scaleIsCorrect   &&   beingProcessed [beingProcessed.Length - 1] == '%';
        scaleIsCorrect = scaleIsCorrect   &&   (beingProcessed [beingProcessed.Length - 2] == ' ');
        scaleIsCorrect = scaleIsCorrect   &&   ( beingProcessed [beingProcessed.Length - 3] != ' ' );

        try 
        {
            int lastIntegerIndex = beingProcessed.Length - 3;
            string integerPart = beingProcessed.Substring (0, lastIntegerIndex);
            ushort scale = UInt16.Parse (integerPart);
            scaleIsCorrect = scaleIsCorrect   &&   (scale > minScalability)   &&   (scale < maxScalability);

            if ( scaleIsCorrect ) 
            { 
            
            }
        }
        catch( Exception ex ) 
        {
        
        }

        return scaleIsCorrect;
    }


    private bool IsKeyCorrect ( string key )
    {
        bool keyIsCorrect = (key.Length == 2);
        keyIsCorrect = keyIsCorrect   &&   (key [0] == 'D');
        keyIsCorrect = keyIsCorrect   ||   (key == "Back");
        return keyIsCorrect;
    }


    internal void StepOnPage ( object sender, TextChangedEventArgs args )
    {
        TextBox textBox = ( TextBox ) sender;
      
        try
        {
            int pageNumber = (int) UInt32.Parse (textBox.Text);
            int visiblePageNum = viewModel.VisualisePageWithNumber (pageNumber);
            visiblePageNumber.Text = viewModel. visiblePageNumber.ToString ( );
            SetEnablePageNavigation ();
        }
        catch (System.FormatException e)
        {
            visiblePageNumber.Text = viewModel. visiblePageNumber.ToString ( );
        }
    }


    internal void Seal ( object sender, TextChangedEventArgs args )
    {
        
    }

}



public static class Winapi
{
    [DllImport ("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int MessageBox ( IntPtr hWnd, string lpText, string lpCaption, uint uType );
}



public static class CursorViaWinapi 
{
    [DllImport ("user32.dll")]
    public static extern bool GetCursorPos ( ref POINT lpPoint );


    [DllImport ("user32.dll")]
    public static extern void mouse_event ( int dsFlags, int dx, int dy, int cButtons, int dsExtraInfo );
}



[StructLayout (LayoutKind.Sequential)]
public struct POINT { public int x; public int y; }


