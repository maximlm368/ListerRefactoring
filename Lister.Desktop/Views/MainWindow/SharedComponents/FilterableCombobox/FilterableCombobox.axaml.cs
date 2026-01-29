using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using System.Collections;

namespace Lister.Desktop.Views.MainWindow.MainView.SharedComponents;

public partial class FilterableCombobox : UserControl
{
    private static readonly Key [] _writableKeys = { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P, Key.A, Key.S, Key.D,
        Key.F, Key.G, Key.H, Key.J, Key.K, Key.L, Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M, Key.Oem1, Key.Oem102, Key.Oem2, Key.Oem3,
        Key.Oem4, Key.Oem5, Key.Oem6, Key.Oem7, Key.Oem8, Key.OemAttn, Key.OemAuto, Key.OemBackslash, Key.OemBackTab, Key.OemClear,
        Key.OemCloseBrackets, Key.OemComma, Key.OemCopy, Key.OemEnlw, Key.OemFinish, Key.OemMinus, Key.OemOpenBrackets, Key.OemPeriod,
        Key.OemPipe, Key.OemPlus, Key.OemQuestion, Key.OemQuotes, Key.OemSemicolon, Key.OemTilde, Key.D0, Key.D1, Key.D2, Key.D3, Key.D4,
        Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.Back, Key.Space };

    private readonly double _minRunnerHeight = 10;
    private readonly double _upperWidth = 16;
    private double _scrollValue;
    private double _runnerStep;
    private double _runnerLocation = 16;
    private bool _isTopAchieved;
    private Timer? _timer;
    private int _currentEdge;
    private int _currentNumber;
    private object? _currentItem;
    private double _itemHeight = 0;
    private double _runnerWalk;
    private readonly List<object> _itemsStorage = [];
    private readonly List<object> _items = [];
    private bool _isScrollable = false;
    private bool _isChangingInFilter;
    private bool _dropdownButtonIsTapped = false;
    private bool _runnerIsCaptured = false;
    private bool _scrollingCausedByTapping = false;
    private bool _runnerShiftCaused = false;
    private double _capturingY;

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = ItemsControl.ItemsSourceProperty.AddOwner<FilterableCombobox> ();
    public IEnumerable? ItemsSource
    {
        get => GetValue ( ItemsSourceProperty );
        set => SetValue ( ItemsSourceProperty, value );
    }

    public static readonly StyledProperty<int> VisibleCountProperty = AvaloniaProperty.Register<FilterableCombobox, int> ( "VisibleCount" );
    public int VisibleCount
    {
        get => GetValue ( VisibleCountProperty );
        set => SetValue ( VisibleCountProperty, value );
    }

    public static new readonly StyledProperty<double> FontSizeProperty = AvaloniaProperty.Register<FilterableCombobox, double> ( "FontSize" );
    public new double FontSize
    {
        get => GetValue ( FontSizeProperty );
        set => SetValue ( FontSizeProperty, value );
    }

    public static readonly StyledProperty<DataTemplate> ItemTemplateProperty =
        AvaloniaProperty.Register<FilterableCombobox, DataTemplate> ( "ItemTemplate" );
    public DataTemplate ItemTemplate
    {
        get => GetValue ( ItemTemplateProperty );
        set => SetValue ( ItemTemplateProperty, value );
    }

    public static readonly StyledProperty<DataTemplate> CurrenItemTemplateProperty =
        AvaloniaProperty.Register<FilterableCombobox, DataTemplate> ( "CurrentItemTemplate" );
    public DataTemplate CurrentItemTemplate
    {
        get => GetValue ( CurrenItemTemplateProperty );
        set => SetValue ( CurrenItemTemplateProperty, value );
    }

    public static readonly StyledProperty<DataTemplate> SelectedItemTemplateProperty =
        AvaloniaProperty.Register<FilterableCombobox, DataTemplate> ( "SelectedItemTemplate" );
    public DataTemplate SelectedItemTemplate
    {
        get => GetValue ( SelectedItemTemplateProperty );
        set => SetValue ( SelectedItemTemplateProperty, value );
    }

    public static readonly StyledProperty<DataTemplate> AllTemplateProperty =
        AvaloniaProperty.Register<FilterableCombobox, DataTemplate> ( "AllTemplate" );
    public DataTemplate AllTemplate
    {
        get => GetValue ( AllTemplateProperty );
        set => SetValue ( AllTemplateProperty, value );
    }

