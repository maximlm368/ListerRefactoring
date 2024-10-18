using AnimatedImage.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
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

    private WaitingViewModel _vm;


    public WaitingView()
    {
        InitializeComponent();

        _vm = App.services.GetRequiredService <WaitingViewModel> ();
        this.DataContext = _vm;
    }


    //internal void HandleDialogOpenig ()
    //{
    //    _vm.HandleDialogOpenig ();
    //}


    //internal void HandleDialogClosing ()
    //{
    //    _vm.HandleDialogClosing ();
    //}


    //private void PreventPasting ( object sender, TappedEventArgs args )
    //{
    //    args.Handled = true;
    //}


    public void ChangeSize ( double heightDiff, double widthDiff )
    {
        _vm.ChangeSize ( heightDiff, widthDiff );
    }
}



