using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Lister.ViewModels;
using MessageBox.Avalonia.Views;
using ReactiveUI;

namespace Lister.Views
{
    public partial class DialogWindow : BaseWindow
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

        internal string Result { get; private set; }


        public DialogWindow ()
        {
            InitializeComponent ();

            _viewModel = new DialogViewModel (this);
            DataContext = _viewModel;
            CanResize = false;
            No.Focus (NavigationMethod.Tab, KeyModifiers.None);
            message.FontWeight = Avalonia.Media.FontWeight.SemiBold;
        }


        internal void ChooseYes ()
        {
            Result = yes;
            this.Close ();
        }


        internal void ChooseNo ()
        {
            Result = no;
            this.Close ();
        }
    }
}
