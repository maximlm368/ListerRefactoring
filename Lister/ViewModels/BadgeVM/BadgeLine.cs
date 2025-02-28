using Avalonia;
using Avalonia.Controls.Shapes;
using Core.Models.Badge;
using Core.DocumentBuilder;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Lister.ViewModels
{
    internal class BadgeLine : ReactiveObject
    {
        //private double _heightConstraint;
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

        private Avalonia.Thickness margin;
        internal Avalonia.Thickness Margin
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


        internal BadgeLine ( double scale, double heightConstraint, bool isFirst ) 
        {
            Badges = new ObservableCollection <BadgeViewModel> ();

            if ( isFirst )
            {
                Margin = new Avalonia.Thickness (0, 0, 0, 0);
            }
            else 
            {
                Margin = new Avalonia.Thickness (0, -1, 0, 0);
            }
            
            _scale = scale;
        }


        internal BadgeLine ( Core.DocumentBuilder.BadgeLine model, double scale )
        {
            Badges = new ();

            foreach ( Badge badge   in   model.Badges ) 
            {
                BadgeViewModel addable = new BadgeViewModel (badge);
                addable.SetCorrectScale (scale);

                Badges.Add ( addable );
            }


            //if ( isFirst )
            //{
            //    Margin = new Thickness ( 0, 0, 0, 0 );
            //}
            //else
            //{
            //    Margin = new Thickness ( 0, -1, 0, 0 );
            //}

            _scale = scale;
        }


        //private BadgeLine ( BadgeLine line )
        //{
        //    Badges = new ObservableCollection <BadgeViewModel> ();
        //    Margin = line.Margin;
        //    _scale = line._scale;

        //    foreach ( BadgeViewModel badge   in   line.Badges ) 
        //    {
        //        Badges.Add ( badge.GetDimensionalOriginal() );
        //    }
        //}


        //internal BadgeLine GetDimensionalOriginal () 
        //{
        //    BadgeLine original = new BadgeLine (this);
        //    return original;
        //}


        internal void ZoomOn ( double scaleCoefficient ) 
        {
            _scale *= scaleCoefficient;
            Height *= scaleCoefficient;

            for ( int index = 0;   index < Badges. Count;   index++ )
            {
                Badges [index].ZoomOn (scaleCoefficient);
            }
        }


        internal void ZoomOut ( double scaleCoefficient )
        {  
            _scale /= scaleCoefficient;
            Height /= scaleCoefficient;

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
