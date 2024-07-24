using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using MessageBox.Avalonia.Views;

namespace Lister.Views
{
    public partial class MessageDialog : BaseWindow
    {
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
                    //message.Text = value;
                    _message = value;
                }
            }
        }


        public MessageDialog ()
        {
            InitializeComponent ();

            this.Icon = null;

            this.CanResize = false;

            //BorderBrush = new SolidColorBrush (MainWindow.black);
            //BorderThickness = new Avalonia.Thickness (1,1,1,1);
            


        }


        internal void HandleTapped ( object sender, TappedEventArgs args )
        {
            this.Close ();
        }
    }
}
