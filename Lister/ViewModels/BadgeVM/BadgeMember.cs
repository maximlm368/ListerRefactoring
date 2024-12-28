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
    abstract public class BadgeMember : ViewModelBase
    {
        protected double _scale = 1;

        private double _width;
        public double Width
        {
            get { return _width; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref _width, value, nameof (Width));
                WidthWithBorder = Width + 2;
            }
        }

        private double _height;
        public double Height
        {
            get { return _height; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref _height, value, nameof (Height));
                HeightWithBorder = Height + 2;
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

        private double _topOffset;
        public double TopOffset
        {
            get { return _topOffset; }
            set
            {
                this.RaiseAndSetIfChanged (ref _topOffset, value, nameof (TopOffset));
                TopOffsetWithBorder = TopOffset - 1;
            }
        }

        private double _leftOffset;
        public double LeftOffset
        {
            get { return _leftOffset; }
            set
            {
                this.RaiseAndSetIfChanged (ref _leftOffset, value, nameof (LeftOffset));
                LeftOffsetWithBorder = LeftOffset - 1;
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

        protected SolidColorBrush outlineColorStorage;
        private SolidColorBrush _outLineColor;
        public SolidColorBrush OutLineColor
        {
            get { return _outLineColor; }
            private set
            {
                //if ( value != null ) 
                //{
                //    Red = value.Color.R;
                //    Green = value.Color.G;
                //    Blue = value.Color.B;
                //}

                this.RaiseAndSetIfChanged (ref _outLineColor, value, nameof (OutLineColor));
            }
        }

        //private byte _red;
        //internal byte Red
        //{
        //    get { return _red; }
        //    private set { _red = value; }
        //}

        //private byte _green;
        //internal byte Green
        //{
        //    get { return _green; }
        //    private set { _green = value; }
        //}

        //private byte _blue;
        //internal byte Blue
        //{
        //    get { return _blue; }
        //    private set { _blue = value; }
        //}


        protected void SetYourself ( double width, double height, double topOffset, double leftOffset
                                                                                  , SolidColorBrush outLineColor ) 
        {
            Width = width;
            Height = height;
            TopOffset = topOffset;
            LeftOffset = leftOffset;
            OutLineColor = null;
            outlineColorStorage = outLineColor;
        }


        public void BecomeFocused ( )
        {
            OutLineColor = outlineColorStorage;
        }


        public void BecomeUnFocused ()
        {
            OutLineColor = null;
        }


        protected void ZoomOn ( double coefficient )
        {
            _scale *= coefficient;
            Width *= coefficient;
            Height *= coefficient;
            TopOffset *= coefficient;
            LeftOffset *= coefficient;
        }


        protected void ZoomOut ( double coefficient )
        {
            _scale /= coefficient;
            Width /= coefficient;
            Height /= coefficient;
            TopOffset /= coefficient;
            LeftOffset /= coefficient;
        }


        public void Shift ( Avalonia.Point newPoint )
        {
            TopOffset -= newPoint.Y;
            LeftOffset -= newPoint.X;
        }
    }
}
