using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    internal class BadgeEditionViewModel : ViewModelBase
    {
        internal VMBadge badge { get; private set; }
        internal double textStackTopShift { get; private set; }
        internal double textStackLeftShift { get; private set; }


        public BadgeEditionViewModel ( VMBadge badge)
        {
            this.badge = badge;
            textStackLeftShift = badge.badgeModel. badgeDescription. badgeDimensions. personTextAreaLeftShiftOnBackground * 3;
            textStackTopShift = badge.badgeModel. badgeDescription.badgeDimensions. personTextAreaTopShiftOnBackground * 3;
        }


    }
}
