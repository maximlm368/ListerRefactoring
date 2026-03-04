using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Lister.Desktop.Views.MainWindow.MainView.Parts.Scene.ViewModel;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.Scene;

public partial class SceneUserControl : UserControl
{
    private SceneViewModel _viewModel;
    private bool _pageIsCaptured;
    private Point _pointerPosition;

    public event Action? SomePartPressed;

    public SceneUserControl ()
    {
        InitializeComponent ();

        _pageIsCaptured = false;
    }

    internal void SomeButtonPressed ( object sender, PointerPressedEventArgs args )
    {
        SomePartPressed?.Invoke ();
    }

    internal void PagePressed ( object sender, PointerPressedEventArgs args )
    {
        PointerPoint point = args.GetCurrentPoint (sender as Control);

        if ( point.Properties.IsLeftButtonPressed )
        {
            Cursor = new Cursor (StandardCursorType.Hand);
            _pageIsCaptured = true;
            _pointerPosition = args.GetPosition (Scroller);
        }
    }

    internal void ReleasePage ( )
    {
        _pageIsCaptured = false;
        Cursor = new Cursor (StandardCursorType.Arrow);
    }

    internal void MovePage ( PointerEventArgs args )
    {
        if ( _pageIsCaptured )
        {
            Avalonia.Point newPosition = args.GetPosition ( Scroller );
            double horizontalDelta = _pointerPosition.X - newPosition.X;
            double verticalDelta = _pointerPosition.Y - newPosition.Y;
            _pointerPosition = newPosition;

            double horizontalOffset = Scroller.Offset.X;
            double verticalOffset = Scroller.Offset.Y;

            Scroller.Offset = new Vector ( horizontalOffset + horizontalDelta, verticalOffset + verticalDelta );
        }
    }
}
