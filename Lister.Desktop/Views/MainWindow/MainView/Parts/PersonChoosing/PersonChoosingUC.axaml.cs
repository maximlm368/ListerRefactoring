using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Lister.Desktop.App;
using Lister.Desktop.ModelMappings;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing;

public partial class PersonChoosingUserControl : UserControl
{
    private static SolidColorBrush? _comboboxItemBackground;
    private static SolidColorBrush? _comboboxItemBorderColor;
    private static IBrush? _currentComboboxItemBackground;
    private static IBrush? _currentComboboxItemBorderColor;

    private bool _personListIsDropped = false;
    private bool _dropdownButtonIsTapped = false;
    private bool _runnerIsCaptured = false;
    private bool _scrollingCausedByTapping = false;
    private bool _runnerShiftCaused = false;
    private double _capturingY;
    private readonly PersonChoosingViewModel _viewModel;
    private TemplateViewModel? _chosenTemplate;
    //private string? _theme;

    private readonly SolidColorBrush _selectedBorderColor = new ( new Color ( 255, 213, 232, 246 ) );
    private readonly SolidColorBrush _borderHovered = new ( new Color ( 255, 81, 76, 72 ) );
    private readonly SolidColorBrush _borderFocused = new ( new Color ( 255, 4, 111, 255 ) );
    private readonly SolidColorBrush _backgroundFocused = new ( new Color ( 255, 255, 255, 255 ) );
    private readonly IBrush? _borderDefault;
    private readonly SolidColorBrush _backgroundDefault = new ( new Color ( 255, 255, 255, 255 ) );
    private readonly SolidColorBrush _backgroundSelected = new ( new Color ( 255, 227, 241, 252 ) );

    public PersonChoosingUserControl ()
    {
        if ( Design.IsDesignMode )
        {
            Design.SetDataContext ( this, new PersonChoosingViewModel () );
        }

        InitializeComponent ();

        DataContext = ListerApp.Services.GetRequiredService<PersonChoosingViewModel> ();
        _viewModel = ( PersonChoosingViewModel ) DataContext;

        Loaded += OnLoaded;
        ActualThemeVariantChanged += ThemeChanged;

        personTextBox.AddHandler ( TextBox.PointerReleasedEvent, PreventPasting, RoutingStrategies.Tunnel );

        _borderDefault = personTextBox.BorderBrush;
        builtInBorder.BorderBrush = personTextBox.BorderBrush;
    }

    internal static void SetComboboxHoveredItemColors ( SolidColorBrush background, SolidColorBrush borderColor )
    {
        _comboboxItemBackground = background;
        _comboboxItemBorderColor = borderColor;
    }

    private void PreventPasting ( object sender, PointerReleasedEventArgs args )
    {
        args.Handled = true;
    }

    internal void AdjustComboboxWidth ( double shift, bool shouldChangeComboboxWidth )
    {
        personTextBox.Width -= shift;
        visiblePersons.Width -= shift;
        listFrame.Width -= shift;

        if ( shouldChangeComboboxWidth )
        {
            personList.Width -= shift;
            _viewModel.ShiftScroller ( shift );
        }
    }

    private void AcceptEntirePersonList ( object sender, PointerPressedEventArgs args )
    {
        _personListIsDropped = false;
        MainView.SomeControlPressed = true;
        _viewModel.SetEntireList ();
    }

    private void ScrollerPressed ( object sender, PointerPressedEventArgs args )
    {
        MainView.SomeControlPressed = true;
    }

    private void CustomComboboxGotFocus ( object sender, GotFocusEventArgs args )
    {
        builtInBorder.BorderBrush = _borderFocused;
        builtInButton.Background = _backgroundFocused;
        builtInBorder.BorderThickness = new Thickness ( 0, 2, 2, 2 );

        if ( personTextBox.Text == null )
        {
            return;
        }

        personTextBox.SelectionStart = personTextBox.Text.Length;
        personTextBox.SelectionEnd = personTextBox.Text.Length;
    }

