using AnimatedImage.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Lister.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Lister.Views;

public partial class WaitingView : UserControl
{
    private static double _canvasHeight;
    private static double _canvasWidth;
    private static double _imageMarginTop;
    private static double _imageMarginLeft;
    private static double _thisMarginTop;


    public WaitingView()
    {
        InitializeComponent();

        Canvas.SetLeft (image, 250);
        Canvas.SetTop (image, 80);

        //WaitingViewModel vm = App.services.GetRequiredService<WaitingViewModel> ();
        //vm.PassView (this);

        //canvas.AddHandler (Canvas.TappedEvent, PreventPasting, RoutingStrategies.Direct);


        //var image = new BitmapImage ();
        //image.BeginInit ();
        //image.UriSource = new Uri (fileName);
        //image.EndInit ();
        //ImageBehavior.SetAnimatedSource (img, image);

        //image.Source = new AnimatedImageSourceUri (new Uri ("avares://Assets/Loading.gif"));



    }


    private void PreventPasting ( object sender, TappedEventArgs args )
    {
        args.Handled = true;
    }


    public void ChangeSize ( double heightDiff, double widthDiff )
    {
        canvas.Height -= heightDiff;
        canvas.Width -= widthDiff;
        _canvasHeight = canvas.Height;
        _canvasWidth = canvas.Width;

        Canvas.SetLeft (image, Canvas.GetLeft(image) - widthDiff/2);
        Canvas.SetTop (image, Canvas.GetTop (image) - heightDiff / 2);
        _imageMarginLeft = Canvas.GetLeft (image);
        _imageMarginTop = Canvas.GetTop (image);

        this.Margin = new Thickness (0, this.Margin.Top + heightDiff);
        _thisMarginTop = Margin. Top;
    }


    public void Recover ( )
    {
        canvas.Height = _canvasHeight;
        canvas.Width = _canvasWidth;

        Canvas.SetLeft (image, _imageMarginLeft);
        Canvas.SetTop (image, _imageMarginTop);

        this.Margin = new Thickness (0, _thisMarginTop);
    }


    public void ShowGif ()
    {
        WaitingViewModel vm = App.services.GetRequiredService<WaitingViewModel> ();
        vm.Show ();
    }
}



