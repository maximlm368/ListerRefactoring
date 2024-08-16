using Avalonia;
using ContentAssembler;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    public class ImageViewModel : BadgeMember
    {
        private string pt;
        internal string Path
        {
            get { return pt; }
            private set
            {
                this.RaiseAndSetIfChanged (ref pt, value, nameof (Path));
            }
        }

        private double wd;
        internal double Width
        {
            get { return wd; }
            private set
            {
                this.RaiseAndSetIfChanged (ref wd, value, nameof (Width));
            }
        }

        private double hg;
        internal double Height
        {
            get { return hg; }
            private set
            {
                this.RaiseAndSetIfChanged (ref hg, value, nameof (Height));
            }
        }

        private string cl;
        internal string Color
        {
            get { return cl; }
            private set
            {
                this.RaiseAndSetIfChanged (ref cl, value, nameof (Color));
            }
        }

        private string gen;
        internal string GeometryElementName
        {
            get { return gen; }
            private set
            {
                this.RaiseAndSetIfChanged (ref gen, value, nameof (GeometryElementName));
            }
        }

        private ImageType it;
        internal ImageType ImageKind
        {
            get { return it; }
            private set
            {
                this.RaiseAndSetIfChanged (ref it, value, nameof (ImageKind));
            }
        }


        public ImageViewModel ( InsideImage image ) 
        {
            double width = image.OutlineWidth;
            double height = image.OutlineHeight;

            bool isImageWorthVisibility = image.ImageKind != ImageType.nothing;

            if ( isImageWorthVisibility ) 
            {
                width = image.OutlineWidth;
                height = image.OutlineHeight;
            }

            Path = image.Path;
            Color = image.Color;
            GeometryElementName = image.GeometryElementName;
            ImageKind = image.ImageKind;

            SetYourself (width, height, image.TopOffset, image.LeftOffset);
        }


        internal void ZoomOn ( double coefficient )
        {
            Width *= coefficient;
            Height *= coefficient;
            base.ZoomOn (coefficient);
        }


        internal void ZoomOut ( double coefficient )
        {
            Width /= coefficient;
            Height /= coefficient;
            base.ZoomOut (coefficient);
        }

    }
}
