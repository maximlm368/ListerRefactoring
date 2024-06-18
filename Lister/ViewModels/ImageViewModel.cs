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

        private Size sz;
        internal Size Size
        {
            get { return sz; }
            private set
            {
                this.RaiseAndSetIfChanged (ref sz, value, nameof (Size));
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
            double width = image.Size. Width;
            double height = image.Size .Height;

            bool isImageWorthVisibility = image.ImageKind != ImageType.nothing;

            if ( isImageWorthVisibility ) 
            {
                width = image.Size.Width;
                height = image.Size.Height;
            }

            Path = image.Path;
            Color = image.Color;
            GeometryElementName = image.GeometryElementName;
            ImageKind = image.ImageKind;

            SetYourself (width, height, image.TopShiftOnBackground, image.LeftShiftOnBackground);
        }


        internal void ZoomOn ( double coefficient )
        {
            Size.ZoomOn (coefficient);
            base.ZoomOn (coefficient);
        }


        internal void ZoomOut ( double coefficient )
        {
            Size.ZoomOut (coefficient);
            base.ZoomOut (coefficient);
        }

    }
}
