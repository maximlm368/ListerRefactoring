using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Lister.Desktop.Views.EditionView.Parts.Edition.ViewModel;

namespace Lister.Desktop.Views.EditionView.Parts.Edition;

public partial class EditionBlockUserControl : UserControl
{
    private EditionBlockViewModel? _viewModel;

    public EditionBlockUserControl()
    {
        InitializeComponent();

        DataContextChanged += ( sender, args ) =>
        {
            _viewModel = DataContext as EditionBlockViewModel;
        };

        EditorTextBox.AddHandler ( TextBox.PointerReleasedEvent, PreventPasting, RoutingStrategies.Tunnel );
    }

    private void PreventPasting ( object? sender, PointerReleasedEventArgs args )
    {
        args.Handled = true;
    }

    internal void ChangeFocusedFontSize ( object sender, KeyEventArgs args )
    {
        string key = args.Key.ToString ();

        if ( key == "Up" )
        {
            _viewModel?.IncreaseFontSize ();
        }
        else if ( key == "Down" )
        {
            _viewModel?.ReduceFontSize ();
        }
    }
}