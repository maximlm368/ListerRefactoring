using Avalonia.Input;
using MessageBox.Avalonia.Views;
using View.ViewBase;
using View.MainWindow;
using View.DialogMessageWindows.Message.ViewModel;
using View.App;

namespace View.DialogMessageWindows.Message;

public partial class MessageDialog : BaseWindow
{
    private MessageViewModel _vm;
    public ShowingDialog Caller { get; private set; }

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
                _message = value;
                _vm.Message = value;
            }
        }
    }


    public MessageDialog ()
    {
        InitializeComponent ();
    }


    public MessageDialog ( ShowingDialog caller, string message ) : this ()
    {
        Caller = caller;
        _vm = new MessageViewModel ();
        DataContext = _vm;
        CanResize = false;
        _vm.PassView (this);

        CanResize = false;

        Activated += delegate { ok.Focus (NavigationMethod.Tab, KeyModifiers.None); };

        MainWin mainWindow = ListerApp.MainWindow as MainWin;
        mainWindow.ModalWindow = this;

        Message = message;
        ok.FocusAdorner = null;
    }


    internal void Shut ()
    {
        Caller.HandleDialogClosing ();
        this.Close ();
    }
}