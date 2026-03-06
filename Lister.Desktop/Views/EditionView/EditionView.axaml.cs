using Avalonia.Controls;
using Avalonia.Input;
using Lister.Desktop.Components.Navigator.ViewModel;
using Lister.Desktop.Components.Zoomer.ViewModel;
using Lister.Desktop.Entities.BadgeVM;
using Lister.Desktop.Views.EditionView.Parts.Edition.ViewModel;
using Lister.Desktop.Views.EditionView.Parts.Filter.ViewModel;
using Lister.Desktop.Views.EditionView.Parts.WorkArea.ViewModel;
using Lister.Desktop.Views.EditionView.ViewModel;
using Lister.Desktop.Views.WaitingView.ViewModel;

namespace Lister.Desktop.Views.EditionView;

/// <summary>
/// Is view for edition of current badge collection.
/// </summary>
public sealed partial class EditorViewUserControl : UserControl
{
    private static bool _someControlPressed;

    private readonly EditorViewModel? _viewModel;

    public EditorViewUserControl ()
    {
        InitializeComponent ();

        EditionBlockViewModel editionBlockViewModel = new ();
        WorkAreaViewModel workAreaViewModel = new ();
        FilterViewModel filterViewModel = new ();
        NavigatorViewModel navigator = new ();
        ZoomerViewModel zoomer = new ( Zoomer.Suffix, Zoomer.MaxZoom, Zoomer.MinZoom, Zoomer.StartZoom );
        WaitingViewModel waiting = new ();

        Editor.DataContext = editionBlockViewModel;
        WorkArea.DataContext = workAreaViewModel;
        Filter.DataContext = filterViewModel;
        Navigator.SetViewModel ( navigator );
        Zoomer.SetViewModel ( zoomer );
        Waiting.DataContext = waiting;

        _viewModel = new ( editionBlockViewModel, workAreaViewModel, filterViewModel, navigator, zoomer, waiting );
        DataContext = _viewModel;
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

    internal void ChangeSize ( double widthDifference, double heightDifference )
    {
        Width -= widthDifference;
        Height -= heightDifference;

        WorkArea.ChangeSize ( heightDifference, widthDifference );
    }

    internal void SetProperSize ( double properWidth, double properHeight )
    {
        double widthDifference = Width - properWidth;
        Width = properWidth;
        Height = properHeight;

        BackButton.ChangeSize ( widthDifference );
    }

    internal void PrepareBy ( List<BadgeViewModel> processables )
    {
        _viewModel?.SetProcessables ( processables );
    }

    internal void ToSide ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();
        _viewModel?.FocusedToSide ( key );
    }

    internal void ReleaseCaptured ()
    {
        WorkArea.ReleaseCaptured ();
    }

    internal void MoveBadge ( PointerEventArgs args )
    {
        WorkArea.MoveBadge ( args );
    }
}
