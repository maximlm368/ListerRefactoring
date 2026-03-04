using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Lister.Desktop.Views.MainWindow.SharedComponents.Zoomer.ViewModel;
using System.Windows.Input;

namespace Lister.Desktop.Views.MainWindow.SharedComponents.Zoomer;

public partial class ZoomerUserControl : UserControl
{
    public static readonly StyledProperty<ICommand> ZoomOnCommandProperty =
    AvaloniaProperty.Register<ZoomerUserControl, ICommand> ( "ZoomOnCommand" );
    public ICommand ZoomOnCommand
    {
        get => GetValue ( ZoomOnCommandProperty );
        set => SetValue ( ZoomOnCommandProperty, value );
    }

    public static readonly StyledProperty<string> SuffixProperty =
        AvaloniaProperty.Register<ZoomerUserControl, string> ( "Suffix" );
    public string Suffix
    {
        get => GetValue ( SuffixProperty );
        set => SetValue ( SuffixProperty, value );
    }

    public static readonly StyledProperty<short> MaxZoomProperty =
        AvaloniaProperty.Register<ZoomerUserControl, short> ( "MaxZoom" );
    public short MaxZoom
    {
        get => GetValue ( MaxZoomProperty );
        set => SetValue ( MaxZoomProperty, value );
    }

    public static readonly StyledProperty<short> MinZoomProperty =
        AvaloniaProperty.Register<ZoomerUserControl, short> ( "MinZoom" );
    public short MinZoom
    {
        get => GetValue ( MinZoomProperty );
        set => SetValue ( MinZoomProperty, value );
    }

    public static readonly StyledProperty<short> StartZoomProperty =
        AvaloniaProperty.Register<ZoomerUserControl, short> ( "StartZoom" );
    public short StartZoom
    {
        get => GetValue ( StartZoomProperty );
        set => SetValue ( StartZoomProperty, value );
    }

    public event Action? SomePartPressed;

    public ZoomerUserControl()
    {
        InitializeComponent();
    }

    private void SomeButtonPressed ( object sender, PointerPressedEventArgs args )
    {
        SomePartPressed?.Invoke ();
    }

    public void SetViewModel ( ZoomerViewModel viewModel )
    {
        DataContext = viewModel;
    }
}