using Avalonia.Controls;
using Avalonia.Input;

namespace Lister.Views
{
    public partial class MessageDialog : Window
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
                    message.Content = value;
                    _message = value;
                }
            }
        }


        public MessageDialog ()
        {
            InitializeComponent ();
        }


        internal void HandleTapped ( object sender, TappedEventArgs args )
        {
            this.Close ();
        }
    }
}
