using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using View.EditionView;


namespace View.MainWindow;

public partial class MainWin : Window
{
    public static readonly Color white = new Color (255, 255, 255, 255);
    public static readonly Color black = new Color (255, 0, 0, 0);
    private static readonly int _onScreenRestriction = 50;

    private static PixelPoint _position;

    public static IStorageProvider CommonStorageProvider { get; private set; }
    internal static MainWin Window { get; private set; }

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

    private MainView.MainView _mainView;
    private PixelSize _screenSize;
    private double _currentWidth;
    private double _currentHeight;
    
    internal double WidthDifference { get; private set; }
    internal double HeightDifference { get; private set; }


    public MainWin ( )
    {
        InitializeComponent();

        CommonStorageProvider = StorageProvider;

        this.Opened += OnOpened;
        _mainView = ( MainView.MainView ) Content;

        this.SizeChanged += OnSizeChanged;
        _currentWidth = Width;
        _currentHeight = Height;

        this.PointerReleased += ReleaseCaptured;
        this.PositionChanged += HandlePositionChange;
        PointerMoved += Moved;

        Window = this;
        Cursor = new Cursor (StandardCursorType.Arrow);

        CanResize = true;

        _position = Position;
    }


    internal static MainWin ? GetMainWindow ( )
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
    }


    private void OnSizeChanged ( object ? sender , SizeChangedEventArgs args )
    {
        try
        {
            MainView.MainView mainView = ( MainView.MainView ) Content;
            double newWidth = args.NewSize.Width;
            double newHeight = args.NewSize.Height;
            double newWidthDifference = _currentWidth - newWidth;
            double newHeightDifference = _currentHeight - newHeight;
            WidthDifference += newWidthDifference;
            HeightDifference = newHeightDifference;
            _currentWidth = newWidth;
            _currentHeight = newHeight;
            mainView.ChangeSize (newWidthDifference, newHeightDifference);
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
            }
            catch ( System.InvalidCastException excp )
            {
            }
        }
    }


    internal void CancelSizeDifference ( )
    {
        WidthDifference = 0;
    }


    internal void ReleaseCaptured ( object sender, PointerReleasedEventArgs args )
    {
        PointerPoint point = args.GetCurrentPoint (sender as Control);
        double x = point.Position.X;
        double y = point.Position.Y;

        MainView.MainView mainView = Content  as  MainView.MainView;

        if ( mainView != null )
        {
            mainView.ReleaseCaptured ();
        }
        else 
        {
            BadgeEditorView editorView = Content  as  BadgeEditorView;

            if (editorView != null) 
            {
                editorView.ReleaseCaptured ( );
            }
        }
    }


    internal void Moved ( object sender, PointerEventArgs args )
    {
        MainView.MainView mainView = Content as MainView.MainView;

        if ( mainView != null )
        {
            mainView.MovePage (args);
        }
        else
        {
            BadgeEditorView editorView = Content as BadgeEditorView;

            if ( editorView != null )
            {
                editorView.MoveBadge (args);
            }
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
