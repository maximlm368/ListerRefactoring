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

        private double wd;
        public double Width
        {
            get { return wd; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref wd, value, nameof (Width));
            }
        }

        private double ht;
        public double Height
        {
            get { return ht; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref ht, value, nameof (Height));
            }
        }

        private double tof;
        public double TopOffset
        {
            get { return tof; }
            set
            {
                this.RaiseAndSetIfChanged (ref tof, value, nameof (TopOffset));
            }
        }

        private double lof;
        public double LeftOffset
        {
            get { return lof; }
            set
            {
                this.RaiseAndSetIfChanged (ref lof, value, nameof (LeftOffset));
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
