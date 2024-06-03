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
        private List <BadgeViewModel> incorrectBadges;
        internal List <BadgeViewModel> IncorrectBadges
        {
            set
            {
                incorrectBadges = value;

                if ( incorrectBadges.Count > 1 ) 
                {
                    BeingProcessedBadge = incorrectBadges [0];
                    BeingProcessedBadge.Show ();
                }
            }
        }



        private BadgeViewModel bpB;
        internal BadgeViewModel BeingProcessedBadge
        {
            get { return bpB; }
            set
            {
                this.RaiseAndSetIfChanged (ref bpB, value, nameof (BeingProcessedBadge));
            }
        }


        public BadgeEditorViewModel ( ){ }


        internal void PassIncorects ( List <BadgeViewModel> incorrects ) 
        {
            bool isNullOrEmpty = (incorrects == null)   ||   (incorrects.Count == 0);

            if ( isNullOrEmpty ) 
            {
                return;
            }

            IncorrectBadges = incorrects;
            BeingProcessedBadge = incorrects [0];
        }

    }
}
