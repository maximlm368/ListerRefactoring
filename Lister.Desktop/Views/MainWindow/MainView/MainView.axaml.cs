using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Lister.Desktop.ModelMappings.BadgeVM;
using Lister.Desktop.Views.MainWindow.EditionView;
using Lister.Desktop.Views.MainWindow.MainView.ViewModel;

namespace Lister.Desktop.Views.MainWindow.MainView;

/// <summary>
/// Is start and main view.
/// </summary>
public sealed partial class MainView : UserControl
{
    private static bool _widthIsChanged;

    internal static bool SomeControlPressed;
    internal static bool IsPathSet;
    internal static int TappedGoToEditorButton { get; private set; }
    internal static double ProperWidth { get; private set; }
    internal static double ProperHeight { get; private set; }

    private readonly MainViewModel? _viewModel;
    private List<BadgeViewModel>? _processableBadges;

    internal BadgeEditorView? EditorView { get; private set; }

    public MainView ()
    {
        InitializeComponent ();
    }

    public MainView ( MainViewModel viewModel ) : this ()
    {
        Waiting.Margin = new Avalonia.Thickness ( 0, 12 );
        ProperWidth = Width;
        ProperHeight = Height;
        DataContext = viewModel;
        _viewModel = viewModel;
        _viewModel.EditionIsChosen += EditIncorrectBadges;
        FocusAdorner = null;

        Loaded += OnLoaded;
        LayoutUpdated += LayoutUpdatedHandler;
        PointerPressed += PointerIsPressed;

        this.AddHandler ( UserControl.TappedEvent, PreventPasting, RoutingStrategies.Tunnel );
    }

    internal void LayoutUpdatedHandler ( object? sender, EventArgs args )
    {
        if ( TappedGoToEditorButton == 1 )
        {
            SwitchToEditor ();
        }
        else
        {
            _viewModel?.ProcessDocument ();
        }
    }

    private void SwitchToEditor ()
    {
        if ( EditorView == null )
        {
            return;
        }

        TappedGoToEditorButton = 0;
        MainWindow? window = MainWindow.GetMainWindow ();

        if ( window == null )
        {
            return;
        }

        Task task = new
        (
            () =>
            {
                if ( _processableBadges != null && _processableBadges.Count > 0 )
                {
                    EditorView.PrepareBy ( _processableBadges, this );
                }

                Dispatcher.UIThread.Invoke
                (
                    () =>
                    {
                        _viewModel?.EndWaiting ();
                        window.Content = EditorView;
                    }
                );
            }
        );

        task.Start ();
    }

    private void PreventPasting ( object? sender, TappedEventArgs args )
    {
        args.Handled = true;
    }

    private void PointerIsPressed ( object? sender, PointerPressedEventArgs args )
    {
        PointerPoint point = args.GetCurrentPoint ( sender as Control );

        if ( point.Properties.IsRightButtonPressed || point.Properties.IsLeftButtonPressed )
        {
            if ( !SomeControlPressed )
            {
                Focusable = true;
                this.Focus ();
            }

            SomeControlPressed = false;
        }

        Focusable = false;
    }

    internal void OnLoaded ( object? sender, RoutedEventArgs args )
    {
        _viewModel?.OnLoaded ();
    }

    internal void ReleaseCaptured ()
    {
        PersonChoosing.ReleaseScrollingLeverage ();
        Scene.ReleasePage ();
    }

    internal void ChangeSize ( double widthDifference, double heightDifference )
    {
        Width -= widthDifference;
        Height -= heightDifference;

        ProperWidth = Width;
        ProperHeight = Height;

        Scene.Width -= widthDifference;
        Scene.Height -= heightDifference;

        Scene.WorkArea.Width -= widthDifference;
        Scene.WorkArea.Height -= heightDifference;

        Waiting.ChangeSize ( heightDifference, widthDifference );

        PersonChoosing.AdjustComboboxWidth ( widthDifference, true );
        PersonChoosing.CloseCustomCombobox ();

        BadgeBuilding.ChangeSize ( widthDifference );

        ChangeSimpleShadow ( widthDifference );
    }

    private void ChangeSimpleShadow ( double widthDifference )
    {
        var logicChildren = SimpleShadow.GetVisualChildren ();

        foreach ( var child in logicChildren )
        {
            Rectangle rectangle = ( Rectangle ) child;
            rectangle.Width -= widthDifference;
        }
    }

    internal void SetProperSize ( double properWidth, double properHeight )
    {
        if ( properWidth != ProperWidth )
        {
            _widthIsChanged = true;
        }
        else
        {
            _widthIsChanged = false;
        }

        double widthDifference = Width - properWidth;
        double heightDifference = Height - properHeight;

        Width = properWidth;
        Height = properHeight;

        ProperWidth = Width;
        ProperHeight = Height;

        Scene.WorkArea.Width -= widthDifference;
        Scene.WorkArea.Height -= heightDifference;

        ChangeSimpleShadow ( widthDifference );
        BadgeBuilding.ChangeSize ( widthDifference );

        PersonChoosing.AdjustComboboxWidth ( widthDifference, _widthIsChanged );
    }

    internal void RefreshTemplateAppearences ()
    {
        _viewModel?.RefreshAppearences ();
    }

    private void EditIncorrectBadges ( List<BadgeViewModel> processableBadges )
    {
        _processableBadges = processableBadges;
        MainWindow? window = MainWindow.GetMainWindow ();

        if ( ( window != null ) && ( processableBadges.Count > 0 ) )
        {
            EditorView = new BadgeEditorView ( processableBadges.Count );
            EditorView.SetProperSize ( ProperWidth, ProperHeight );
            window.CancelSizeDifference ();
            TappedGoToEditorButton = 1;
            _viewModel?.WaitingWhileBuilding ();
        }
    }

    internal void MovePage ( PointerEventArgs args )
    {
        Scene.MovePage ( args );
    }
}
