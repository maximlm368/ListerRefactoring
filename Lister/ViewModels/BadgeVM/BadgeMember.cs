using Avalonia;
using Avalonia.Media;
using AvaloniaEdit.Highlighting;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    abstract public class BadgeMember : ReactiveObject
    {
        protected double _scale = 1;

        private double _width;
        public double Width
        {
            get { return _width; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref _width, value, nameof (Width));
                WidthWithBorder = Width + BorderThickness.Left + BorderThickness.Right;
            }
        }

        private double _height;
        public double Height
        {
            get { return _height; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref _height, value, nameof (Height));
                HeightWithBorder = Height + BorderThickness.Top + BorderThickness.Bottom;
            }
        }

        private double _widthWithBorder;
        public double WidthWithBorder
        {
            get { return _widthWithBorder; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref _widthWithBorder, value, nameof (WidthWithBorder));
            }
        }

        private double _heightWithBorder;
        public double HeightWithBorder
        {
            get { return _heightWithBorder; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref _heightWithBorder, value, nameof (HeightWithBorder));
            }
        }

        private Thickness _borderThickness;
        public Thickness BorderThickness
        {
            get { return _borderThickness; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref _borderThickness, value, nameof (BorderThickness));
                LeftOffsetWithBorder = LeftOffset - BorderThickness.Left;
                TopOffsetWithBorder = TopOffset - BorderThickness.Top;
                HeightWithBorder = Height + BorderThickness.Top + BorderThickness.Bottom;
                WidthWithBorder = Width + BorderThickness.Left + BorderThickness.Right;
            }
        }

        private double _topOffset;
        public double TopOffset
        {
            get { return _topOffset; }
            set
            {
                this.RaiseAndSetIfChanged (ref _topOffset, value, nameof (TopOffset));
                TopOffsetWithBorder = TopOffset - BorderThickness.Top;
            }
        }

        private double _leftOffset;
        public double LeftOffset
        {
            get { return _leftOffset; }
            set
            {
                this.RaiseAndSetIfChanged (ref _leftOffset, value, nameof (LeftOffset));
                LeftOffsetWithBorder = LeftOffset - BorderThickness.Left;
            }
        }

        private double _topOffsetWithBorder;
        public double TopOffsetWithBorder
        {
            get { return _topOffsetWithBorder; }
            set
            {
                this.RaiseAndSetIfChanged (ref _topOffsetWithBorder, value, nameof (TopOffsetWithBorder));
            }
        }

        private double _leftOffsetWithBorder;
        public double LeftOffsetWithBorder
        {
            get { return _leftOffsetWithBorder; }
            set
            {
                this.RaiseAndSetIfChanged (ref _leftOffsetWithBorder, value, nameof (LeftOffsetWithBorder));
            }
        }

        private SolidColorBrush _borderColor;
        public SolidColorBrush BorderColor
        {
            get { return _borderColor; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _borderColor, value, nameof (BorderColor));
            }
        }


        protected void SetYourself ( double width, double height, double topOffset, double leftOffset ) 
        {
            Width = width;
            Height = height;
            TopOffset = topOffset;
            LeftOffset = leftOffset;
        }


        public void BecomeFocused ( SolidColorBrush borderColor, Thickness borderThickness )
        {
            BorderColor = borderColor;

            if ( borderColor == null ) 
            {
                BorderColor = new SolidColorBrush (new Color (255, 0, 0, 0));
            }

            BorderThickness = borderThickness;
        }


        public void BecomeUnFocused ()
        {
            BorderColor = null;
            BorderThickness = new Thickness (0, 0, 0, 0);
        }


        public void Mark ( SolidColorBrush borderColor, Thickness borderThickness )
        {
            BorderColor = borderColor;
            BorderThickness = borderThickness;
        }


        protected void ZoomOn ( double coefficient )
        {
            _scale *= coefficient;
            Width *= coefficient;
            Height *= coefficient;
            TopOffset *= coefficient;
            LeftOffset *= coefficient;

            //LeftOffset = Math.Round (LeftOffset * coefficient, 0);

        //    BorderThickness =
        //new Thickness (BorderThickness.Left * coefficient, BorderThickness.Top, BorderThickness.Right, BorderThickness.Bottom);
        }


        protected void ZoomOut ( double coefficient )
        {
            _scale /= coefficient;
            Width /= coefficient;
            Height /= coefficient;
            TopOffset /= coefficient;
            LeftOffset /= coefficient;

            //LeftOffset = Math.Round (LeftOffset / coefficient, 0);

            //   BorderThickness =
            //new Thickness (BorderThickness.Left / coefficient, BorderThickness.Top, BorderThickness.Right, BorderThickness.Bottom);
        }


        public void Shift ( Avalonia.Point newPoint )
        {
            TopOffset -= newPoint.Y;
            LeftOffset -= newPoint.X;
        }
    }
}
