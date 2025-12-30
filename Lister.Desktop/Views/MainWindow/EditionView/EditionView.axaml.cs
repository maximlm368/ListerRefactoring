using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using Lister.Desktop.App;
using Lister.Desktop.ModelMappings.BadgeVM;
using Lister.Desktop.Views.DialogMessageWindows.Dialog;
using Lister.Desktop.Views.MainWindow.EditionView.ViewModel;
using Lister.Desktop.Views.MainWindow.WaitingView.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Threading.Tasks;
using mainView = Lister.Desktop.Views.MainWindow.MainView;

namespace Lister.Desktop.Views.MainWindow.EditionView;

/// <summary>
/// Is view for edition of current badge collection.
/// </summary>
public sealed partial class BadgeEditorView : UserControl
{
    private static readonly string _question = "Сохранить изменения и вернуться к макету ?";
    private static readonly int _focusedTextLineLengthLimit = 100;
    private static bool _widthIsChanged;
    private static bool _heightIsChanged;
    private static bool _someControlPressed;

    private double _capturingY;
    private bool _runnerIsCaptured = false;
    private TextBlock? _focusedText;
    private Image? _focusedImage;
    private Shape? _focusedShape;
    private bool _capturedTextExists;
    private bool _capturedImageExists;
    private bool _capturedShapeExists;
    private bool _badgeIsCaptured;
    private Point _pointerOnBadgeComponent;
    private Point _pointerOnBadge;
    private mainView.MainView? _back;
    private readonly BadgeEditorViewModel? _viewModel;
    private bool _isReleaseLocked;
    private bool _isTextEditorFocused;
    private bool _isZoomOnOutFocused;
    private Stopwatch? _focusTime;
    private double _dastinationPointer = 0;

    public BadgeEditorView ()
    {
        InitializeComponent ();
    }

    public BadgeEditorView ( int incorrectBadgesAmmount ) : this ()
    {
        EditorViewModelArgs args = ListerApp.Services.GetRequiredService<EditorViewModelArgs> ();
        _viewModel = new ( incorrectBadgesAmmount, args );
        DataContext = _viewModel;
        _viewModel.BackingActivated += CheckBacking;
        _viewModel.BackingComplated += ComplateBacking;
        _viewModel.PeopleGotEmpty += () => { Cancel.IsEnabled = false; };

        DisableFocusAdorner ();
        EditorTextBox.AddHandler ( TextBox.PointerReleasedEvent, PreventPasting, RoutingStrategies.Tunnel );
        FilterChoosing.SelectedIndex = 0;
        PointerPressed += PointerIsPressed;
    }

    private void PreventPasting ( object? sender, PointerReleasedEventArgs args )
    {
        args.Handled = true;
    }

    private void DisableFocusAdorner ()
    {
        FocusAdorner = null;
        FirstBadge.FocusAdorner = null;
        PreviousBadge.FocusAdorner = null;
        ToNextBadge.FocusAdorner = null;
        ToLastBadge.FocusAdorner = null;
        ZoomOnBadge.FocusAdorner = null;
        ZoomOutBadge.FocusAdorner = null;
        EditionPanel.FocusAdorner = null;
        Extender.FocusAdorner = null;
        Switcher.FocusAdorner = null;
        Up.FocusAdorner = null;
        Down.FocusAdorner = null;
        Upper.FocusAdorner = null;
        Downer.FocusAdorner = null;
        ZoomOn.FocusAdorner = null;
        ZoomOut.FocusAdorner = null;
        Spliter.FocusAdorner = null;
        Cancel.FocusAdorner = null;
        BackButton.FocusAdorner = null;
    }

    private void PointerIsPressed ( object? sender, PointerPressedEventArgs args )
    {
        var point = args.GetCurrentPoint ( sender as Control );

        if ( point.Properties.IsRightButtonPressed )
        {
            if ( !_someControlPressed )
            {
                Focusable = true;
                this.Focus ();
            }

            _someControlPressed = false;
        }
        else if ( point.Properties.IsLeftButtonPressed )
        {
            if ( !_someControlPressed )
            {
                Focusable = true;
                this.Focus ();
            }

            _someControlPressed = false;
        }

        Focusable = false;
    }

