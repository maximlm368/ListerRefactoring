using Avalonia.Controls;
using Avalonia.Interactivity;
using Lister.Desktop.App;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonSource.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.PersonSource;

public partial class PersonSourceUserControl : UserControl
{
    public PersonSourceUserControl ()
    {
        InitializeComponent ();

        openPicker.FocusAdorner = null;
        DataContext = ListerApp.Services.GetRequiredService<PersonSourceViewModel> ();
    }

    private void OnLoaded ( object sender, RoutedEventArgs args ) 
    {
        PersonSourceViewModel? viewModel = DataContext as PersonSourceViewModel;
        viewModel?.OnLoaded ();
    }
}
