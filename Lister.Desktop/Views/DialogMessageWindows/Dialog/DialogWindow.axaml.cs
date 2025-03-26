using Avalonia.Controls;
using Avalonia.Input;
using Lister.Desktop.App;
using Lister.Desktop.Views.DialogMessageWindows.Dialog.ViewModel;
using Lister.Desktop.Views.MainWindow;
using Lister.Desktop.Views.ViewBase;

namespace Lister.Desktop.Views.DialogMessageWindows.Dialog;

public sealed partial class DialogWindow : Window
{
    public readonly string YesStr = "yes";
    public readonly string NoStr = "no";

    private DialogViewModel _viewModel;
    private ShowingDialog _caller;

    private string _message;
    internal string Message
    {
        get
        {
            return _message;
        }

        set
        {
            if ( value != null )
            {
                message.Text = value;
                _message = value;
            }
        }
    }

    internal static bool IsOpen { get; set; }

    internal string Result { get; private set; }


    public DialogWindow ( )
    {
        InitializeComponent ();
    }


    public DialogWindow ( ShowingDialog caller ) : this()
    {
        _caller = caller;
        _viewModel = new DialogViewModel (this);
        DataContext = _viewModel;

        CanResize = false;

        Activated += delegate { Yes.Focus (NavigationMethod.Tab, KeyModifiers.None); };

        MainWin mainWindow = ListerApp.MainWindow as MainWin;
        mainWindow.ModalWindow = this;

        Yes.FocusAdorner = null;
        No.FocusAdorner = null;
    }


    internal void ChooseYes ()
    {
        Result = YesStr;
        this.Close ();
        _caller.HandleDialogClosing ();
    }


    internal void ChooseNo ()
    {
        Result = NoStr;
        this.Close ();
        _caller.HandleDialogClosing ();
    }
}
