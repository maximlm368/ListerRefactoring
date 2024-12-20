using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ContentAssembler;
using Lister.Extentions;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    public class ImageViewModel : BoundToMember
    {
        public static Dictionary <string, Bitmap> NameToImage = new ();

        public string Path { get; private set; }

        private Bitmap _bitMap;
        internal Bitmap BitMap
        {
            get { return _bitMap; }
            private set
            {
                this.RaiseAndSetIfChanged (ref _bitMap, value, nameof (BitMap));
            }
        }


        public ImageViewModel ( InsideImage image )
        {
            Path = image.Path;

            if ( ! NameToImage.ContainsKey (image.Path)   ||   ( NameToImage [image.Path] == null ) )
            {
                Uri uri = new Uri (image.Path);
                string absolutePath = uri.AbsolutePath;
                NameToImage [image.Path] = ImageHelper.LoadFromResource (uri);
            }

            BitMap = NameToImage [image.Path];
            Binding = image.BindingName;

            SetYourself (image.Width, image.Height, image.TopOffset, image.LeftOffset);
        }


        internal void ZoomOn ( double coefficient )
        {
            base.ZoomOn (coefficient);
        }


        internal void ZoomOut ( double coefficient )
        {
            base.ZoomOut (coefficient);
        }

    }



    public abstract class BoundToMember : BadgeMember
    {
        private string _binding;
        public string Binding
        {
            get { return _binding; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref _binding, value, nameof (Binding));
            }
        }
    }
}
