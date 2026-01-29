using Avalonia.Controls;
using Avalonia.Interactivity;
using Lister.Desktop.App;
using Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace Lister.Desktop.Views.MainWindow.MainView.Parts.PersonChoosing;

public partial class PersonChoosingUserControl : UserControl
{
    private readonly PersonChoosingViewModel _viewModel;
    public event Action? SomePartPressed;

    public PersonChoosingUserControl ()
    {
        if ( Design.IsDesignMode )
        {
            Design.SetDataContext ( this, new PersonChoosingViewModel () );
        }

        InitializeComponent ();

        DataContext = ListerApp.Services.GetRequiredService<PersonChoosingViewModel> ();
        _viewModel = ( PersonChoosingViewModel ) DataContext;

        Loaded += OnLoaded;
        ActualThemeVariantChanged += ThemeChanged;
    }

    private void OnLoaded ( object? sender, RoutedEventArgs args )
    {
        PART_FilterableCombobox.SomePartPressed += () => 
        {
            SomePartPressed?.Invoke ();
        };

        _viewModel.SetUp ();
    }

    private void ThemeChanged ( object? sender, EventArgs args )
    {
        if ( ActualThemeVariant == null )
        {
            return;
        }

        _viewModel.SetUp ();
    }
}
