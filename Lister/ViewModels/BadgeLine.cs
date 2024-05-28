using Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    internal class BadgeLine : ViewModelBase
    {
        private double _restWidth;
        private double _scale;

        private ObservableCollection <BadgeViewModel> badges;
        internal ObservableCollection <BadgeViewModel> Badges
        {
            get { return badges; }
            set
            {
                this.RaiseAndSetIfChanged (ref badges, value, nameof (Badges));
            }
        }

        private ObservableCollection <Thickness> margin;
        internal ObservableCollection <Thickness> Margin
        {
            get { return margin; }
            set
            {
                this.RaiseAndSetIfChanged (ref margin, value, nameof (Margin));
            }
        }


        internal BadgeLine( double width, double scale ) 
        {
            _restWidth = width;
            _scale = scale;
        }


        internal ActionSuccess AddBadge ( BadgeViewModel badge ) 
        {
            badge.SetCorrectScale ( _scale );

            if ( _restWidth < badge.BadgeWidth ) 
            {
                return ActionSuccess.Failure;
            }
            else 
            {
                Badges.Add (badge);
                _restWidth -= badge.BadgeWidth;
                return ActionSuccess.Success;
            }
        }


        internal void ZoomOn ( double scaleCoefficient ) 
        {
            _restWidth *= scaleCoefficient;
            _scale *= scaleCoefficient;

            for ( int index = 0;   index < Badges. Count;   index++ )
            {
                Badges [index].ZoomOn (scaleCoefficient);
            }
        }


        internal void ZoomOut ( double scaleCoefficient )
        {
            _restWidth /= scaleCoefficient;
            _scale /= scaleCoefficient;

            for ( int index = 0;   index < Badges. Count;   index++ )
            {
                Badges [index].ZoomOut (scaleCoefficient);
            }
        }


        internal void Show ( )
        {
            for ( int index = 0;   index < Badges. Count;   index++ )
            {
                Badges [index].Show ();
            }
        }


        internal void Hide ()
        {
            for ( int index = 0;   index < Badges. Count;   index++ )
            {
                Badges [index].Hide ();
            }
        }
    }



    internal enum ActionSuccess 
    {
        Failure = 0,
        Success = 1
    }
}
