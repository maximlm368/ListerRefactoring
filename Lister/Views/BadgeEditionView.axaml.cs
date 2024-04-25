using Avalonia.Controls;
using Lister.ViewModels;

namespace Lister.Views
{
    public partial class BadgeEditionView : UserControl
    {
        internal BadgeEditionView (VMBadge beingEditedBadge)
        {
            this.DataContext = new BadgeEditionViewModel (beingEditedBadge);

            InitializeComponent();
        }
    }
}
