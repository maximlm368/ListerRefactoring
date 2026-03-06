using Avalonia.Controls;
using Avalonia.Interactivity;
using Lister.Desktop.Views.MainView.Parts.PersonSource.ViewModel;

namespace Lister.Desktop.Views.MainView.Parts.PersonSource;

public partial class PersonSourceUserControl : UserControl
{
    public PersonSourceUserControl ()
    {
        InitializeComponent ();

        openPicker.FocusAdorner = null;
    }

    private void OnLoaded ( object sender, RoutedEventArgs args ) 
    {
        PersonSourceViewModel? viewModel = DataContext as PersonSourceViewModel;
        viewModel?.OnLoaded ();
    }
}
