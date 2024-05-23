using Avalonia.Media;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    abstract internal class VisibleMember : ViewModelBase
    {
        private double wd;
        internal double Width
        {
            get { return wd; }
            private set
            {
                this.RaiseAndSetIfChanged (ref wd, value, nameof (Width));
            }
        }

        private double ht;
        internal double Height
        {
            get { return ht; }
            private set
            {
                this.RaiseAndSetIfChanged (ref ht, value, nameof (Height));
            }
        }

        private double tof;
        internal double TopOffset
        {
            get { return tof; }
            private set
            {
                this.RaiseAndSetIfChanged (ref tof, value, nameof (TopOffset));
            }
        }

        private double lof;
        internal double LeftOffset
        {
            get { return lof; }
            private set
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
            Width *= coefficient;
            Height *= coefficient;
            TopOffset *= coefficient;
            LeftOffset *= coefficient;
        }


        protected void ZoomOut ( double coefficient )
        {
            Width /= coefficient;
            Height /= coefficient;
            TopOffset /= coefficient;
            LeftOffset /= coefficient;
        }
    }
}