    internal void SomeButtonPressed ( object sender, PointerPressedEventArgs args )
    {
        _someControlPressed = true;
    }

    private void ComplateBacking ()
    {
        if ( Parent is not MainWindow mainWindow || _viewModel == null )
        {
            return;
        }

        _back?.SetProperSize ( _viewModel.ViewWidth, _viewModel.ViewHeight );
        mainWindow.CancelSizeDifference ();
        _back?.RefreshTemplateAppearences ();
        mainWindow.Content = _back;
    }

    internal void ChangeSize ( double widthDifference, double heightDifference )
    {
        ChangeShadowSize ( widthDifference );
        _viewModel?.ChangeSize ( widthDifference, heightDifference );
        WaitingViewModel waitingVM = ListerApp.Services.GetRequiredService<WaitingViewModel> ();
        waitingVM.ChangeSize ( heightDifference, widthDifference );
    }

    internal void ChangeShadowSize ( double widthDifference )
    {
        var leftChildren = LeftShadow.Children;
        var rightChildren = RightShadow.Children;

        foreach ( var child in leftChildren )
        {
            if ( child.Width > 10 )
            {
                child.Width -= widthDifference / 2;
            }
            else
            {
                double left = child.GetValue ( Canvas.LeftProperty );
                child.SetValue ( Canvas.LeftProperty, left - widthDifference / 2 );
            }
        }

        foreach ( var child in rightChildren )
        {
            if ( child.Width > 10 )
            {
                child.Width -= widthDifference / 2;
            }
        }

        var logicChildren = SimpleShadow.GetVisualChildren ();

        foreach ( var child in logicChildren )
        {
            Rectangle rectangle = ( Rectangle ) child;
            rectangle.Width -= widthDifference;
        }
    }

    internal void SetProperSize ( double properWidth, double properHeight )
    {
        if ( properWidth != Width )
        {
            _widthIsChanged = true;
        }
        else
        {
            _widthIsChanged = false;
        }

        if ( properHeight != Height )
        {
            _heightIsChanged = true;
        }
        else
        {
            _heightIsChanged = false;
        }

        double widthDifference = Width - properWidth;
        double heightDifference = Height - properHeight;
        Width = properWidth;
        Height = properHeight;
        ChangeShadowSize ( widthDifference );

        if ( _widthIsChanged || _heightIsChanged )
        {
            _viewModel?.ChangeSize ( widthDifference, heightDifference );
        }
    }

    internal void PrepareBy ( List<BadgeViewModel> processables, mainView.MainView back )
    {
        _viewModel?.SetProcessables ( processables );
        _back = back;
    }

    private async void CheckBacking ()
    {
        if ( MainWindow.Window == null )
        {
            return;
        }

        _viewModel?.HandleDialogOpenig ();
        DialogWindow dialog = new ( _question );
        dialog.Closed += ( s, a ) => { _viewModel?.HandleDialogClosing (); };

        bool result = await dialog.ShowDialog<bool> ( MainWindow.Window );

        if ( result )
        {
            _viewModel?.GoBack ();
        }
    }

    internal void HandleTextEdition ( object sender, TextChangedEventArgs args )
    {
        string? editable = EditorTextBox.Text;

        if ( editable == null || _focusedText == null )
        {
            return;
        }

        if ( editable.Length > _focusedTextLineLengthLimit )
        {
            EditorTextBox.Text = editable [.._focusedTextLineLengthLimit];

            return;
        }

        _viewModel?.ResetFocusedText ( editable );
    }

    internal void SelectionChanged ( object sender, SelectionChangedEventArgs args )
    {
        ComboBox? comboBox = sender as ComboBox;
        string? selected = comboBox?.SelectedValue as string;
        _viewModel?.Filter ( selected );
    }

    internal void SliderItemPointerEntered ( object sender, PointerEventArgs args )
    {
        if ( sender is not Border border )
        {
            return;
        }

        border.BorderBrush = new SolidColorBrush ( new Color ( 255, 37, 112, 167 ) );
    }

    internal void SliderItemPointerExited ( object sender, PointerEventArgs args )
    {
        if ( sender is not Border border )
        {
            return;
        }

        border.BorderBrush = null;
    }

