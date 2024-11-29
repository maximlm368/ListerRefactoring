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
        private readonly double _correctionStep = 0.6;
        protected double _scale = 1;

        private double _width;
        public double Width
        {
            get { return _width; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref _width, value, nameof (Width));
            }
        }

        private double _height;
        public double Height
        {
            get { return _height; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref _height, value, nameof (Height));
            }
        }

        private double _topOffset;
        public double TopOffset
        {
            get { return _topOffset; }
            set
            {
                this.RaiseAndSetIfChanged (ref _topOffset, value, nameof (TopOffset));
            }
        }

        private double _leftOffset;
        public double LeftOffset
        {
            get { return _leftOffset; }
            set
            {
                this.RaiseAndSetIfChanged (ref _leftOffset, value, nameof (LeftOffset));
            }
        }


        protected void SetYourself ( double width, double height, double topOffset, double leftOffset ) 
        {
            Width = width;
            Height = height;
            TopOffset = topOffset;
            LeftOffset = leftOffset;
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
    }
}
