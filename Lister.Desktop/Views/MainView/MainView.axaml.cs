using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Lister.Desktop.Components.ButtonsBlock.ViewModel;
using Lister.Desktop.Components.Navigator.ViewModel;
using Lister.Desktop.Components.Zoomer.ViewModel;
using Lister.Desktop.Views.MainView.ViewModel;

namespace Lister.Desktop.Views.MainView;

/// <summary>
/// Is start and main view.
/// </summary>
public sealed partial class MainViewUserControl : UserControl
{
    internal static bool IsPathSet;
    internal static double ProperWidth { get; private set; }
    internal static double ProperHeight { get; private set; }
    internal static int TappedGoToEditorButton { get; private set; }

    private bool _someControlPressed;

    internal MainViewModel? ViewModel { get; private set; }

    public MainViewUserControl ()
    {
        InitializeComponent ();
    }

    public MainViewUserControl ( MainViewModel viewModel ) : this ()
    {
        ProperWidth = Width;
        ProperHeight = Height;

        DataContext = viewModel;
        ViewModel = viewModel;

        PersonChoosing.DataContext = ViewModel.PersonChoosing;
        PersonSource.DataContext = ViewModel.PersonSource;
        Scene.DataContext = ViewModel.Scene;
        Waiting.DataContext = ViewModel.Waiting;

        NavigatorViewModel navigator = new ();
        ZoomerViewModel zoomer = new ( Zoomer.Suffix, Zoomer.MaxZoom, Zoomer.MinZoom, Zoomer.StartZoom );

        Navigator.SetViewModel ( navigator );
        Zoomer.SetViewModel ( zoomer );

        ViewModel.SetNavigator ( navigator );
        ViewModel.SetZoomer ( zoomer );
        ViewModel.SetButtonsBlock ( ButtonsBlock.DataContext as ButtonsBlockViewModel );

        Loaded += OnLoaded;
        LayoutUpdated += LayoutUpdatedHandler;
        PointerPressed += PointerIsPressed;

        this.AddHandler ( UserControl.TappedEvent, PreventPasting, RoutingStrategies.Tunnel );

        PersonChoosing.SomePartPressed += () => 
        {
            _someControlPressed = true;
        };
    }

    internal void LayoutUpdatedHandler ( object? sender, EventArgs args )
    {
        ViewModel?.ProcessDocument ();
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
            if ( !_someControlPressed )
            {
                Focusable = true;
                this.Focus ();
            }

            _someControlPressed = false;
        }

        Focusable = false;
    }

    internal void OnLoaded ( object? sender, RoutedEventArgs args )
    {
        ViewModel?.CheckIfPersonSourceCorrect ();
    }

    internal void ReleaseCaptured ()
    {
        Scene.ReleasePage ();
    }

    internal void ChangeSize ( double widthDifference, double heightDifference )
    {
        Width -= widthDifference;
        Height -= heightDifference;

        ProperWidth = Width;
        ProperHeight = Height;

        Scene.WorkArea.Width -= widthDifference;
        Scene.WorkArea.Height -= heightDifference;
    }

    internal void SetProperSize ( double properWidth, double properHeight )
    {
        double widthDifference = Width - properWidth;
        double heightDifference = Height - properHeight;

        Width = properWidth;
        Height = properHeight;

        ProperWidth = Width;
        ProperHeight = Height;

        Scene.WorkArea.Width -= widthDifference;
        Scene.WorkArea.Height -= heightDifference;

        BuildButton.ChangeSize ( widthDifference );
    }

    internal void Show ()
    {
        ViewModel?.Show ();
    }

    internal void MovePage ( PointerEventArgs args )
    {
        Scene.MovePage ( args );
    }
}
