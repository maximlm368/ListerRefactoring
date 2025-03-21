using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using View.MainWindow.MainView.Parts.Scene.ViewModel;
using View.App;
using Microsoft.Extensions.DependencyInjection;

namespace View.MainWindow.MainView.Parts.Scene;

public partial class SceneUserControl : UserControl
{
    private bool _pageIsCaptured;
    private Point _pointerPosition;
    private SceneViewModel _viewModel;

    public SceneUserControl ()
    {
        InitializeComponent ();
        DataContext = ListerApp.Services.GetRequiredService<SceneViewModel> ();
        _viewModel = (SceneViewModel) DataContext;
        extender.FocusAdorner = null;
        edit.FocusAdorner = null;
        clearBadges.FocusAdorner = null;
        save.FocusAdorner = null;
        print.FocusAdorner = null;

        _pageIsCaptured = false;
    }


    internal void SomeButtonPressed ( object sender, PointerPressedEventArgs args )
    {
        MainView.SomeControlPressed = true;
    }


    internal void PagePressed ( object sender, PointerPressedEventArgs args )
    {
        PointerPoint point = args.GetCurrentPoint (sender as Control);

        if ( point.Properties.IsLeftButtonPressed )
        {
            Cursor = new Cursor (StandardCursorType.Hand);
            _pageIsCaptured = true;
            _pointerPosition = args.GetPosition (scroller);
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
            Avalonia.Point newPosition = args.GetPosition ( scroller );
            double horizontalDelta = _pointerPosition.X - newPosition.X;
            double verticalDelta = _pointerPosition.Y - newPosition.Y;
            _pointerPosition = newPosition;

            double horizontalOffset = scroller.Offset.X;
            double verticalOffset = scroller.Offset.Y;

            scroller.Offset = new Vector ( horizontalOffset + horizontalDelta, verticalOffset + verticalDelta );
        }
    }
}
