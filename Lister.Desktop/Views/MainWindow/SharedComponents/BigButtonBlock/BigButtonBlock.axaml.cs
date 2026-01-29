using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Windows.Input;

namespace Lister.Desktop.Views.MainWindow.MainView.SharedComponents;

public partial class BigButtonBlock : UserControl
{
    private double _width;

    public static readonly StyledProperty<ICommand> CommandProperty = AvaloniaProperty.Register<BigButtonBlock, ICommand> ( "Command" );
    public ICommand Command
    {
        get => GetValue ( CommandProperty );
        set => SetValue ( CommandProperty, value );
    }

    public static readonly StyledProperty<string> ButtonInscriptionProperty = 
        AvaloniaProperty.Register<BigButtonBlock, string> ( "ButtonInscription" );
    public string ButtonInscription
    {
        get => GetValue ( ButtonInscriptionProperty );
        set => SetValue ( ButtonInscriptionProperty, value );
    }

    public static readonly StyledProperty<string> ButtonPictureProperty =
        AvaloniaProperty.Register<BigButtonBlock, string> ( "ButtonPicture" );
    public string ButtonPicture
    {
        get => GetValue ( ButtonPictureProperty );
        set => SetValue ( ButtonPictureProperty, value );
    }

    public BigButtonBlock()
    {
        InitializeComponent();
    }

    private void OnLoaded ( object sender, RoutedEventArgs args ) 
    {
        _width = Bounds.Width;
        BackButton.Command = Command;
        Inscription.Content = ButtonInscription;
        Picture.Content = ButtonPicture;
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