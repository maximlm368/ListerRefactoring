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
        private double _width;
        private double _restWidth;

        private ObservableCollection<BadgeViewModel> badges;
        internal ObservableCollection<BadgeViewModel> Badges
        {
            get { return badges; }
            set
            {
                this.RaiseAndSetIfChanged (ref badges, value, nameof (Badges));
            }
        }


        internal BadgeLine( double width ) 
        {
            _width = width;
            _restWidth = 0;
        }


        internal ActionSuccess AddBadge ( BadgeViewModel badge ) 
        {
            if ( _restWidth < badge.BadgeWidth ) 
            {
                return ActionSuccess.Failure;
            }
            else 
            {
                badges.Add (badge);
                _restWidth -= badge.BadgeWidth;
                return ActionSuccess.Success;
            }
        }


        internal void ZoomOn ( double scaleCoefficient ) 
        {
            _width *= scaleCoefficient;
            _restWidth *= scaleCoefficient;

            for ( int index = 0;   index < Badges. Count;   index++ )
            {
                Badges [index].ZoomOn (scaleCoefficient);
            }
        }


        internal void ZoomOut ( double scaleCoefficient )
        {
            _width /= scaleCoefficient;
            _restWidth /= scaleCoefficient;

            for ( int index = 0;   index < Badges. Count;   index++ )
            {
                Badges [index].ZoomOut (scaleCoefficient);
            }
        }
    }



    internal enum ActionSuccess 
    {
        Failure = 0,
        Success = 1
    }
}
