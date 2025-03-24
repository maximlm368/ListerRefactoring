using Avalonia.Input;
using MessageBox.Avalonia.Views;
using Lister.Desktop.Views.ViewBase;
using Lister.Desktop.Views.MainWindow;
using Lister.Desktop.Views.DialogMessageWindows.Message.ViewModel;
using Lister.Desktop.App;

namespace Lister.Desktop.Views.DialogMessageWindows.Message;

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
        MainWin mainWindow = ListerApp.MainWindow as MainWin;

        if ( mainWindow != null ) 
        {
            mainWindow.ModalWindow = this;
        }

        Message = message;
        ok.FocusAdorner = null;

        Activated += delegate { ok.Focus ( NavigationMethod.Tab, KeyModifiers.None ); };
    }


    internal void Shut ()
    {
        Caller.HandleDialogClosing ();
        this.Close ();
    }
}