    public static readonly StyledProperty<DataTemplate> AllCurrentTemplateProperty =
    AvaloniaProperty.Register<FilterableCombobox, DataTemplate> ( "AllCurrentTemplate" );
    public DataTemplate AllCurrentTemplate
    {
        get => GetValue ( AllCurrentTemplateProperty );
        set => SetValue ( AllCurrentTemplateProperty, value );
    }

    public static readonly StyledProperty<DataTemplate> AllSelectedTemplateProperty =
        AvaloniaProperty.Register<FilterableCombobox, DataTemplate> ( "AllSelectedTemplate" );
    public DataTemplate AllSelectedTemplate
    {
        get => GetValue ( AllSelectedTemplateProperty );
        set => SetValue ( AllSelectedTemplateProperty, value );
    }

    public static readonly StyledProperty<string> DefaultPlaceholderProperty =
        AvaloniaProperty.Register<FilterableCombobox, string> ( "DefaulPlaceholder" );
    public string DefaultPlaceholder
    {
        get => GetValue ( DefaultPlaceholderProperty );
        set => SetValue ( DefaultPlaceholderProperty, value );
    }

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<FilterableCombobox, object?> ( "SelectedItem" );
    public object? SelectedItem
    {
        get => GetValue ( SelectedItemProperty );
        set
        {
            if ( !_isChangingInFilter )
            {
                PART_EditableTextBox.Text = value == null ? DefaultPlaceholder : value.ToString ();
                PART_EditableTextBox.CaretIndex = PART_EditableTextBox.Text != null ? PART_EditableTextBox.Text.Length : 0;
            }

            SetValue ( SelectedItemProperty, value );
        }
    }

    public event Action? SomePartPressed;

    public FilterableCombobox ()
    {
        InitializeComponent ();

        PART_EditableTextBox.AddHandler ( TextBox.PointerReleasedEvent, PreventPasting, RoutingStrategies.Tunnel );

        PropertyChanged += ( sender, args ) =>
        {
            if ( args.Property.Name == "ItemsSource" )
            {
                AdjustItems ();
            }
        };
    }

    private void PreventPasting ( object? sender, PointerReleasedEventArgs args )
    {
        args.Handled = true;
    }

    private void OnLoaded ( object sender, RoutedEventArgs args )
    {
        _currentEdge = VisibleCount - 1;

        if ( ItemsSource == null )
        {
            PART_EditableTextBox.IsReadOnly = true;
            PART_EditableTextBox.Focusable = false;

            return;
        }

        AdjustItems ();

        PART_ItemsControl.Loaded += ( sender, args ) =>
        {
            if ( PART_ItemsControl.ItemsPanelRoot == null )
            {
                return;
            }

            CalculateScrollBar ();

            foreach ( Control item in PART_ItemsControl.ItemsPanelRoot.Children )
            {
                item.Tapped += OnItemTapped;
            }

            UseTemplates ();
            PART_AllSign.ContentTemplate = SelectedItem == null && _currentNumber > -2 ? AllSelectedTemplate : AllTemplate;
        };
    }

    private void AdjustItems ()
    {
        if ( ItemsSource == null )
        {
            return;
        }

        PART_EditableTextBox.IsReadOnly = false;
        PART_EditableTextBox.Focusable = true;
        PART_ItemsControl.ItemTemplate = ItemTemplate;
        PART_AllSign.ContentTemplate = AllSelectedTemplate;

        _items.Clear ();
        _itemsStorage.Clear ();

        List<object> items = [];
        int counter = 0;

        foreach ( var item in ItemsSource )
        {
            if ( counter < VisibleCount )
            {
                items.Add ( item );
            }

            _itemsStorage.Add ( item );
            _items.Add ( item );

            counter++;
        }

        PART_ItemsControl.ItemsSource = items;
        SelectedItem = null;

        _currentNumber = -1;
        _currentItem = null;
        _scrollValue = 0;
        _currentEdge = Math.Min ( VisibleCount, _items.Count ) - 1;
    }

