using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
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
        private double _scale = 2.8;

        private List <BadgeViewModel> _incorrectBadges;
        internal List <BadgeViewModel> IncorrectBadges
        {
            get { return _incorrectBadges; }
            set
            {
                bool isNullOrEmpty = ( value == null )   ||   ( value.Count == 0 );

                if ( isNullOrEmpty )
                {
                    return;
                }

                this.RaiseAndSetIfChanged (ref _incorrectBadges, value, nameof (IncorrectBadges));

                if ( value [0] == null )
                {
                    return;
                }

                BadgeViewModel beingPrecessed = value [0].GetDimensionalOriginal ();
                beingPrecessed.ZoomOn (_scale);
                beingPrecessed.Show ();
                BeingProcessedBadge = beingPrecessed;
                BeingProcessedNumber = 1;
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

        private int bpN;
        internal int BeingProcessedNumber
        {
            get { return bpN; }
            set
            {
                this.RaiseAndSetIfChanged (ref bpN, value, nameof (BeingProcessedNumber));
            }
        }


        public BadgeEditorViewModel ( )
        {
            IncorrectBadges = new List<BadgeViewModel> ();
        }


        internal void ToFirst ( )
        {

        }


        internal void ToPrevious ( )
        {

        }


        internal void ToNext ( )
        {

        }


        internal void ToLast ( )
        {

        }


        internal void ToParticularBadge ( int number )
        {

        }


        internal void MoveCaptured ( Point delta )
        {
            BeingProcessedBadge.TextLines [0].TopOffset -= delta.Y;
            BeingProcessedBadge.TextLines [0].LeftOffset -= delta.X;
            BeingProcessedBadge.TextLines [0].Width = 700;
            
        }


        //internal void ReleaseCaptured ( )
        //{
        //    BeingProcessedBadge.TextLines [0].TopOffset -= delta.Y;
        //    BeingProcessedBadge.TextLines [0].LeftOffset -= delta.X;
        //}
    }
}
