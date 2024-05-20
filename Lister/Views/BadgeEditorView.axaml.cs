using Avalonia.Controls;
using Lister.ViewModels;

namespace Lister.Views
{
    internal partial class BadgeEditorView : UserControl
    {
        internal BadgeEditorView ()
        {
            InitializeComponent ();
            this.DataContext = new BadgeEditorViewModel ();
        }


        internal void SetIncorrectBadges ( List<VMBadge> incorrectBadges ) 
        {
            BadgeEditorViewModel viewModel = (BadgeEditorViewModel) DataContext;
            viewModel.IncorrectBadges = incorrectBadges;
        }
    }
}
