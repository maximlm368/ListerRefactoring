using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Windows.Input;

namespace Lister.Desktop.Components.SuperButtonBlock;

public partial class SuperButtonBlockUserControl : UserControl
{
    private double _width;

    public static readonly StyledProperty<ICommand> CommandProperty = 
        AvaloniaProperty.Register<SuperButtonBlockUserControl, ICommand> ( "Command" );
    public ICommand Command
    {
        get => GetValue ( CommandProperty );
        set => SetValue ( CommandProperty, value );
    }

    public static readonly StyledProperty<string> ButtonInscriptionProperty = 
        AvaloniaProperty.Register<SuperButtonBlockUserControl, string> ( "ButtonInscription" );
    public string ButtonInscription
    {
        get => GetValue ( ButtonInscriptionProperty );
        set => SetValue ( ButtonInscriptionProperty, value );
    }

    public static readonly StyledProperty<string> ButtonPictureProperty =
        AvaloniaProperty.Register<SuperButtonBlockUserControl, string> ( "ButtonPicture" );
    public string ButtonPicture
    {
        get => GetValue ( ButtonPictureProperty );
        set => SetValue ( ButtonPictureProperty, value );
    }

    public static readonly StyledProperty<string> ToolTipProperty =
        AvaloniaProperty.Register<SuperButtonBlockUserControl, string> ( "ToolTip" );
    public string ToolTip
    {
        get => GetValue ( ToolTipProperty );
        set => SetValue ( ToolTipProperty, value );
    }

    public SuperButtonBlockUserControl()
    {
        InitializeComponent();
    }

    private void OnLoaded ( object sender, RoutedEventArgs args ) 
    {
        _width = Bounds.Width;
    }

    private void OnSizeChanged ( object sender, SizeChangedEventArgs args )
    {
        if ( !IsLoaded ) 
        {
            return;
        }

        double newWidth = args.NewSize.Width;
        double newWidthDifference = _width - newWidth;
        _width = newWidth;
        ChangeSize ( newWidthDifference );
    }

    internal void ChangeSize ( double widthDifference )
    {
        foreach ( Control? child in LeftShadow.Children )
        {
            if ( child.Width > 10 )
            {
                child.Width -= widthDifference / 2;
            }
            else
            {
                double left = child.GetValue ( Canvas.LeftProperty );
                child.SetValue ( Canvas.LeftProperty, left - widthDifference / 2 );
            }
        }

        foreach ( Control? child in RightShadow.Children )
        {
            if ( child.Width > 10 )
            {
                child.Width -= widthDifference / 2;
            }
        }
    }
}