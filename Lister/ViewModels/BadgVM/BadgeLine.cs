using Avalonia;
using Avalonia.Controls;
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
        internal double _restWidth;
        private double _heightConstraint;
        internal double _scale;

        private ObservableCollection <BadgeViewModel> badges;
        internal ObservableCollection <BadgeViewModel> Badges
        {
            get { return badges; }
            set
            {
                this.RaiseAndSetIfChanged (ref badges, value, nameof (Badges));
            }
        }

        private Thickness margin;
        internal Thickness Margin
        {
            get { return margin; }
            set
            {
                this.RaiseAndSetIfChanged (ref margin, value, nameof (Margin));
            }
        }

        private double maxH;
        internal double Height 
        { 
            get 
            {
                foreach ( BadgeViewModel badge   in   Badges ) 
                {
                    if ( badge.BorderHeight > maxH ) 
                    {
                        maxH = badge.BorderHeight;
                    }
                }

                return maxH;
            } 

            private set { maxH = value; } 
        }


        internal BadgeLine( double width, double scale, double heightConstraint, bool isFirst ) 
        {
            Badges = new ObservableCollection <BadgeViewModel> ();

            if ( isFirst )
            {
                Margin = new Thickness (0, 0, 0, 0);
            }
            else 
            {
                Margin = new Thickness (0, -1, 0, 0);
            }
            
            _scale = scale;
            _restWidth = width;
            _heightConstraint = heightConstraint;
        }


        private BadgeLine ( BadgeLine line )
        {
            Badges = new ObservableCollection <BadgeViewModel> ();
            Margin = line.Margin;
            _scale = line._scale;
            _restWidth = line._restWidth / _scale;
            _heightConstraint = line._heightConstraint / _scale;

            foreach ( BadgeViewModel badge   in   line.Badges ) 
            {
                Badges.Add ( badge.GetDimensionalOriginal() );
            }
        }


        internal BadgeLine GetDimensionalOriginal () 
        {
            BadgeLine original = new BadgeLine (this);
            return original;
        }


        internal ActionSuccess AddBadge ( BadgeViewModel badge, bool shouldScale ) 
        {
            if ( shouldScale ) 
            {
                badge.SetCorrectScale (_scale);
            }

            bool isFailureByWidth = ( _restWidth < badge.BadgeWidth );
            bool isFailureByHeight = ( _heightConstraint < badge.BadgeHeight );

            if ( isFailureByWidth )
            {
                return ActionSuccess.FailureByWidth;
            }
            else if ( isFailureByHeight )
            {
                return ActionSuccess.FailureByHeight;
            }
            else
            {
                if ( Badges. Count == 0 )
                {
                    badge.Margin = new Thickness (0, 0, 0, 0);
                }
                else 
                {
                    badge.Margin = new Thickness (-1, 0, 0, 0);
                }

                Badges.Add (badge);
                _restWidth -= badge.BadgeWidth;
                //Margin = new Thickness (_restWidth / 2, 0, 0, 0);

                return ActionSuccess.Success;
            }
        }


        internal void ZoomOn ( double scaleCoefficient ) 
        {
            _restWidth *= scaleCoefficient;
            _scale *= scaleCoefficient;

            for ( int index = 0;   index < Badges. Count;   index++ )
            {
                Badges [index].ZoomOn (scaleCoefficient, false);
            }
        }


        internal void ZoomOut ( double scaleCoefficient )
        {
            _restWidth /= scaleCoefficient;
            _scale /= scaleCoefficient;

            for ( int index = 0;   index < Badges. Count;   index++ )
            {
                Badges [index].ZoomOut (scaleCoefficient, false);
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


        internal void Clone ()
        {
            for ( int index = 0;   index < Badges.Count;   index++ )
            {
                Badges [index].Clone ();
            }
        }


        internal void Clear ()
        {
            Badges.Clear ();
        }
    }



    internal enum ActionSuccess 
    {
        FailureByWidth = 0,
        FailureByHeight = 1,
        Success = 2
    }
}
