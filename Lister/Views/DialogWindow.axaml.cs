using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using ExCSS;
using Lister.ViewModels;
using MessageBox.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace Lister.Views
{
    public partial class DialogWindow : Window
    {
        //internal DialogWindow ()
        //{
        //    InitializeComponent ();

        //    DataContext = new DialogViewModel ();

        //    this.CanResize = false;

        //    this.ExtendClientAreaToDecorationsHint = false;

        //    this.WhenActivated (action => action (ViewModel!.ChooseYes.Subscribe (Close)));
        //    this.WhenActivated (action => action (ViewModel!.ChooseNo.Subscribe (Close)));

        //    yes.CornerRadius = new Avalonia.CornerRadius ();
        //    no.CornerRadius = new Avalonia.CornerRadius ();
        //}

        //ReactiveWindow<DialogViewModel>

        public readonly string yes = "yes";
        public readonly string no = "no";

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

            MainWindow mainWindow = App.MainWindow as MainWindow;
            mainWindow.ModalWindow = this;
        }


        internal void ChooseYes ()
        {
            Result = yes;
            this.Close ();
            _caller.HandleDialogClosing ();
        }


        internal void ChooseNo ()
        {
            Result = no;
            this.Close ();
            _caller.HandleDialogClosing ();
        }



    }
}
