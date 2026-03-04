using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Lister.Desktop.ModelMappings.BadgeVM;
using Lister.Desktop.Views.MainWindow.EditionView.Parts.Filter.ViewModel;

namespace Lister.Desktop.Views.MainWindow.EditionView.Parts.Filter;

public partial class FilterUserControl : UserControl
{
    private FilterViewModel? _viewModel;
    private double _capturingY;
    private bool _runnerIsCaptured = false;
    private double _dastinationPointer = 0;
    private static bool _someControlPressed;

    public static readonly StyledProperty<string> AllFilterProperty =
        AvaloniaProperty.Register<FilterUserControl, string> ( "AllFilter" );
    public string AllFilter
    {
        get => GetValue ( AllFilterProperty );
        set => SetValue ( AllFilterProperty, value );
    }

    public static readonly StyledProperty<string> CorrectFilterProperty =
        AvaloniaProperty.Register<FilterUserControl, string> ( "CorrectFilter" );
    public string CorrectFilter
    {
        get => GetValue ( CorrectFilterProperty );
        set => SetValue ( CorrectFilterProperty, value );
    }

    public static readonly StyledProperty<string> IncorrectFilterProperty =
        AvaloniaProperty.Register<FilterUserControl, string> ( "IncorrectFilter" );
    public string IncorrectFilter
    {
        get => GetValue ( IncorrectFilterProperty );
        set => SetValue ( IncorrectFilterProperty, value );
    }

    public static readonly StyledProperty<string> AllTipProperty =
    AvaloniaProperty.Register<FilterUserControl, string> ( "AllTip" );
    public string AllTip
    {
        get => GetValue ( AllTipProperty );
        set => SetValue ( AllTipProperty, value );
    }

    public static readonly StyledProperty<string> CorrectTipProperty =
        AvaloniaProperty.Register<FilterUserControl, string> ( "CorrectTip" );
    public string CorrectTip
    {
        get => GetValue ( CorrectTipProperty );
        set => SetValue ( CorrectTipProperty, value );
    }

    public static readonly StyledProperty<string> IncorrectTipProperty =
        AvaloniaProperty.Register<FilterUserControl, string> ( "IncorrectTip" );
    public string IncorrectTip
    {
        get => GetValue ( IncorrectTipProperty );
        set => SetValue ( IncorrectTipProperty, value );
    }

    public FilterUserControl ()
    {
        InitializeComponent ();

        FilterChoosing.SelectedIndex = 0;

        DataContextChanged += ( sender, args ) =>
        {
            _viewModel = DataContext as FilterViewModel;

            if ( _viewModel == null ) 
            {
                return;
            }

            _viewModel.ScrollerHided += ExtendItemsControl;
            _viewModel.ScrollerShowed += ShrinkItemsControl;

            _viewModel.SetNames ( [AllFilter, CorrectFilter, IncorrectFilter, AllTip, CorrectTip, IncorrectTip] );
        };

        Loaded += ( sender, args ) =>
        {
            FilterChoosing.SelectedIndex = 0;
        };
    }

    private void ExtendItemsControl ()
    {
        ScrollableItems.IsVisible = false;
        LargeItems.IsVisible = true;
        ScrollBar.IsVisible = false;
    }

    private void ShrinkItemsControl ()
    {
        ScrollableItems.IsVisible = true;
        LargeItems.IsVisible = false;
        ScrollBar.IsVisible = true;
    }

    internal void ScrollByWheel ( object sender, PointerWheelEventArgs args )
    {
        bool isDirectionUp = args.Delta.Y > 0;
        _viewModel?.ScrollByWheel ( isDirectionUp );
    }

    internal void SomeButtonPressed ( object sender, PointerPressedEventArgs args )
    {
        _someControlPressed = true;
    }

    internal void ToParticularBadge ( object sender, TappedEventArgs args )
    {
        if ( sender is Border border && border.DataContext is BadgeCorrectnessViewModel context )
        {
            _viewModel?.ToParticularBadge ( context );
        }
    }

    internal void ToParticularBadge ( object sender, KeyEventArgs args )
    {
        //if ( _isTextEditorFocused || _isZoomOnOutFocused )
        //{
        //    return;
        //}

        string key = args.Key.ToString ();

        //if ( key == "Up" )
        //{
        //    _viewModel?.ToPrevious ();
        //}
        //else if ( key == "Down" )
        //{
        //    _viewModel?.ToNext ();
        //}
    }

    internal void SelectionChanged ( object sender, SelectionChangedEventArgs args )
    {
        ComboBox? comboBox = sender as ComboBox;
        string? selected = comboBox?.SelectedValue as string;

        if ( comboBox != null && comboBox.IsLoaded )
        {
            _viewModel?.Filter ( selected );
        }
    }

    internal void ItemPointerEntered ( object sender, PointerEventArgs args )
    {
        if ( sender is not Border border )
        {
            return;
        }

        border.BorderBrush = new SolidColorBrush ( new Color ( 255, 37, 112, 167 ) );
    }

    internal void ItemPointerExited ( object sender, PointerEventArgs args )
    {
        if ( sender is not Border border )
        {
            return;
        }

        border.BorderBrush = null;
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

    private void Extend ( object sender, RoutedEventArgs args )
    {
        MainGrid.ColumnDefinitions [0].Width = new GridLength ( 212 );
        //MainGrid.ColumnDefinitions [1].Width = new GridLength ( 48 );
        LargeView.IsVisible = true;
        NarrowView.IsVisible = false;
    }

    private void Shrink ( object sender, RoutedEventArgs args )
    {
        MainGrid.ColumnDefinitions [0].Width = new GridLength ( 0 );
        //MainGrid.ColumnDefinitions [1].Width = new GridLength ( 48 );
        LargeView.IsVisible = false;
        NarrowView.IsVisible = true;
    }
}