    #region CapturingAndMoving
    internal void HandleGettingFocus ( object sender, GotFocusEventArgs args )
    {
        _isReleaseLocked = true;
        _isTextEditorFocused = true;
    }

    internal void TextEditorLostFocus ( object sender, RoutedEventArgs args )
    {
        _isTextEditorFocused = false;
    }

    internal void ZoomOnOutGotFocus ( object sender, GotFocusEventArgs args )
    {
        _isReleaseLocked = true;
        _isZoomOnOutFocused = true;
    }


    internal void ZoomOnOutLostFocus ( object sender, RoutedEventArgs args )
    {
        _isZoomOnOutFocused = false;
    }

    internal void Focus ( object sender, TappedEventArgs args )
    {
        if ( sender is TextBlock textBlock )
        {
            ScalabilityGrade.IsEnabled = true;
            EditorTextBox.IsEnabled = true;
            FocusTextLine ( textBlock );

            return;
        }

        if ( sender is Image image )
        {
            FocusImage ( image );

            return;
        }

        if ( sender is Shape shape )
        {
            FocusShape ( shape );

            return;
        }
    }

    internal void FocusTextLine ( TextBlock line )
    {
        if ( _focusedText != null && _focusedText.Parent is Border frame )
        {
            frame.BorderBrush = null;
        }

        _focusedText = line;
        string? content = _focusedText.Text;
        int lineNumber = 0;
        int counter = 0;
        bool shouldBreak = false;
        var children = TextLines.GetLogicalChildren ();

        foreach ( ILogical child in children )
        {
            IEnumerable<ILogical> ch = child.GetLogicalChildren ();

            foreach ( ILogical border in ch )
            {
                var textBlocks = border.GetLogicalChildren ();

                foreach ( ILogical textBlock in textBlocks )
                {
                    if ( _focusedText.Equals ( textBlock ) )
                    {
                        lineNumber = counter;
                        shouldBreak = true;

                        break;
                    }
                }

                if ( shouldBreak )
                {
                    break;
                }
            }

            if ( shouldBreak )
            {
                break;
            }

            counter++;
        }

        if ( line.Parent is Border container ) 
        {
            container.BorderBrush = new SolidColorBrush ( new Color ( 255, 0, 0, 255 ) );
        }
        
        _viewModel?.FocusTextLine ( content, lineNumber );
        Cursor = new Cursor ( StandardCursorType.SizeAll );
        _isReleaseLocked = true;
        _focusTime = Stopwatch.StartNew ();
    }

    internal void FocusShape ( Shape shape )
    {
        if ( shape.DataContext is ShapeViewModel shapeViewModel )
        {
            _viewModel?.FocusShape ( shapeViewModel.Type, shapeViewModel.Id );
        }

        _focusedShape = shape;
        Cursor = new ( StandardCursorType.SizeAll );
        _isReleaseLocked = true;
        _focusTime = Stopwatch.StartNew ();
    }

    internal void FocusImage ( Image image )
    {
        if ( image.DataContext is ImageViewModel imageViewModel )
        {
            _viewModel?.FocusImage ( imageViewModel.Id );
        }

        _focusedImage = image;
        Cursor = new Cursor ( StandardCursorType.SizeAll );
        _isReleaseLocked = true;
        _focusTime = Stopwatch.StartNew ();
    }

    internal void Move ( object sender, PointerEventArgs args )
    {
        bool shouldMove = ( ( _capturedTextExists ) || ( _capturedImageExists ) || ( _capturedShapeExists ) );

        if ( shouldMove )
        {
            Point newPosition = _pointerOnBadgeComponent;

            if ( _capturedTextExists )
            {
                newPosition = args.GetPosition ( _focusedText );
            }
            else if ( _capturedImageExists )
            {
                newPosition = args.GetPosition ( _focusedImage );
            }
            else if ( _capturedShapeExists )
            {
                newPosition = args.GetPosition ( _focusedShape );
            }

            double verticalDelta = _pointerOnBadgeComponent.Y - newPosition.Y;
            double horizontalDelta = _pointerOnBadgeComponent.X - newPosition.X;
            Point delta = new ( horizontalDelta, verticalDelta );
            _viewModel?.MoveCaptured ( delta );
        }
    }

