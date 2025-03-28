using Avalonia.Controls;
using Avalonia.Input;
using Lister.Desktop.Views.DialogMessageWindows.Message.ViewModel;

namespace Lister.Desktop.Views.DialogMessageWindows.Message;

public sealed partial class MessageWindow : Window
{
    public MessageWindow ()
    {
        InitializeComponent ();
    }


    public MessageWindow ( string message ) : this ()
    {
        MessageViewModel viewModel = new ( message );
        viewModel.Closed += () => Close ();
        DataContext = viewModel;

        Activated += (s,a) => ok.Focus ( NavigationMethod.Tab, KeyModifiers.None );
    }
}