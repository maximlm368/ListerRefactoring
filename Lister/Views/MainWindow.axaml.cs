using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ContentAssembler;
using DataGateway;
using Lister.ViewModels;

using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Threading.Tasks;


namespace Lister.Views;

public partial class MainWindow : Window
{
    public static IStorageProvider CommonStorageProvider { get; private set; }

    private PixelSize _screenSize;
    private double _currentWidth;
    private double _currentHeight;
    
    internal double WidthDifference { get; private set; }
    internal double HeightDifference { get; private set; }


    public MainWindow ( )
    {
        InitializeComponent();

        CommonStorageProvider = StorageProvider;

        //string badgeTemplatesFolderPath = @"./";
        //IBadgeAppearenceProvider badgeAppearenceDataSource = new BadgeAppearenceProvider (badgeTemplatesFolderPath);
        //IPeopleDataSource peopleDataSource = new PeopleSource ();
        //IResultOfSessionSaver converter = new ContentAssembler.ConverterToPdf ();
        //IUniformDocumentAssembler docAssembler = new UniformDocAssembler (badgeAppearenceDataSource, peopleDataSource);


        this.Opened += OnOpened;
        ModernMainView mainView = ( ModernMainView ) Content;


        //mainView.SetOwner ( this );
        //mainView.PassAssembler ( docAssembler );

        this.SizeChanged += OnSizeChanged;
        _currentWidth = Width;
        _currentHeight = Height;
        this.Tapped += HandleTapping;
        this.PointerReleased += ReleaseCaptured;

    }


    internal void SetWidth (int width) 
    {
        MainView mainView = ( MainView ) Content;
        mainView.SetWidth (width);
    }


    internal void SetSize ( PixelSize size )
    {
        _screenSize = size;
    }


    private void OnOpened ( object? sender, EventArgs e )
    {
        int windowWidth = ( int ) this.DesiredSize.Width / 2;
        int windowHeight = ( int ) this.DesiredSize.Height / 2;
        int x = ( _screenSize.Width - windowWidth ) / 2;
        int y = ( _screenSize.Height - windowHeight ) / 2;
        //this.Position = new Avalonia.PixelPoint (x, y);
        int wqw = 0;
    }


    private void OnSizeChanged ( object? sender , SizeChangedEventArgs e )
    {
        try
        {
            ModernMainView mainView = ( ModernMainView ) Content;
            double newWidth = e.NewSize.Width;
            double newHeight = e.NewSize.Height;
            double widthDifference = _currentWidth - newWidth;
            double heightDifference = _currentHeight - newHeight;
            WidthDifference += widthDifference;
            HeightDifference += heightDifference;
            _currentWidth = newWidth;
            _currentHeight = newHeight;
            mainView.ChangeSize (widthDifference, heightDifference);
        }
        catch ( System.InvalidCastException ex )
        {
            try
            {
                BadgeEditorView mainView = ( BadgeEditorView ) Content;
                double newWidth = e.NewSize.Width;
                double newHeight = e.NewSize.Height;
                double widthDifference = _currentWidth - newWidth;
                double heightDifference = _currentHeight - newHeight;
                WidthDifference += widthDifference;
                HeightDifference += heightDifference;
                _currentWidth = newWidth;
                _currentHeight = newHeight;
                mainView.ChangeSize (widthDifference, heightDifference);
            }
            catch ( System.InvalidCastException excp )
            {
            }
        }
    }


    internal void ResetDifference ( )
    {
        WidthDifference = 0;
        HeightDifference = 0;
    }


    internal void HandleTapping ( object sender, TappedEventArgs args )
    {
        try 
        {
            ModernMainView mainView = ( ModernMainView ) Content;
            mainView.CloseCustomCombobox ();
        }
        catch( InvalidCastException ex) {}
    }


    internal void ReleaseCaptured ( object sender, PointerReleasedEventArgs args )
    {
        try
        {
            ModernMainView mainView = ( ModernMainView ) Content;
            mainView.ReleaseRunner ();
        }
        catch ( InvalidCastException ex ) 
        {
            BadgeEditorView mainView = ( BadgeEditorView ) Content;
            mainView.ReleaseCaptured ();
        }
    }
}


//POINT cursorCoordinates = new POINT ();
//CursorViaWinapi.GetCursorPos (ref cursorCoordinates);
//int coordinateX = cursorCoordinates.x;
//int coordinateY = cursorCoordinates.y;

//ContentAssembler.Size pointOfReference = new ContentAssembler.Size (99, 63);
//int x = 99;
//int y = 63;

//ContentAssembler.Size targetSize = mainView.GetCustomComboboxDimensions ();
//int targetWidth = ( int ) targetSize.width;
//int targetHeight = ( int ) targetSize.height;

//bool cursorIsOutsideTarget = coordinateX < ( x );
//cursorIsOutsideTarget = cursorIsOutsideTarget && ( coordinateX > ( x + targetWidth ) );
//cursorIsOutsideTarget = cursorIsOutsideTarget && ( coordinateY < y );
//cursorIsOutsideTarget = cursorIsOutsideTarget && ( coordinateY > ( y + targetHeight ) );

//if ( cursorIsOutsideTarget )
//{
//    mainView.CloseCustomCombobox ();
//}