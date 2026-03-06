using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Lister.Desktop.Entities.BadgeVM;
using Lister.Desktop.Views.EditionView.Parts.WorkArea.ViewModel;
using System.Diagnostics;

namespace Lister.Desktop.Views.EditionView.Parts.WorkArea;

public partial class WorkAreaUserControl : UserControl
{
    private WorkAreaViewModel? _viewModel;
    private TextBlock? _focusedTextBlock;
    private Image? _focusedImage;
    private Shape? _focusedShape;
    private bool _capturedTextExists;
    private bool _capturedImageExists;
    private bool _capturedShapeExists;
    private bool _badgeIsCaptured;
    private Point _pointerOnBadgeComponent;
    private Point _pointerOnBadge;
    private bool _isReleaseLocked;
    private Stopwatch? _focusTime;

    public WorkAreaUserControl ()
    {
        InitializeComponent ();

        DataContextChanged += (sender, args) => 
        {
            _viewModel = DataContext as WorkAreaViewModel;
        };
    }

    internal void ChangeSize ( double widthDifference, double heightDifference )
    {
        Width -= widthDifference;
        Height -= heightDifference;

        MainBorder.Width -= widthDifference;

        Scroller.Width -= widthDifference;
        Scroller.Height -= heightDifference;
    }

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

    internal void FocusBadgeElement ( object sender, TappedEventArgs args )
    {
        if ( sender is TextBlock textBlock )
        {
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
        if ( _focusedTextBlock != null && _focusedTextBlock.Parent is Border frame )
        {
            frame.BorderBrush = null;
        }

        _focusedTextBlock = line;
        string? content = _focusedTextBlock.Text;
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
                    if ( _focusedTextBlock.Equals ( textBlock ) )
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

    internal void CaptureBadgeElement ( object sender, PointerPressedEventArgs args )
    {
        TextBlock? textBlock = sender as TextBlock;

        if ( ( _focusedTextBlock != null ) && ( textBlock == _focusedTextBlock ) )
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

    internal void MoveBadgeElement ( object sender, PointerEventArgs args )
    {
        bool shouldMove = ( ( _capturedTextExists ) || ( _capturedImageExists ) || ( _capturedShapeExists ) );

        if ( shouldMove )
        {
            Point newPosition = _pointerOnBadgeComponent;

            if ( _capturedTextExists )
            {
                newPosition = args.GetPosition ( _focusedTextBlock );
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

    internal void MoveBadge ( PointerEventArgs args )
    {
        bool badgeIsMovable = !( _capturedTextExists || _capturedImageExists || _capturedShapeExists ) && _badgeIsCaptured;

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

    internal void SetCrossCursor ( object sender, PointerEventArgs args )
    {
        TextBlock? textBlock = sender as TextBlock;

        if ( ( _focusedTextBlock != null ) && ( _focusedTextBlock == textBlock ) )
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

    internal void ReleaseCaptured ()
    {
        bool focusedExists = _focusedTextBlock != null || _focusedImage != null || _focusedShape != null;

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
        if ( _focusedTextBlock != null && _focusedTextBlock.Parent is Border container )
        {
            container.BorderBrush = null;
            _focusedTextBlock = null;
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
}