    private void CalculateScrollBar ()
    {
        if ( PART_ItemsControl.ItemsPanelRoot == null )
        {
            return;
        }

        Controls items = PART_ItemsControl.ItemsPanelRoot.Children;

        if ( items.Count < 1 )
        {
            scroller.Height = 0;
            scroller.Width = 0;
            PART_AllSign.Height = 0;
            PART_AllSign.IsVisible = false;

            return;
        }

        Control first = items.First ();

        if ( first is ContentPresenter presenter )
        {
            _itemHeight = presenter.Bounds.Height;
        }

        PART_AllSign.IsVisible = _items.Count == _itemsStorage.Count;
        PART_AllSign.Height = PART_AllSign.IsVisible ? PART_AllSign.Bounds.Height : 0;

        _isScrollable = _items.Count > VisibleCount;

        PART_HorizontalPanel.ColumnDefinitions [1].Width =
            _isScrollable ? new GridLength ( 16, GridUnitType.Pixel ) : new GridLength ( 0, GridUnitType.Pixel );

        scroller.Width = _isScrollable ? _upperWidth : 0;
        scroller.Height = items.Count * _itemHeight + PART_AllSign.Height;

        if ( _isScrollable )
        {
            _runnerWalk = _itemHeight * VisibleCount + PART_AllSign.Height - _upperWidth * 2;
            double proportion = _items.Count * _itemHeight / _runnerWalk;

            if ( proportion > 0 )
            {
                runner.Height = Math.Max ( _itemHeight * VisibleCount / proportion, _minRunnerHeight );
                _runnerStep = ( _runnerWalk - runner.Height ) / ( _items.Count - VisibleCount );
                Canvas.SetTop ( runner, _runnerLocation );
            }
        }
    }

    internal void CloseOpen ( object sender, TappedEventArgs args )
    {
        if ( PART_EditableTextBox.Text == null )
        {
            return;
        }

        PART_Popup.IsOpen = !PART_Popup.IsOpen;
        SelectedItem = _currentItem;
        PART_EditableTextBox.Focus ( NavigationMethod.Tab );

        if ( PART_Popup.IsOpen )
        {
            SetVisibleItems ();
        }
    }

    internal void TappedOnPopup ( object sender, PointerPressedEventArgs args )
    {
        SomePartPressed?.Invoke ();
    }

    private void OnLostFocus ( object sender, RoutedEventArgs args )
    {
        PART_Popup.IsOpen = false;
    }

    private void ManageByKey ( object? sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();

        if ( key == "Return" )
        {
            CloseOpen ();
        }
        else if ( key == "Escape" )
        {
            PART_Popup.IsOpen = false;
        }
        else if ( key == "Up" )
        {
            CompleteScrolling ( true );
        }
        else if ( key == "Down" )
        {
            CompleteScrolling ( false );
        }
    }

    private void CloseOpen ()
    {
        if ( PART_EditableTextBox.Text == null )
        {
            return;
        }

        if ( PART_Popup.IsOpen )
        {
            SelectedItem = _currentItem;
            PART_Popup.IsOpen = false;

            if ( SelectedItem == null && _currentNumber == -2 )
            {
                _currentNumber = -1;
            }
        }
        else
        {
            PART_Popup.IsOpen = true;

            SetVisibleItems ();
        }
    }

    private void OpenCloseButtonPressed ( object sender, PointerPressedEventArgs args )
    {
        SomePartPressed?.Invoke ();
    }

    private void RunnerReleased ( object sender, PointerReleasedEventArgs args )
    {
        if ( _scrollingCausedByTapping || _runnerIsCaptured )
        {
            if ( _runnerIsCaptured )
            {
                PaintRunner ( 0x81, 0x79, 0x74 );
                _runnerIsCaptured = false;
            }

            if ( _scrollingCausedByTapping )
            {
                _scrollingCausedByTapping = false;
            }

            _scrollingCausedByTapping = false;
        }
        else
        {
            if ( !_runnerShiftCaused && !_dropdownButtonIsTapped )
            {
                PART_Popup.IsOpen = false;
            }
            else
            {
                _runnerShiftCaused = false;
            }
        }

        _dropdownButtonIsTapped = false;
    }

    private void ScrollByWheel ( object sender, PointerWheelEventArgs args )
    {
        CompleteScrolling ( args.Delta.Y > 0 );
    }

    private void ScrollByTapping ( object sender, PointerPressedEventArgs args )
    {
        _scrollingCausedByTapping = true;

        if ( sender is Canvas trigger )
        {
            bool isDirectionUp = trigger.Name == "upper";
            _timer = new Timer ( new TimerCallback ( ShiftCaller ), isDirectionUp, 0, 100 );
        }
    }

    private void ShiftCaller ( object? isDirectionUp )
    {
        if ( isDirectionUp == null )
        {
            return;
        }

        Dispatcher.UIThread.Invoke (
            () =>
            {
                CompleteScrolling ( ( bool ) isDirectionUp );
            }
        );
    }

