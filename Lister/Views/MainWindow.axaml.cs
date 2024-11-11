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
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Input.Platform;
using Avalonia.Styling;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;


namespace Lister.Views;

public partial class MainWindow : Window
{
    public static readonly Color white = new Color (255, 255, 255, 255);
    public static readonly Color black = new Color (255, 0, 0, 0);
    private static readonly int _onScreenRestriction = 50;

    private static PixelPoint _position;

    public static IStorageProvider CommonStorageProvider { get; private set; }
    internal static MainWindow Window { get; private set; }

    private Window _modalWindow;
    internal Window ModalWindow 
    {
        get { return _modalWindow; }

        set 
        {
            _modalWindow = value;
        } 
    }

    internal static double HeightfDifference { get; private set; }

    private ModernMainView _mainView;
    private PixelSize _screenSize;
    private double _currentWidth;
    private double _currentHeight;
    
    internal double WidthDifference { get; private set; }
    internal double HeightDifference { get; private set; }


    public MainWindow ( )
    {
        InitializeComponent();

        CommonStorageProvider = StorageProvider;

        this.Opened += OnOpened;
        _mainView = ( ModernMainView ) Content;

        this.SizeChanged += OnSizeChanged;
        _currentWidth = Width;
        _currentHeight = Height;

        this.Tapped += HandleTapping;
        this.PointerReleased += ReleaseCaptured;
        this.PositionChanged += HandlePositionChange;

        Window = this;
        Cursor = new Cursor (StandardCursorType.Arrow);

        CanResize = true;

        _position = Position;
    }


    internal static MainWindow ? GetMainWindow ( )
    {
        return Window;
    }


    internal void SetSize ( PixelSize size )
    {
        _screenSize = size;
    }


    private void OnOpened ( object ? sender, EventArgs args )
    {
        int windowWidth = ( int ) this.DesiredSize.Width / 2;
        int windowHeight = ( int ) this.DesiredSize.Height / 2;
        int x = ( _screenSize.Width - windowWidth ) / 2;
        int y = ( _screenSize.Height - windowHeight ) / 2;
    }


    private void OnPositionChanged ( object ? sender, PixelPointEventArgs args )
    {
        Position = _position;

        //int dfd = 0;
    }


    private void OnSizeChanged ( object ? sender , SizeChangedEventArgs args )
    {
        try
        {
            ModernMainView mainView = ( ModernMainView ) Content;
            double newWidth = args.NewSize.Width;
            double newHeight = args.NewSize.Height;
            double newWidthDifference = _currentWidth - newWidth;
            double newHeightDifference = _currentHeight - newHeight;
            WidthDifference += newWidthDifference;
            HeightDifference = newHeightDifference;
            _currentWidth = newWidth;
            _currentHeight = newHeight;
            mainView.ChangeSize (newWidthDifference, newHeightDifference);

            //BadgeEditorViewModel editorVM = App.services.GetRequiredService<BadgeEditorViewModel> ();
            //if (editorVM != null) 
            //{
            //    editorVM.ChangeSize (newWidthDifference, newHeightDifference);
            //}
        }
        catch ( System.InvalidCastException ex )
        {
            try
            {
                BadgeEditorView mainView = ( BadgeEditorView ) Content;
                double newWidth = args.NewSize.Width;
                double newHeight = args.NewSize.Height;
                double widthDifference = _currentWidth - newWidth;
                double heightDifference = _currentHeight - newHeight;
                WidthDifference += widthDifference;
                HeightDifference = heightDifference;
                _currentWidth = newWidth;
                _currentHeight = newHeight;
                mainView.ChangeSize (widthDifference, heightDifference);

                //BadgeEditorViewModel editorVM = App.services.GetRequiredService<BadgeEditorViewModel> ();
                //editorVM.ChangeSize (widthDifference, heightDifference);
            }
            catch ( System.InvalidCastException excp )
            {
            }
        }
    }


    internal void CancelSizeDifference ( )
    {
        WidthDifference = 0;
        //HeightDifference = 0;

        //_mainView. waiting.Recover ();
    }


    internal void HandleTapping ( object sender, TappedEventArgs args )
    {
        try
        {
            ModernMainView mainView = ( ModernMainView ) Content;
            mainView.CloseCustomCombobox ();
        }
        catch ( InvalidCastException ex )
        {
            //BadgeEditorView mainView = ( BadgeEditorView ) Content;
            //mainView.ReleaseCaptured ();
        }
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


    internal void HandlePositionChange ( object sender, PixelPointEventArgs args )
    {
        RestrictPosition (sender, args);
        HoldDialogIfExistsOnLinux (sender, args);
        _position = Position;
    }


    private void RestrictPosition ( object sender, PixelPointEventArgs args )
    {
        PixelPoint currentPosition = this.Position;

        var screens = Screens;
        int count = screens.All.Count;

        Screen screen = screens.All [0];
        int screenHeight = screen.WorkingArea.Height;
        int screenWidth = screen.WorkingArea.Width;

        if ( currentPosition.Y > ( screenHeight - _onScreenRestriction ) )
        {
            this.Position = new PixelPoint (currentPosition.X, ( screenHeight - _onScreenRestriction ));
        }
    }


    private void HoldDialogIfExistsOnLinux ( object sender, PixelPointEventArgs args )
    {
        if ( ModalWindow == null ) 
        {
            return;
        }

        PixelPoint delta = Position - _position;

        ModalWindow.Position += delta;
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