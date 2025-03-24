using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonSource.ViewModel;
using Lister.Desktop.App;
using Avalonia.Interactivity;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.PersonSource;

public partial class PersonSourceUserControl : UserControl
{
    public PersonSourceUserControl ()
    {
        InitializeComponent ();

        openPicker.FocusAdorner = null;
        DataContext = ListerApp.Services.GetRequiredService<PersonSourceViewModel> ();
        PersonSourceViewModel viewModel = ( PersonSourceViewModel ) DataContext;
        Loaded += OnLoaded;
    }


    public void OnLoaded ( object sender, RoutedEventArgs args ) 
    {
        PersonSourceViewModel viewModel = DataContext as PersonSourceViewModel;
        viewModel?.OnLoaded ();
    }
}