    private void StopRepeatScrolling ( object sender, PointerReleasedEventArgs args )
    {
        _timer?.Dispose ();
        _scrollingCausedByTapping = false;
    }

    private void ShiftRunner ( object sender, PointerPressedEventArgs args )
    {
        if ( _scrollingCausedByTapping )
        {
            return;
        }

        if ( sender is Canvas trigger )
        {
            double limit = args.GetPosition ( trigger ).Y;
            bool isDirectionUp = limit < Canvas.GetTop ( runner );

            while ( true )
            {
                if ( isDirectionUp )
                {
                    if ( Canvas.GetTop ( runner ) <= limit )
                    {
                        break;
                    }
                }
                else
                {
                    if ( ( Canvas.GetTop ( runner ) + runner.Height ) >= limit )
                    {
                        break;
                    }
                }

                CompleteScrolling ( isDirectionUp );
            }
        }
    }

    private void CaptureRunner ( object sender, PointerPressedEventArgs args )
    {
        PaintRunner ( 0x51, 0x4c, 0x48 );
        _runnerIsCaptured = true;
        _capturingY = args.GetPosition ( args.Source as Canvas ).Y;
    }

    private void OverRunner ( object sender, PointerEventArgs args )
    {
        PaintRunner ( 0xd1, 0xd1, 0xd1 );
    }

    private void ExitedRunner ( object sender, PointerEventArgs args )
    {
        PaintRunner ( 0x81, 0x79, 0x74 );
    }

    private void PaintRunner ( byte red, byte green, byte blue )
    {
        runner.Background = new SolidColorBrush ( new Color ( 255, red, green, blue ) );
    }

    private void ScrollByRunnerMoving ( object sender, PointerEventArgs args )
    {
        if ( _runnerIsCaptured )
        {
            Point position = args.GetPosition ( args.Source as Canvas );
            double runnerVerticalDelta = _capturingY - position.Y;
            ScrollByRunnerMoving ( runnerVerticalDelta );
        }
    }

    internal void ScrollByRunnerMoving ( double runnerStep )
    {
        double wholeScroll = _itemHeight * ( _items.Count - VisibleCount );
        double proportion = wholeScroll / _runnerWalk;
        int stepCount = ( int ) Math.Abs ( runnerStep * proportion / _itemHeight );

        for ( int index = 0; index < stepCount; index++ )
        {
            CompleteScrolling ( runnerStep > 0 );
        }
    }

    private void Dischoice ( object sender, PointerPressedEventArgs args )
    {
        _currentItem = null;
        CloseOpen ();
        SomePartPressed?.Invoke ();
    }

    internal void Filter ( object sender, KeyEventArgs args )
    {
        if ( ItemsSource == null || !_writableKeys.Contains ( args.Key ) )
        {
            return;
        }

        _runnerLocation = _upperWidth;
        PART_Popup.IsOpen = false;
        _items.Clear ();
        List<object> items = [];
        int counter = 0;

        foreach ( object? item in ItemsSource )
        {
            string? entireName = item.ToString ();

            if ( !string.IsNullOrWhiteSpace ( entireName ) &&
                PART_EditableTextBox.Text != null &&
                entireName.Contains ( PART_EditableTextBox.Text, StringComparison.CurrentCultureIgnoreCase )
            )
            {
                _items.Add ( item );

                if ( counter < VisibleCount )
                {
                    items.Add ( item );
                }

                counter++;
            }
        }

        PART_ItemsControl.ItemsSource = items;
        PART_Popup.IsOpen = true;

        _isChangingInFilter = true;
        SelectedItem = null;
        _isChangingInFilter = false;

        _currentNumber = PART_EditableTextBox.Text == string.Empty ? -2 : 0;
        _currentItem = PART_EditableTextBox.Text != string.Empty && items.Count > 0 ? items [0] : null;
        _currentEdge = Math.Min ( VisibleCount, _items.Count ) - 1;
        _scrollValue = 0;

        SetVisibleItems ();
    }

