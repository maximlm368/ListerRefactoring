using Avalonia.Controls;
using Avalonia.Input;
using Lister.Desktop.Views.DialogMessageWindows.Dialog.ViewModel;

namespace Lister.Desktop.Views.DialogMessageWindows.Dialog;

public sealed partial class DialogWindow : Window
{
    public DialogWindow ( )
    {
        InitializeComponent ();
    }


    public DialogWindow ( string question ) : this()
    {
        DialogViewModel viewModel = new (question);
        viewModel.YesChosen += ChooseYes;
        viewModel.NoChosen += ChooseNo;
        DataContext = viewModel;

        Activated += ( s, a ) => { Yes.Focus ( NavigationMethod.Tab, KeyModifiers.None ); };

        MainWindow.MainWindow.Window.ModalWindow = this;
    }


    internal void ChooseYes ()
    {
        Close (true);
    }


    internal void ChooseNo ()
    {
        Close (false);
    }
}
