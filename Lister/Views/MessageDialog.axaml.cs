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
                    List<string> lines = value.SplitBySeparators (new List<char> () { '.' });

                    if ( lines.Count == 1 ) 
                    {
                        lines = new List<string> () { value };
                    }

                    List<string> pureLines = new List<string> ();

                    foreach ( string line   in   lines ) 
                    {
                        string pureLine = line.Trim ();
                        pureLines.Add ( pureLine );
                    }

                    _vm.MessageLines = pureLines;
                    _message = value;
                }
            }
        }


        public MessageDialog ( ShowingDialog caller )
        {
            InitializeComponent ();

            Caller = caller;
            _vm = new MessageViewModel ();
            DataContext = _vm;
            CanResize = false;
            _vm.PassView (this);

            CanResize = false;

            Activated += delegate { ok.Focus (NavigationMethod.Tab, KeyModifiers.None); };
        }


        internal void Shut ()
        {
            Caller.HandleDialogClosing ();
            this.Close ();
        }
    }
}
