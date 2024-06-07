using Avalonia.Controls;
using Lister.ViewModels;

namespace Lister.Views
{
    public partial class BadgeEditorView : UserControl
    {
        public BadgeEditorView ()
        {
            InitializeComponent ();
            this.DataContext = new BadgeEditorViewModel ();
        }


        internal void PassIncorrectBadges ( List<BadgeViewModel> incorrects ) 
        {
            BadgeEditorViewModel viewModel = (BadgeEditorViewModel) DataContext;
            viewModel.SetIncorects(incorrects);
        }
    }
}
