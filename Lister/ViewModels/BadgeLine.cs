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


        //private ObservableCollection<Thickness> margin;
        //internal ObservableCollection<Thickness> Margin
        //{
        //    get { return margin; }
        //    set
        //    {
        //        this.RaiseAndSetIfChanged (ref margin, value, nameof (Margin));
        //    }
        //}


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
                    if ( badge.BadgeHeight > maxH ) 
                    {
                        maxH = badge.BadgeHeight;
                    }
                }

                return maxH;
            } 

            private set { maxH = value; } 
        }


        internal BadgeLine( double width, double scale, double heightConstraint ) 
        {
            Badges = new ObservableCollection<BadgeViewModel> ();
            _scale = scale;
            _restWidth = width;
            _heightConstraint = heightConstraint;
            int ff = 0;
        }


        private BadgeLine ( BadgeLine line )
        {
            Badges = new ObservableCollection<BadgeViewModel> ();
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
                Badges.Add (badge);
                _restWidth -= badge.BadgeWidth;
                Margin = new Thickness (_restWidth/2, 0, 0, 0);
                
                return ActionSuccess.Success;
            }
            int dfdf = 0;
        }


        internal void ZoomOn ( double scaleCoefficient ) 
        {
            _restWidth *= scaleCoefficient;
            _scale *= scaleCoefficient;
            double newMarginLeft = Margin.Left * scaleCoefficient;
            Margin = new Thickness (newMarginLeft, 0, 0, 0);

            for ( int index = 0;   index < Badges. Count;   index++ )
            {
                Badges [index].ZoomOn (scaleCoefficient);
            }
        }


        internal void ZoomOut ( double scaleCoefficient )
        {
            _restWidth /= scaleCoefficient;
            _scale /= scaleCoefficient;
            double newMarginLeft = Margin.Left / scaleCoefficient;
            Margin = new Thickness (newMarginLeft, 0, 0, 0);

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
