using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using View.App;
using View.WaitingView.ViewModel;

namespace View.WaitingView;

public partial class WaitingView : UserControl
{
    private WaitingViewModel _viewModel;


    public WaitingView()
    {
        InitializeComponent();

        DataContext = ListerApp.services.GetRequiredService <WaitingViewModel> ();
        _viewModel = (WaitingViewModel) DataContext;
    }


    public void ChangeSize ( double heightDiff, double widthDiff )
    {
        _viewModel.ChangeSize ( heightDiff, widthDiff );
    }
}



