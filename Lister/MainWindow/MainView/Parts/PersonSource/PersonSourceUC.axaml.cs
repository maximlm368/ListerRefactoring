using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using View.MainWindow.MainView.Parts.PersonSource.ViewModel;
using View.App;

namespace View.MainWindow.MainView.Parts.PersonSource;

public partial class PersonSourceUserControl : UserControl
{
    public PersonSourceUserControl ()
    {
        InitializeComponent ();
        openPicker.FocusAdorner = null;
        DataContext = ListerApp.services.GetRequiredService<PersonSourceViewModel> ();
        PersonSourceViewModel viewModel = (PersonSourceViewModel) DataContext;
        viewModel.OnLoaded ();
    }
}
