using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Lister.Desktop.App;
using Lister.Desktop.Views.MainWindow.WaitingView.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace Lister.Desktop.Views.MainWindow.WaitingView;

/// <summary>
/// Is view that is visible only while some asynchronous long time action like badge building or pdf creation is occurring.
/// </summary>
public sealed partial class WaitingView : UserControl
{
    private WaitingViewModel _viewModel;


    public WaitingView()
    {
        InitializeComponent ();

        DataContext = ListerApp.Services.GetRequiredService <WaitingViewModel> ();
        _viewModel = (WaitingViewModel) DataContext;
    }


    public void ChangeSize ( double heightDiff, double widthDiff )
    {
        _viewModel.ChangeSize ( heightDiff, widthDiff );
    }
}