    internal void ToSide ( object sender, KeyEventArgs args )
    {
        if ( _isTextEditorFocused || _isZoomOnOutFocused )
        {
            return;
        }

        string key = args.Key.ToString ();
        _viewModel?.FocusedToSide ( key );
    }

    internal void ChangeFocusedFontSize ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();

        if ( key == "Up" )
        {
            _viewModel?.IncreaseFontSize ();
        }
        else if ( key == "Down" )
        {
            _viewModel?.ReduceFontSize ();
        }
    }

    internal void Capture ( object sender, PointerPressedEventArgs args )
    {
        TextBlock? textBlock = sender as TextBlock;

        if ( ( _focusedText != null ) && ( textBlock == _focusedText ) )
        {
            _pointerOnBadgeComponent = args.GetPosition ( textBlock );
            _capturedTextExists = true;
        }

        Image? image = sender as Image;

        if ( ( _focusedImage != null ) && ( image == _focusedImage ) )
        {
            _pointerOnBadgeComponent = args.GetPosition ( image );
            _capturedImageExists = true;
        }

        Shape? shape = sender as Shape;

        if ( ( _focusedShape != null ) && ( shape == _focusedShape ) )
        {
            _pointerOnBadgeComponent = args.GetPosition ( shape );
            _capturedShapeExists = true;
        }
    }

    internal void ReleaseCaptured ()
    {
        bool focusedExists = _focusedText != null || _focusedImage != null || _focusedShape != null;

        if ( _badgeIsCaptured )
        {
            _badgeIsCaptured = false;

            if ( !focusedExists )
            {
                Cursor = new Cursor ( StandardCursorType.Arrow );
            }
        }

        if ( _capturedTextExists || _capturedImageExists || _capturedShapeExists )
        {
            Release ();
        }
        else if ( focusedExists && ( _focusTime != null ) )
        {
            if ( !_isReleaseLocked )
            {
                Release ();
            }
            else
            {
                _focusTime.Stop ();
                TimeSpan timeSpan = _focusTime.Elapsed;

                if ( timeSpan.Ticks > 100_000_000 )
                {
                    Release ();
                }

                _isReleaseLocked = false;
            }
        }
    }

    private void Release ()
    {
        if ( _focusedText != null && _focusedText.Parent is Border container )
        {
            container.BorderBrush = null;
            _focusedText = null;
            _capturedTextExists = false;
        }
        else if ( _focusedImage != null )
        {
            _focusedImage = null;
            _capturedImageExists = false;
        }
        else if ( _focusedShape != null )
        {
            _focusedShape = null;
            _capturedShapeExists = false;
        }

        Cursor = new Cursor ( StandardCursorType.Arrow );
        _viewModel?.ReleaseCaptured ();
    }
    #endregion

    #region Cursor
    internal void SetCrossCursor ( object sender, PointerEventArgs args )
    {
        TextBlock? textBlock = sender as TextBlock;

        if ( ( _focusedText != null ) && ( _focusedText == textBlock ) )
        {
            Cursor = new Cursor ( StandardCursorType.SizeAll );
        }

        Shape? shape = sender as Shape;

        if ( ( _focusedShape != null ) && ( shape == _focusedShape ) )
        {
            Cursor = new Cursor ( StandardCursorType.SizeAll );
        }

        Image? image = sender as Image;

        if ( ( _focusedImage != null ) && ( image == _focusedImage ) )
        {
            Cursor = new Cursor ( StandardCursorType.SizeAll );
        }
    }

    internal void SetArrowCursor ( object sender, PointerEventArgs args )
    {
        Cursor = new Cursor ( StandardCursorType.Arrow );
    }
    #endregion

    #region NavigationAndScrolling
    internal void ToParticularBadge ( object sender, TappedEventArgs args )
    {
        if ( sender is Border border && border.DataContext is BadgeCorrectnessViewModel context )
        {
            _viewModel?.ToParticularBadge ( context );
        }
    }

    internal void ShiftRunner ( object sender, TappedEventArgs args )
    {
        Canvas? activator = sender as Canvas;
        _dastinationPointer = args.GetPosition ( activator ).Y;
        _viewModel?.ShiftRunner ( _dastinationPointer );
    }

    internal void CaptureRunner ( object sender, PointerPressedEventArgs args )
    {
        byte red = 0x51;
        byte green = 0x4c;
        byte blue = 0x48;

        if ( sender is Canvas runner ) 
        {
            runner.Background = new SolidColorBrush ( new Color ( 255, red, green, blue ) );
        }

        _runnerIsCaptured = true;
        Point inRunnerRelativePosition = args.GetPosition ( args.Source as Canvas );
        _capturingY = inRunnerRelativePosition.Y;
    }

    internal void OverRunner ( object sender, PointerEventArgs args )
    {
        byte red = 0xd1;
        byte green = 0xd1;
        byte blue = 0xd1;

        if ( sender is Canvas runner )
        {
            runner.Background = new SolidColorBrush ( new Color ( 255, red, green, blue ) );
        }
    }

    internal void ExitedRunner ( object sender, PointerEventArgs args )
    {
        byte red = 0x81;
        byte green = 0x79;
        byte blue = 0x74;

        if ( sender is Canvas runner )
        {
            runner.Background = new SolidColorBrush ( new Color ( 255, red, green, blue ) );
        }
    }

    internal void MoveRunner ( object sender, PointerEventArgs args )
    {
        if ( _runnerIsCaptured )
        {
            Point pointerPosition = args.GetPosition ( args.Source as Canvas );
            double runnerVerticalDelta = _capturingY - pointerPosition.Y;
            _viewModel?.MoveRunner ( runnerVerticalDelta );
        }
    }

    internal void ReleaseRunner ( object sender, PointerReleasedEventArgs args )
    {
        if ( _runnerIsCaptured )
        {
            byte red = 0x81;
            byte green = 0x79;
            byte blue = 0x74;

            if ( sender is Canvas runner )
            {
                runner.Background = new SolidColorBrush ( new Color ( 255, red, green, blue ) );
            }

            _runnerIsCaptured = false;
        }
    }

    internal void ScrollByWheel ( object sender, PointerWheelEventArgs args )
    {
        bool isDirectionUp = args.Delta.Y > 0;
        _viewModel?.ScrollByWheel ( isDirectionUp );
    }

    internal void ToParticularBadge ( object sender, KeyEventArgs args )
    {
        if ( _isTextEditorFocused || _isZoomOnOutFocused )
        {
            return;
        }

        string key = args.Key.ToString ();

        if ( key == "Up" )
        {
            _viewModel?.ToPrevious ();
        }
        else if ( key == "Down" )
        {
            _viewModel?.ToNext ();
        }
    }
    #endregion

    internal void CaptureBadge ( object sender, PointerPressedEventArgs args )
    {
        if ( _capturedTextExists || _capturedImageExists || _capturedShapeExists )
        {
            Cursor = new Cursor ( StandardCursorType.SizeAll );

            return;
        }

        PointerPoint point = args.GetCurrentPoint ( sender as Control );

        if ( point.Properties.IsLeftButtonPressed )
        {
            Cursor = new Cursor ( StandardCursorType.Hand );
            _badgeIsCaptured = true;
            _pointerOnBadge = args.GetPosition ( Scroller );
        }
    }

    internal void MoveBadge ( PointerEventArgs args )
    {
        bool badgeIsMovable = !( _capturedTextExists || _capturedImageExists || _capturedShapeExists )
           &&
           _badgeIsCaptured;

        if ( badgeIsMovable )
        {
            Point newPosition = args.GetPosition ( Scroller );
            double verticalDelta = _pointerOnBadge.Y - newPosition.Y;
            double horizontalDelta = _pointerOnBadge.X - newPosition.X;
            _pointerOnBadge = new Point ( newPosition.X, newPosition.Y );
            double horizontalOffset = Scroller.Offset.X;
            double verticalOffset = Scroller.Offset.Y;
            Scroller.Offset = new Vector ( horizontalOffset + horizontalDelta, verticalOffset + verticalDelta );
        }
    }
}
