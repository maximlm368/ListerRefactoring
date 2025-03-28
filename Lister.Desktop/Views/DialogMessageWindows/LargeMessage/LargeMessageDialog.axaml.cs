using Avalonia.Controls;
using Avalonia.Input;
using Lister.Desktop.Views.DialogMessageWindows.LargeMessage.ViewModel;

namespace Lister.Desktop.Views.DialogMessageWindows.LargeMessage;

public sealed partial class LargeMessageDialog : Window
{
    private bool _messageIsSelected;
    private bool _pathIsSelected;


    public LargeMessageDialog ()
    {
        InitializeComponent ();
    }


    public LargeMessageDialog ( List<string> errors, string errorsSource ) : this ()
    {
        LargeMessageViewModel viewModel = new LargeMessageViewModel ();
        viewModel.Closed += () => Close ();

        DataContext = viewModel;

        Activated += (s,a) => ok.Focus (NavigationMethod.Tab, KeyModifiers.None);

        MainWindow.MainWindow.Window.ModalWindow = this;

        viewModel.Set (errors, errorsSource);
    }


    internal void TappedForWholeSelection ( object sender, TappedEventArgs args )
    {
        if ( !_messageIsSelected )
        {
            _messageIsSelected = true;
            SelectableTextBlock textBlock = sender as SelectableTextBlock;
            textBlock.SelectAll ();
        }
        else
        {
            _messageIsSelected = false;
        }
    }


    internal void TappedPathForWholeSelection ( object sender, TappedEventArgs args )
    {
        if ( !_pathIsSelected )
        {
            _pathIsSelected = true;
            TextBox textBox = sender as TextBox;
            textBox.SelectAll ();
        }
        else
        {
            _pathIsSelected = false;
        }
    }


    internal void MessageGotFocus ( object sender, GotFocusEventArgs args )
    {
        SelectableTextBlock textBlock = sender as SelectableTextBlock;
        textBlock.SelectAll ();
    }
}
