using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Lister.Desktop.Components.ButtonsBlock.ViewModel;

namespace Lister.Desktop.Components.ButtonsBlock;

public partial class ButtonsBlockUserControl : UserControl
{
    public static readonly StyledProperty<int> BadgeCountProperty =
        AvaloniaProperty.Register<ButtonsBlockUserControl, int> ( "BadgeCount" );
    public int BadgeCount
    {
        get => GetValue ( BadgeCountProperty );
        set => SetValue ( BadgeCountProperty, value );
    }

    public static readonly StyledProperty<int> IncorrectBadgeCountProperty =
        AvaloniaProperty.Register<ButtonsBlockUserControl, int> ( "IncorrectBadgeCount" );
    public int IncorrectBadgeCount
    {
        get => GetValue ( IncorrectBadgeCountProperty );
        set => SetValue ( IncorrectBadgeCountProperty, value );
    }

    public static readonly StyledProperty<bool> IsButtonsEnabledProperty =
        AvaloniaProperty.Register<ButtonsBlockUserControl, bool> ( "IsButtonsEnabled" );
    public bool IsButtonsEnabled
    {
        get => GetValue ( IsButtonsEnabledProperty );
        set => SetValue ( IsButtonsEnabledProperty, value );
    }

    public event Action? SomePartPressed;

    public ButtonsBlockUserControl()
    {
        InitializeComponent();
        DataContext = new ButtonsBlockViewModel ();
    }

    internal void SomeButtonPressed ( object sender, PointerPressedEventArgs args )
    {
        SomePartPressed?.Invoke ();
    }
}