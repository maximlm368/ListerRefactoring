using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Lister.Desktop.Components.SuperButtonBlock;
using Lister.Desktop.Components.Navigator.ViewModel;
using System.Windows.Input;

namespace Lister.Desktop.Components.Navigator;

public partial class NavigatorUserControl : UserControl
{
    private int _currentNumber = 0;
    private NavigatorViewModel? _viewModel;

    public static readonly StyledProperty<bool> IsVisibleNumberEditableProperty =
        AvaloniaProperty.Register<NavigatorUserControl, bool> ( "IsVisibleNumberEditable" );
    public bool IsVisibleNumberEditable
    {
        get => GetValue ( IsVisibleNumberEditableProperty );
        set => SetValue ( IsVisibleNumberEditableProperty, value );
    }

    public static readonly StyledProperty<ICommand> ToFirstCommandProperty =
         AvaloniaProperty.Register<NavigatorUserControl, ICommand> ( "ToFirstCommand" );
    public ICommand ToFirstCommand
    {
        get => GetValue ( ToFirstCommandProperty );
        set => SetValue ( ToFirstCommandProperty, value );
    }

    public static readonly StyledProperty<ICommand> ToPreviousCommandProperty =
        AvaloniaProperty.Register<NavigatorUserControl, ICommand> ( "ToPreviousCommand" );
    public ICommand ToPreviousCommand
    {
        get => GetValue ( ToPreviousCommandProperty );
        set => SetValue ( ToPreviousCommandProperty, value );
    }

    public static readonly StyledProperty<ICommand> ToNextCommandProperty =
        AvaloniaProperty.Register<NavigatorUserControl, ICommand> ( "ToNextCommand" );
    public ICommand ToNextCommand
    {
        get => GetValue ( ToNextCommandProperty );
        set => SetValue ( ToNextCommandProperty, value );
    }

    public static readonly StyledProperty<ICommand> ToLastCommandProperty =
        AvaloniaProperty.Register<NavigatorUserControl, ICommand> ( "ToLastCommand" );
    public ICommand ToLastCommand
    {
        get => GetValue ( ToLastCommandProperty );
        set => SetValue ( ToLastCommandProperty, value );
    }

    public static readonly StyledProperty<string> ToFirstToolTipProperty =
        AvaloniaProperty.Register<SuperButtonBlockUserControl, string> ( "ToFirstToolTip" );
    public string ToFirstToolTip
    {
        get => GetValue ( ToFirstToolTipProperty );
        set => SetValue ( ToFirstToolTipProperty, value );
    }

    public static readonly StyledProperty<string> ToPreviousToolTipProperty =
        AvaloniaProperty.Register<SuperButtonBlockUserControl, string> ( "ToPreviousToolTip" );
    public string ToPreviousToolTip
    {
        get => GetValue ( ToPreviousToolTipProperty );
        set => SetValue ( ToPreviousToolTipProperty, value );
    }

    public static readonly StyledProperty<string> ToNextToolTipProperty =
        AvaloniaProperty.Register<SuperButtonBlockUserControl, string> ( "ToNextToolTip" );
    public string ToNextToolTip
    {
        get => GetValue ( ToNextToolTipProperty );
        set => SetValue ( ToNextToolTipProperty, value );
    }

    public static readonly StyledProperty<string> ToLastToolTipProperty =
        AvaloniaProperty.Register<SuperButtonBlockUserControl, string> ( "ToLastToolTip" );
    public string ToLastToolTip
    {
        get => GetValue ( ToLastToolTipProperty );
        set => SetValue ( ToLastToolTipProperty, value );
    }

    public event Action? SomePartPressed;

    public NavigatorUserControl()
    {
        InitializeComponent();
    }

    public void SetViewModel ( NavigatorViewModel viewModel )
    {
        DataContext = viewModel;
        _viewModel = viewModel;

        _viewModel.CurrentNumberChanged += ( value ) =>
        {
            _currentNumber = value;
            Number.Text = value.ToString ();
        };
    }

    private void OnLoaded ( object sender, RoutedEventArgs args )
    {
        if ( _currentNumber < 1 )
        {
            Number.Text = "1";
        }
    }

    private void SomeButtonPressed ( object sender, PointerPressedEventArgs args )
    {
        SomePartPressed?.Invoke ();
    }

    private void ToNumber ( object sender, TextChangedEventArgs args )
    {
        if ( !IsLoaded || _viewModel == null )
        {
            return;
        }

        if ( sender is TextBox textBox )
        {
            string? text = textBox.Text;

            if ( string.IsNullOrWhiteSpace ( text ) )
            {
                return;
            }

            bool isInt = Int32.TryParse ( text, out int currentNumber );

            if ( isInt && currentNumber > 0 && currentNumber <= _viewModel.Count )
            {
                _viewModel.ToNumber ( currentNumber );
                _currentNumber = currentNumber;
            }
            else
            {
                Number.Text = _currentNumber.ToString ();
            }
        }
    }

    private void CounterLostFocus ( object sender, RoutedEventArgs args )
    {
        if ( sender is TextBox textBox )
        {
            string? text = textBox.Text;

            if ( string.IsNullOrWhiteSpace ( text ) )
            {
                _viewModel?.RecoverPageCounterIfEmpty ();
                Number.Text = "1";
            }
        }
    }
}
