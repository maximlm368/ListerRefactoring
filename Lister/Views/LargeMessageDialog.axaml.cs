using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using DocumentFormat.OpenXml.Wordprocessing;
using ExtentionsAndAuxiliary;
using Lister.ViewModels;
using MessageBox.Avalonia.Views;

namespace Lister.Views
{
    public partial class LargeMessageDialog : BaseWindow
    {
        private LargeMessageViewModel _viewModel;
        public ShowingDialog Caller { get; private set; }

        private bool _messageIsSelected;

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

                    _viewModel.MessageLines = pureLines;
                    _message = value;
                }
            }
        }


        public LargeMessageDialog ()
        {
            InitializeComponent ();
        }


        public LargeMessageDialog ( ShowingDialog caller, List<string> errors, string errorsSource ) : this ()
        {
            Caller = caller;
            _viewModel = new LargeMessageViewModel ();
            DataContext = _viewModel;
            CanResize = false;
            _viewModel.PassView (this);

            CanResize = false;
            ok.FocusAdorner = null;
            

            Activated += delegate { ok.Focus (NavigationMethod.Tab, KeyModifiers.None); };

            MainWindow mainWindow = App.MainWindow;
            mainWindow.ModalWindow = this;

            List<string> messageLines = null;
            _viewModel.Set (errors, errorsSource);
        }


        internal List<string> ConvertToLinesForStandard ( string message )
        {
            List<string> pureLines = new List<string> ();

            if ( message == null )
            {
                return pureLines;
            }

            List<string> lines = message.SplitBySeparators (new List<char> () { '.' });

            if ( lines.Count == 1 )
            {
                lines = new List<string> () { message };
            }

            foreach ( string line   in   lines )
            {
                string pureLine = line.Trim ();
                pureLines.Add (pureLine);
            }

            return pureLines;
        }


        internal void Shut ()
        {
            Caller.HandleDialogClosing ();
            this.Close ();
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
    }
}