    private void CustomComboboxLostFocus ( object sender, RoutedEventArgs args )
    {
        CloseCustomCombobox ();

        builtInBorder.BorderBrush = _borderDefault;
        builtInButton.Background = _backgroundDefault;
        builtInBorder.BorderThickness = new Thickness ( 0, 1, 1, 1 );
    }

    private void CustomComboboxPointerOver ( object sender, PointerEventArgs args )
    {
        if ( personTextBox.IsFocused )
        {
            builtInButton.Background = _backgroundFocused;
            builtInBorder.BorderBrush = _borderFocused;
        }
        else
        {
            builtInButton.Background = new SolidColorBrush ( new Color ( 255, 248, 248, 248 ) );
            builtInBorder.BorderBrush = _borderHovered;
        }
    }

    private void CustomComboboxPointerExited ( object sender, PointerEventArgs args )
    {
        if ( personTextBox.IsFocused )
        {
            builtInButton.Background = _backgroundFocused;
            builtInBorder.BorderBrush = _borderFocused;
        }
        else
        {
            builtInButton.Background = _backgroundDefault;
            builtInBorder.BorderBrush = _borderDefault;
        }
    }

    #region Drop
    internal void CloseCustomCombobox ()
    {
        if ( _personListIsDropped )
        {
            _personListIsDropped = false;
            _viewModel.HideDropListWithoutChange ();
        }
    }

    private void DropOrPickUpPersonsOrScroll ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();

        if ( key == "Return" )
        {
            DropOrPickUp ();

            return;
        }

        if ( key == "Escape" )
        {
            PickUp ();

            return;
        }

        if ( key == "Up" )
        {
            _viewModel.ScrollByKey ( true );

            return;
        }

