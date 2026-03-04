using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Lister.Desktop.ModelMappings.BadgeVM;
using Lister.Desktop.Views.DialogMessageWindows.Dialog;
using Lister.Desktop.Views.MainWindow.EditionView.Parts.Edition.ViewModel;
using Lister.Desktop.Views.MainWindow.EditionView.Parts.Filter.ViewModel;
using Lister.Desktop.Views.MainWindow.EditionView.Parts.WorkArea.ViewModel;
using Lister.Desktop.Views.MainWindow.EditionView.ViewModel;
using Lister.Desktop.Views.MainWindow.SharedComponents.Navigator.ViewModel;
using Lister.Desktop.Views.MainWindow.SharedComponents.Zoomer.ViewModel;
using Lister.Desktop.Views.MainWindow.WaitingView.ViewModel;
using System.Diagnostics;

namespace Lister.Desktop.Views.MainWindow.EditionView;

/// <summary>
/// Is view for edition of current badge collection.
/// </summary>
public sealed partial class EditorViewUserControl : UserControl
{
    private static readonly string _promptQuestion = "Сохранить изменения и вернуться к макету?";
    private static bool _someControlPressed;

    private readonly Image? _focusedImage;
    private readonly Shape? _focusedShape;
    private readonly EditorViewModel? _viewModel;
    private readonly Stopwatch? _focusTime;
    private readonly TextBlock? _focusedText;
    private double _capturingY;
    private bool _runnerIsCaptured = false;
    private bool _capturedTextExists;
    private bool _capturedImageExists;
    private bool _capturedShapeExists;
    private bool _badgeIsCaptured;
    private Point _pointerOnBadgeComponent;
    private bool _isReleaseLocked;
    private bool _isTextEditorFocused;
    private bool _isZoomOnOutFocused;
    private double _dastinationPointer = 0;

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

        _viewModel.BackingActivated += CheckBacking;
    }

    private void PreventPasting ( object? sender, PointerReleasedEventArgs args )
    {
        args.Handled = true;
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

    private async void CheckBacking ()
    {
        if ( MainWindow.Window == null )
        {
            return;
        }

        _viewModel?.HandleDialogOpenig ();
        DialogWindow dialog = new ( _promptQuestion );
        dialog.Closed += ( s, a ) => _viewModel?.HandleDialogClosing ();

        bool result = await dialog.ShowDialog<bool> ( MainWindow.Window );

        if ( result )
        {
            _viewModel?.GoBack ();
        }
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
