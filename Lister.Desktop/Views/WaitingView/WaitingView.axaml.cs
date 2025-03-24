using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Lister.Desktop.App;
using Lister.Desktop.Views.WaitingView.ViewModel;

namespace Lister.Desktop.Views.WaitingView;

public partial class WaitingView : UserControl
{
    private WaitingViewModel _viewModel;


    public WaitingView()
    {
        InitializeComponent();

        DataContext = ListerApp.Services.GetRequiredService <WaitingViewModel> ();
        _viewModel = (WaitingViewModel) DataContext;
    }


    public void ChangeSize ( double heightDiff, double widthDiff )
    {
        _viewModel.ChangeSize ( heightDiff, widthDiff );
    }
}