        if ( key == "Down" )
        {
            _viewModel.ScrollByKey ( false );
        }
    }

    private void ButtonPressed ( object sender, PointerPressedEventArgs args )
    {
        _dropdownButtonIsTapped = true;
        MainView.SomeControlPressed = true;
    }

    private void ButtonReleased ( object sender, PointerReleasedEventArgs args )
    {
        DropOrPickUp ();
        personTextBox.Focus ( NavigationMethod.Tab );

        builtInButton.Background = _backgroundDefault;
    }

    private void DropOrPickUp ()
    {
        if ( personTextBox.Text == null )
        {
            return;
        }

        if ( _personListIsDropped )
        {
            _personListIsDropped = false;
            _viewModel.HideDropListWithChange ();
        }
        else
        {
            _personListIsDropped = true;
            _viewModel.ShowDropDown ();

            if ( _viewModel.EntireIsSelected )
            {
                allPersonsSign.Background = _backgroundSelected;
                allPersonsSign.BorderBrush = _selectedBorderColor;
            }
        }
    }

    private void PickUp ()
    {
        if ( _personListIsDropped )
        {
            _personListIsDropped = false;
            _viewModel.HideDropListWithoutChange ();
        }
    }
    #endregion Drop

    #region PersonListReduction
    private void HandlePersonListReduction ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();
        
        if ( !IsKeyImputable ( key ) )
        {
            return;
        }

        TextBox textBox = ( TextBox ) sender;
        string? input = textBox.Text;

        if ( input == null )
        {
            return;
        }

        _personListIsDropped = true;
        _viewModel.RefreshPersonList ( input );
    }

    private static bool IsKeyImputable ( string key )
    {
        bool result = key != "Tab"
            && ( key != "LeftShift" )
            && ( key != "RightShift" )
            && ( key != "Left" )
            && ( key != "Up" )
            && ( key != "Right" )
            && ( key != "Down" )
            && ( key != "Return" )
            && ( key != "Escape" );

        return result;
    }
    #endregion PersonListReduction

    #region ChoosingAndItemHovering
    private void HoverComboboxItem ( object sender, PointerEventArgs args )
    {
        if ( sender is Label hovered ) 
        {
            _currentComboboxItemBackground = hovered.Background;
            _currentComboboxItemBorderColor = hovered.BorderBrush;
            hovered.Background = _comboboxItemBackground;
            hovered.BorderBrush = _comboboxItemBorderColor;
        }
    }

    private void ExitComboboxItem ( object sender, PointerEventArgs args )
    {
        if ( _personListIsDropped && sender is Label hovered )
        {
            hovered.Background = _currentComboboxItemBackground;
            hovered.BorderBrush = _currentComboboxItemBorderColor;
        }
    }

    private void HandleChoosingByTapping ( object sender, PointerPressedEventArgs args )
    {
        MainView.SomeControlPressed = true;
        Label chosenLabel = ( Label ) sender;

        if ( chosenLabel.Content != null ) 
        {
            _viewModel.SetChosenPerson ( ( string ) chosenLabel.Content );
        }
    }
    #endregion

    #region Scrolling
    private void ScrollByWheel ( object sender, PointerWheelEventArgs args )
    {
        bool isDirectionUp = args.Delta.Y > 0;
        _viewModel.ScrollByWheel ( isDirectionUp );
    }

    private void ScrollByTapping ( object sender, PointerPressedEventArgs args )
    {
        if ( sender is Canvas trigger ) 
        {
            bool isDirectionUp = ( trigger.Name == "upper" );
            _scrollingCausedByTapping = true;
            _viewModel.ScrollByButton ( isDirectionUp );
        }
    }

    private void ShiftRunner ( object sender, PointerPressedEventArgs args )
    {
        if ( sender is Canvas trigger ) 
        {
            double limit;
            bool isDirectionUp = trigger.Name == "topSpan";

            if ( isDirectionUp )
            {
                limit = args.GetPosition ( trigger ).Y;
            }
            else
            {
                limit = bottomSpan.Height - args.GetPosition ( trigger ).Y;
            }

            _runnerShiftCaused = true;
            _viewModel.ShiftRunner ( isDirectionUp, limit );
        }
    }

    private void CaptureRunner ( object sender, PointerPressedEventArgs args )
    {
        PaintRunner ( 0x51, 0x4c, 0x48 );

        _runnerIsCaptured = true;
        Point inRunnerRelativePosition = args.GetPosition ( args.Source as Canvas );
        _capturingY = inRunnerRelativePosition.Y;
    }

    private void OverRunner ( object sender, PointerEventArgs args )
    {
        PaintRunner ( 0xd1, 0xd1, 0xd1 );
    }

    private void ExitedRunner ( object sender, PointerEventArgs args )
    {
        PaintRunner ( 0x81, 0x79, 0x74 );
    }

    internal void ReleaseScrollingLeverage ()
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
            _viewModel.StopScrolling ();
        }
        else
        {
            if ( !_runnerShiftCaused && !_dropdownButtonIsTapped )
            {
                CloseCustomCombobox ();
            }
            else
            {
                _runnerShiftCaused = false;
            }
        }

        _dropdownButtonIsTapped = false;
    }

    private void PaintRunner ( byte red, byte green, byte blue )
    {
        runner.Background = new SolidColorBrush ( new Color ( 255, red, green, blue ) );
    }

    private void MoveRunner ( object sender, PointerEventArgs args )
    {
        if ( _runnerIsCaptured )
        {
            Point pointerPosition = args.GetPosition ( args.Source as Canvas );
            double runnerVerticalDelta = _capturingY - pointerPosition.Y;
            _viewModel.MoveRunner ( runnerVerticalDelta );
        }
    }
    #endregion Scrolling

    //private void HandleClosing ( object sender, EventArgs args )
    //{
    //    if ( templateChoosing.SelectedItem is not TemplateViewModel chosen )
    //    {
    //        return;
    //    }

    //    if ( _chosenTemplate == chosen )
    //    {
    //        return;
    //    }

    //    _chosenTemplate = chosen;
    //    _viewModel.ChosenTemplate = chosen;
    //}

    private void OnLoaded ( object? sender, RoutedEventArgs args )
    {
        _viewModel.SetUp ( );
    }

    private void ThemeChanged ( object? sender, EventArgs args )
    {
        if ( ActualThemeVariant == null )
        {
            return;
        }

        _viewModel.SetUp ( );
    }
}
