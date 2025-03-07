using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using View.MainWindow.MainView.Parts.PersonChoosing.ViewModel;
using View.CoreModelReflection;
using View.App;
using Microsoft.Extensions.DependencyInjection;

namespace View.MainWindow.MainView.Parts.PersonChoosing;

public partial class PersonChoosingUserControl : UserControl
{
    private static SolidColorBrush _comboboxItemBackground;
    private static SolidColorBrush _comboboxItemBorderColor;

    private bool _personListIsDropped = false;
    private bool _dropdownButtonIsTapped = false;
    private bool _runnerIsCaptured = false;
    private bool _scrollingCausedByTapping = false;
    private bool _runnerShiftCaused = false;
    private double _capturingY;
    private double _shiftScratch = 0;
    private PersonChoosingViewModel _viewModel;
    private TemplateViewModel _chosenTemplate;
    private string _theme;

    private static IBrush _currentComboboxItemBackground;
    private static IBrush _currentComboboxItemBorderColor;

    SolidColorBrush _borderHovered = new SolidColorBrush ( new Color (255, 81, 76, 72) );
    SolidColorBrush _backgroundHovered = new SolidColorBrush (new Color (255, 213, 232, 246));

    SolidColorBrush _borderFocused = new SolidColorBrush (new Color (255, 4, 111, 255));
    SolidColorBrush _backgroundFocused = new SolidColorBrush (new Color (255, 255, 255, 255));

    IBrush _borderDefault;
    SolidColorBrush _backgroundDefault = new SolidColorBrush (new Color (255, 255, 255, 255));
    SolidColorBrush _backgroundSelected = new SolidColorBrush (new Color (255, 227, 241, 252));
    private readonly SolidColorBrush _selectedBorderColor = new SolidColorBrush (new Color (255, 213, 232, 246));


