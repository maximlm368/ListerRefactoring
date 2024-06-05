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
        private double _restWidth;
        private double _heightConstraint;
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
            _restWidth = width;
            _scale = scale;
            _heightConstraint = heightConstraint;
        }


        internal ActionSuccess AddBadge ( BadgeViewModel badge ) 
        {
            badge.SetCorrectScale ( _scale );

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
        FailureByWidth = 0,
        FailureByHeight = 1,
        Success = 2
    }
}
