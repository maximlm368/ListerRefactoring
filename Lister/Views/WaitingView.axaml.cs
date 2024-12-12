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
    private WaitingViewModel _viewModel;


    public WaitingView()
    {
        InitializeComponent();

        DataContext = App.services.GetRequiredService <WaitingViewModel> ();
        _viewModel = (WaitingViewModel) DataContext;
    }


    public void ChangeSize ( double heightDiff, double widthDiff )
    {
        _viewModel.ChangeSize ( heightDiff, widthDiff );
    }
}