    private void CompleteScrolling ( bool isDirectionUp )
    {
        double currentScrollValue = _scrollValue;

        if ( isDirectionUp )
        {
            if ( PART_AllSign.IsVisible )
            {
                if ( _currentNumber > -1 )
                {
                    _currentNumber--;

                    if ( _currentNumber < ( _currentEdge - Math.Min ( VisibleCount, _items.Count ) + 1 ) )
                    {
                        _isTopAchieved = true;

                        PART_AllSign.ContentTemplate = AllCurrentTemplate;
                        _currentItem = null;

                        if ( _currentNumber < ( _currentEdge - Math.Min ( VisibleCount, _items.Count ) + 0 ) )
                        {
                            currentScrollValue += _itemHeight;

                            if ( currentScrollValue > PART_AllSign.Height )
                            {
                                currentScrollValue = PART_AllSign.Height;
                            }

                            if ( _isScrollable )
                            {
                                ShiftRunner ( true );
                            }

                            _currentEdge--;
                        }
                    }
                }
            }
            else
            {
                if ( _currentNumber > 0 )
                {
                    _currentNumber--;

                    if ( _currentNumber < ( _currentEdge - Math.Min ( VisibleCount, _items.Count ) + 1 ) )
                    {
                        currentScrollValue += _itemHeight;

                        if ( currentScrollValue > PART_AllSign.Height )
                        {
                            currentScrollValue = PART_AllSign.Height;
                        }

                        if ( _isScrollable )
                        {
                            ShiftRunner ( true );
                        }

                        _currentEdge--;
                    }
                }
            }

            if ( _currentNumber > -1 )
            {
                _currentItem = _isTopAchieved ? null : _items [_currentNumber];
            }
        }
        else
        {
            if ( _currentNumber < -1 )
            {
                _currentNumber++;
                PART_AllSign.ContentTemplate = AllCurrentTemplate;
            }
            else
            {
                if ( PART_AllSign.ContentTemplate == AllCurrentTemplate )
                {
                    PART_AllSign.ContentTemplate = AllTemplate;
                }

                _isTopAchieved = false;
                bool currentIsInRange = _currentNumber < ( _items.Count - 1 );

                if ( currentIsInRange )
                {
                    _currentNumber++;
                    _currentItem = _items [_currentNumber];

                    if ( _currentNumber > _currentEdge )
                    {
                        _currentEdge++;
                        currentScrollValue -= _itemHeight;

                        if ( _isScrollable )
                        {
                            ShiftRunner ( false );
                        }
                    }
                }
            }
        }

        _scrollValue = currentScrollValue;
        SetVisibleItems ();
    }

    private void SetVisibleItems ()
    {
        int scratch = ( int ) ( _scrollValue / _itemHeight ) * ( -1 );

        if ( scratch < 0 )
        {
            scratch = 0;
            _scrollValue = 0;
        }

        List<object> items = [];

        for ( int index = 0; index < Math.Min ( _items.Count, VisibleCount ); index++ )
        {
            object item = _items [scratch + index];
            items.Add ( item );
        }

        PART_ItemsControl.ItemsSource = items;
        UseTemplates ();
    }

    private void UseTemplates ()
    {
        if ( PART_ItemsControl.ItemsPanelRoot == null )
        {
            return;
        }

        foreach ( Control item in PART_ItemsControl.ItemsPanelRoot.Children )
        {
            if (
                item is ContentPresenter presenter &&
                presenter.Content == _currentItem
            )
            {
                presenter.ContentTemplate = CurrentItemTemplate;
            }

            if (
                item is ContentPresenter selected &&
                selected.Content == SelectedItem
            )
            {
                selected.ContentTemplate = SelectedItemTemplate;
            }

            item.Tapped += OnItemTapped;
        }
    }

    private void OnItemTapped ( object? sender, TappedEventArgs args )
    {
        if ( sender is ContentPresenter presenter )
        {
            int counter = 0;
            int currentNumber = -1;
            int newNumber = -1;

#pragma warning disable CS8602 // –азыменование веро€тной пустой ссылки.
            foreach ( object? item in PART_ItemsControl.ItemsSource )
            {
                if ( item == _currentItem )
                {
                    currentNumber = counter;
                }

                if ( item == presenter.Content )
                {
                    newNumber = counter;
                }

                if ( newNumber != -1 && currentNumber != -1 )
                {
                    break;
                }

                counter++;
            }

            _currentNumber += newNumber - currentNumber;
            SelectedItem = presenter.Content;
            _currentItem = presenter.Content;
            PART_Popup.IsOpen = false;
        }
    }

    private void ShiftRunner ( bool isDirectionUp )
    {
        double oldValue = Canvas.GetTop ( runner );
        Canvas.SetTop ( runner, oldValue + _runnerStep * ( isDirectionUp ? -1 : 1 ) );
        _runnerLocation = Canvas.GetTop ( runner );
    }
}
