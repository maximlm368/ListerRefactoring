using Avalonia.Controls;

namespace Lister.Views
{
    public partial class OpenerCloserButton : Button
    {
        public bool isOpeningDone;

        public OpenerCloserButton()
        {
            InitializeComponent();
            isOpeningDone = false;
        }
    }
}
