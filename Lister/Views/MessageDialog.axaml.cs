using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ExtentionsAndAuxiliary;
using Lister.ViewModels;
using MessageBox.Avalonia.Views;

namespace Lister.Views
{
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

            MainWindow mainWindow = App.MainWindow as MainWindow;
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
}