    public PersonChoosingUserControl ()
    {
        InitializeComponent ();

        DataContext = ListerApp.services.GetRequiredService <PersonChoosingViewModel> ();
        _viewModel = ( PersonChoosingViewModel ) DataContext;

        Loaded += OnLoaded;
        
        //ActualThemeVariantChanged += ThemeChanged;

        personTextBox.AddHandler (TextBox.PointerReleasedEvent, PreventPasting, RoutingStrategies.Tunnel);

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
            _viewModel.ShiftScroller (shift);
        }
    }


    internal void AcceptEntirePersonList ( object sender, PointerPressedEventArgs args )
    {
        _personListIsDropped = false;
        MainView.SomeControlPressed = true;
        _viewModel.SetEntireList ();
    }


    internal void ScrollerPressed ( object sender, PointerPressedEventArgs args )
    {
        MainView.SomeControlPressed = true;
    }


    internal void CustomComboboxGotFocus ( object sender, GotFocusEventArgs args )
    {
        builtInBorder.BorderBrush = _borderFocused;
        builtInButton.Background = _backgroundFocused;
        builtInBorder.BorderThickness = new Thickness (0, 2, 2, 2);

        if ( personTextBox.Text == null )
        {
            return;
        }

        personTextBox.SelectionStart = personTextBox.Text.Length;
        personTextBox.SelectionEnd = personTextBox.Text.Length;
    }


    internal void CustomComboboxLostFocus ( object sender, RoutedEventArgs args )
    {
        CloseCustomCombobox ();

        builtInBorder.BorderBrush = _borderDefault;
        builtInButton.Background = _backgroundDefault;
        builtInBorder.BorderThickness = new Thickness (0, 1, 1, 1);
    }


    internal void CustomComboboxPointerOver ( object sender, PointerEventArgs args )
    {
        if ( personTextBox.IsFocused )
        {
            builtInButton.Background = _backgroundFocused;
            builtInBorder.BorderBrush = _borderFocused;
        }
        else 
        {
            builtInButton.Background = new SolidColorBrush (new Color (255, 248, 248, 248));
            builtInBorder.BorderBrush = _borderHovered;
        }
    }


    internal void CustomComboboxPointerExited ( object sender, PointerEventArgs args )
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

    internal void CloseCustomCombobox ( )
    {
        if ( _personListIsDropped )
        {
            _personListIsDropped = false;
            _viewModel.HideDropListWithoutChange ();
        }
    }


    internal void DropOrPickUpPersonsOrScroll ( object sender, KeyEventArgs args )
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
            ScrollByKey (true);
            return;
        }

        if ( key == "Down" )
        {
            ScrollByKey (false);
        }
    }


    internal void ButtonPressed ( object sender, PointerPressedEventArgs args )
    {
        _dropdownButtonIsTapped = true;
        MainView.SomeControlPressed = true;
    }


    internal void ButtonReleased ( object sender, PointerReleasedEventArgs args )
    {
        DropOrPickUp ();
        personTextBox.Focus (NavigationMethod.Tab);

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

    internal void HandlePersonListReduction ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();
        bool keyIsUnimpacting = IsKeyUnimpacting (key);

        if ( keyIsUnimpacting )
        {
            return;
        }

        TextBox textBox = ( TextBox ) sender;
        string input = textBox.Text;

        if ( input == null ) 
        {
            return;
        }

        _personListIsDropped = true;
        _viewModel.ReductPersonList ( input );
    }


    private bool IsKeyUnimpacting ( string key )
    {
        bool keyIsUnimpacting = key == "Tab";
        keyIsUnimpacting = keyIsUnimpacting || ( key == "LeftShift" );
        keyIsUnimpacting = keyIsUnimpacting || ( key == "RightShift" );
        keyIsUnimpacting = keyIsUnimpacting || ( key == "Left" );
        keyIsUnimpacting = keyIsUnimpacting || ( key == "Up" );
        keyIsUnimpacting = keyIsUnimpacting || ( key == "Right" );
        keyIsUnimpacting = keyIsUnimpacting || ( key == "Down" );
        keyIsUnimpacting = keyIsUnimpacting || ( key == "Return" );
        keyIsUnimpacting = keyIsUnimpacting || ( key == "Escape" );
        return keyIsUnimpacting;
    }


    #endregion PersonListReduction

    #region ChoosingAndItemHovering

    internal void HoverComboboxItem ( object sender, PointerEventArgs args )
    {
        Label hovered = sender as Label;
        _currentComboboxItemBackground = hovered.Background;
        _currentComboboxItemBorderColor = hovered.BorderBrush;
        hovered.Background = _comboboxItemBackground;
        hovered.BorderBrush = _comboboxItemBorderColor;
    }


    internal void ExitComboboxItem ( object sender, PointerEventArgs args )
    {
        if ( _personListIsDropped )
        {
            Label hovered = sender as Label;
            hovered.Background = _currentComboboxItemBackground;
            hovered.BorderBrush = _currentComboboxItemBorderColor;
        }
    }


    internal void HandleChoosingByTapping ( object sender, PointerPressedEventArgs args )
    {
        MainView.SomeControlPressed = true;

        Label chosenLabel = ( Label ) sender;
        string chosenName = ( string ) chosenLabel.Content;
        _viewModel.SetChosenPerson (chosenName);
    }

    #endregion


    #region Scrolling

    internal void ScrollByWheel ( object sender, PointerWheelEventArgs args )
    {
        bool isDirectionUp = args.Delta.Y > 0;
        _viewModel.ScrollByWheel ( isDirectionUp );
    }


    internal void ScrollByTapping ( object sender, PointerPressedEventArgs args )
    {
        Canvas activator = sender as Canvas;
        bool isDirectionUp = (activator.Name == "upper");
        int count = personList.ItemCount;
        _scrollingCausedByTapping = true;
        _viewModel.ScrollByButton ( isDirectionUp, count );
    }


    internal void ShiftRunner ( object sender, PointerPressedEventArgs args )
    {
        Canvas activator = sender as Canvas;
        _shiftScratch = args.GetPosition ( activator ).Y;
        double limit = 0;
        bool isDirectionUp = activator.Name == "topSpan";

        if ( isDirectionUp ) 
        {
            limit = args.GetPosition (activator).Y;
        }
        else 
        {
            limit = bottomSpan.Height - args.GetPosition (activator).Y;
        }

        _runnerShiftCaused = true;
        _viewModel.ShiftRunner ( isDirectionUp, limit );
    }


    internal void CaptureRunner ( object sender, PointerPressedEventArgs args )
    {
        PaintRunner (0x51, 0x4c, 0x48);

        _runnerIsCaptured = true;
        Point inRunnerRelativePosition = args.GetPosition (( Canvas ) args.Source);
        _capturingY = inRunnerRelativePosition.Y;
    }


    internal void OverRunner ( object sender, PointerEventArgs args )
    {
        PaintRunner (0xd1, 0xd1, 0xd1);
    }


    internal void ExitedRunner ( object sender, PointerEventArgs args )
    {
        PaintRunner (0x81, 0x79, 0x74);
    }


    internal void ReleaseScrollingLeverage ( )
    {
        if ( _scrollingCausedByTapping    ||   _runnerIsCaptured )
        {
            if ( _runnerIsCaptured )
            {
                PaintRunner (0x81, 0x79, 0x74);
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
            if ( !_runnerShiftCaused   &&   !_dropdownButtonIsTapped )
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
        runner.Background = new SolidColorBrush (new Color (255, red, green, blue));
    }


    internal void MoveRunner ( object sender, PointerEventArgs args )
    {
        if ( _runnerIsCaptured ) 
        {
            Point pointerPosition = args.GetPosition (( Canvas ) args.Source);
            double runnerVerticalDelta = _capturingY - pointerPosition.Y;
            _viewModel.MoveRunner ( runnerVerticalDelta );
        }
    }


    private void ScrollByKey ( bool isDirectionUp )
    {
        _viewModel.ScrollByKey ( isDirectionUp );
    }

    #endregion Scrolling


    internal void HandleClosing ( object sender, EventArgs args )
    {
        TemplateViewModel chosen = templateChoosing.SelectedItem as TemplateViewModel;

        if ( chosen == null )
        {
            return;
        }

        if ( _chosenTemplate == chosen ) 
        {
            return;
        }

        _chosenTemplate = chosen;

        _viewModel.ChosenTemplate = chosen;
    }


    internal void OnLoaded ( object sender, RoutedEventArgs args )
    {
        _theme = ActualThemeVariant.Key.ToString ();
        _viewModel.SetUp (_theme);
    }


    internal void ThemeChanged ( object sender, EventArgs args )
    {
        if ( ActualThemeVariant == null )
        {
            return;
        }

        _theme = ActualThemeVariant.Key.ToString ();
        _viewModel.SetUp (_theme);
    }
}


