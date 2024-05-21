using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    public class BadgeEditorViewModel : ViewModelBase
    {
        private List<VMBadge> incorrectBadges;
        internal List<VMBadge> IncorrectBadges
        {
            set
            {
                incorrectBadges = value;

                if ( incorrectBadges.Count > 1 ) 
                {
                    BeingProcessedBadge = incorrectBadges [0];
                    BeingProcessedBadge.ShowBackgroundImage ();
                }
            }
        }



        private VMBadge bpB;
        internal VMBadge BeingProcessedBadge
        {
            get { return bpB; }
            set
            {
                this.RaiseAndSetIfChanged (ref bpB, value, nameof (BeingProcessedBadge));
            }
        }


        public BadgeEditorViewModel ( )
        {
            
        }


    }
}
