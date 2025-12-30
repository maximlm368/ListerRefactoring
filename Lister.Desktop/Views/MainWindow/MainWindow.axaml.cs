using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Lister.Desktop.Views.MainWindow.EditionView;
using mainView = Lister.Desktop.Views.MainWindow.MainView;

namespace Lister.Desktop.Views.MainWindow;

public sealed partial class MainWindow : Window
{
    private static readonly int _onScreenRestriction = 50;
    private static PixelPoint _pointerPosition;

    internal static IStorageProvider? CommonStorageProvider { get; private set; }
    internal static MainWindow? Window { get; private set; }
    internal static double HeightfDifference { get; private set; }

    private double _currentWidth;
    private double _currentHeight;

    internal Window? ModalWindow { get; set; }
    internal double WidthDifference { get; private set; }
    internal double HeightDifference { get; private set; }


    public MainWindow ( )
    {
        InitializeComponent();

        CommonStorageProvider = StorageProvider;
        _currentWidth = Width;
        _currentHeight = Height;
        Window = this;
        Cursor = new Cursor (StandardCursorType.Arrow);
        CanResize = true;
        _pointerPosition = Position;

        SizeChanged += OnSizeChanged;
        PointerReleased += ReleaseCaptured;
        PositionChanged += HandlePositionChange;
        PointerMoved += Moved;
    }

    internal static MainWindow ? GetMainWindow ( )
    {
        return Window;
    }

    private void OnSizeChanged ( object? sender, SizeChangedEventArgs args )
    {
        if ( Content is MainView.MainView mainView )
        {
            double newWidth = args.NewSize.Width;
            double newHeight = args.NewSize.Height;
            double newWidthDifference = _currentWidth - newWidth;
            double newHeightDifference = _currentHeight - newHeight;
            WidthDifference += newWidthDifference;
            HeightDifference = newHeightDifference;
            _currentWidth = newWidth;
            _currentHeight = newHeight;
            mainView.ChangeSize ( newWidthDifference, newHeightDifference );

            return;
        }

        if ( Content is BadgeEditorView editionView )
        {
            double newWidth = args.NewSize.Width;
            double newHeight = args.NewSize.Height;
            double widthDifference = _currentWidth - newWidth;
            double heightDifference = _currentHeight - newHeight;
            WidthDifference += widthDifference;
            HeightDifference = heightDifference;
            _currentWidth = newWidth;
            _currentHeight = newHeight;
            editionView.ChangeSize ( widthDifference, heightDifference );
        }
    }

    internal void CancelSizeDifference ( )
    {
        WidthDifference = 0;
    }

    internal void ReleaseCaptured ( object? sender, PointerReleasedEventArgs args )
    {
        if ( Content is MainView.MainView mainView )
        {
            mainView.ReleaseCaptured ();
        }
        else
        {
            if ( Content is BadgeEditorView editorView )
            {
                editorView.ReleaseCaptured ();
            }
        }
    }

    internal void Moved ( object? sender, PointerEventArgs args )
    {
        if ( Content is MainView.MainView mainView )
        {
            mainView.MovePage ( args );
        }
        else
        {
            if ( Content is BadgeEditorView editorView )
            {
                editorView.MoveBadge ( args );
            }
        }
    }

    internal void HandlePositionChange ( object? sender, PixelPointEventArgs args )
    {
        RestrictPosition (sender, args);
        HoldDialogIfExistsOnLinux (sender, args);
        _pointerPosition = Position;
    }

    private void RestrictPosition ( object? sender, PixelPointEventArgs args )
    {
        PixelPoint currentPosition = this.Position;
        Screens screens = Screens;
        Screen screen = screens.All [0];
        int screenHeight = screen.WorkingArea.Height;

        if ( currentPosition.Y > ( screenHeight - _onScreenRestriction ) )
        {
            this.Position = new PixelPoint (currentPosition.X, ( screenHeight - _onScreenRestriction ));
        }
    }

    private void HoldDialogIfExistsOnLinux ( object? sender, PixelPointEventArgs args )
    {
        if ( ModalWindow == null ) 
        {
            return;
        }

        PixelPoint delta = Position - _pointerPosition;
        ModalWindow.Position += delta;
    }
}
