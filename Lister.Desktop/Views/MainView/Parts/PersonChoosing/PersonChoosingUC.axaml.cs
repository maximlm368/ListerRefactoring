using Avalonia.Controls;
using Avalonia.Interactivity;
using Lister.Desktop.Views.MainView.Parts.PersonChoosing.ViewModel;

namespace Lister.Desktop.Views.MainView.Parts.PersonChoosing;

public partial class PersonChoosingUserControl : UserControl
{
    private PersonChoosingViewModel? _viewModel;
    public event Action? SomePartPressed;

    public PersonChoosingUserControl ()
    {
        if ( Design.IsDesignMode )
        {
            Design.SetDataContext ( this, new PersonChoosingViewModel () );
        }

        InitializeComponent ();
        
        Loaded += OnLoaded;
        ActualThemeVariantChanged += ThemeChanged;
    }

    private void OnLoaded ( object? sender, RoutedEventArgs args )
    {
        _viewModel = DataContext as PersonChoosingViewModel;

        PART_FilterableCombobox.AreaIsPressed += () => 
        {
            SomePartPressed?.Invoke ();
        };

        _viewModel?.SetUp ();
    }

    private void ThemeChanged ( object? sender, EventArgs args )
    {
        if ( ActualThemeVariant == null )
        {
            return;
        }

        _viewModel?.SetUp ();
    }
}
