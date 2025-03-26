using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Lister.Desktop.App;
using Lister.Desktop.ModelMappings;
using Lister.Desktop.ModelMappings.BadgeVM;
using Lister.Desktop.Views.MainWindow.EditionView;
using Lister.Desktop.Views.MainWindow.MainView.Parts.BuildButton.ViewModel;
using Lister.Desktop.Views.MainWindow.MainView.ViewModel;
using Lister.Desktop.Views.ViewBase;
using Microsoft.Extensions.DependencyInjection;

namespace Lister.Desktop.Views.MainWindow.MainView;

/// <summary>
/// Is start and main view.
/// </summary>
public sealed partial class MainView : ShowingDialog
{
    private static bool _widthIsChanged;
    private static bool _heightIsChanged;

    internal static bool SomeControlPressed;
    internal static bool IsPathSet;
    internal static MainView Instance { get; private set; }
    internal static int TappedGoToEditorButton { get; private set; }
    internal static double ProperWidth { get; private set; }
    internal static double ProperHeight { get; private set; }

    private MainViewModel _viewModel;
    private List <BadgeViewModel> _processableBadges;
    private PageViewModel _firstPage;

    internal BadgeEditorView EditorView { get; private set; }

    public MainView ()
    {
        InitializeComponent ();

        waiting.Margin = new Avalonia.Thickness(0,12);
        Instance = this;
        ProperWidth = Width;
        ProperHeight = Height;
        DataContext = ListerApp.Services.GetRequiredService<MainViewModel> ();
        _viewModel = ( MainViewModel ) DataContext;
        _viewModel.PassView (this);
        FocusAdorner = null;

        Loaded += OnLoaded;
        LayoutUpdated += LayoutUpdatedHandler;
        PointerPressed += PointerIsPressed;

        this.AddHandler (UserControl.TappedEvent, PreventPasting, RoutingStrategies.Tunnel);
    }


    public override void HandleDialogClosing ()
    {
        _viewModel.HandleDialogClosing ();
    }


    internal void LayoutUpdatedHandler ( object sender, EventArgs args )
    {
        _viewModel.LayoutUpdated ();
    }


    private void PreventPasting ( object sender, TappedEventArgs args )
    {
        args.Handled = true;
    }


    private void PointerIsPressed ( object sender, PointerPressedEventArgs args )
    {
        var point = args.GetCurrentPoint (sender as Control);

        if ( point.Properties.IsRightButtonPressed )
        {
            if ( ! SomeControlPressed ) 
            {
                Focusable = true;
                this.Focus ();
            }

            SomeControlPressed = false;
        }
        else if ( point.Properties.IsLeftButtonPressed ) 
        {
            if ( ! SomeControlPressed )
            {
                Focusable = true;
                this.Focus ();
            }

            SomeControlPressed = false;
        }

        Focusable = false;
    }


    internal void OnLoaded ( object sender, RoutedEventArgs args )
    {
        _viewModel.OnLoaded ();
    }


    internal void ReleaseCaptured () 
    {
        personChoosing.ReleaseScrollingLeverage ();
        scene.ReleasePage ();
    }


    internal void ChangeSize ( double widthDifference, double heightDifference )
    {
        Width -= widthDifference;
        Height -= heightDifference;

        ProperWidth = Width;
        ProperHeight = Height;

        scene.Width -= widthDifference;
        scene.Height -= heightDifference;

        scene.workArea.Width -= widthDifference;
        scene.workArea.Height -= heightDifference;

        waiting.ChangeSize (heightDifference, widthDifference);

        personChoosing.AdjustComboboxWidth (widthDifference, true);
        personChoosing.CloseCustomCombobox ();

        badgeBuilding.ChangeSize (widthDifference);

        ChangeSimpleShadow (widthDifference);
    }


    private void ChangeSimpleShadow ( double widthDifference )
    {
        var logicChildren = simpleShadow.GetVisualChildren ();

        foreach ( var child   in   logicChildren )
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

        if ( properHeight != ProperHeight )
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

        ProperWidth = Width;
        ProperHeight = Height;

        scene.workArea.Width -= widthDifference;
        scene.workArea.Height -= heightDifference;

        ChangeSimpleShadow (widthDifference);
        badgeBuilding.ChangeSize (widthDifference);

        personChoosing.AdjustComboboxWidth (widthDifference, _widthIsChanged);
    }


    internal void RefreshTemplateAppearences ( )
    {
        _viewModel.RefreshAppearences ();
    }


    internal void EditIncorrectBadges ( List <BadgeViewModel> processableBadges, PageViewModel firstPage )
    {
        _processableBadges = processableBadges;
        _firstPage = firstPage;
        MainView mainView = this;
        MainWin window = MainWin.GetMainWindow ();

        if ( ( window != null )   &&   ( processableBadges.Count > 0 ) )
        {
            EditorView = new BadgeEditorView ( BadgesBuildingViewModel.BuildingOccured, processableBadges.Count );
            EditorView.SetProperSize (ProperWidth, ProperHeight);
            window.CancelSizeDifference ();
            TappedGoToEditorButton = 1;
            _viewModel.SetWaitingUpdatingLayout ();
        }
    }


    internal void SwitchToEditor ( )
    {
        TappedGoToEditorButton = 0;
        MainWin window = MainWin.GetMainWindow ();

        Task task = new Task
        (
            () =>
            {
                EditorView.SetProcessableBadges ( _processableBadges, _firstPage );
                EditorView.PassBackPoint ( this );

                Dispatcher.UIThread.Invoke
                (
                    () =>
                    {
                        _viewModel.EndWaiting ();
                        window.Content = EditorView;
                    }
                );
            }
        );

        task.Start ();
    }


    internal void MovePage ( PointerEventArgs args )
    {
        scene.MovePage ( args );
    }
}




