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


        internal void SetIncorrectBadges ( List<VMBadge> incorrectBadges ) 
        {
            BadgeEditorViewModel viewModel = (BadgeEditorViewModel) DataContext;
            //viewModel.IncorrectBadges = incorrectBadges;
        }
    }
}
