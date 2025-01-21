using Avalonia;
using Avalonia.Media;
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
    public class ImageViewModel : BoundToTextLine
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


        public ImageViewModel ( int id, InsideImage image )
        {
            Path = image.Path;

            if ( ! NameToImage.ContainsKey (image.Path)   ||   ( NameToImage [image.Path] == null ) )
            {
                Uri uri = new Uri (image.Path);
                string absolutePath = uri.AbsolutePath;
                NameToImage [image.Path] = ImageHelper.LoadFromResource (uri);
            }

            Id = id;
            BitMap = NameToImage [image.Path];
            Binding = image.BindingName;
            IsAboveOfBinding = image.IsAboveOfBinding;

            Color color = new Color (255, image.OutlineRGB [0], image.OutlineRGB [1], image.OutlineRGB [2]);
            SolidColorBrush brush = new SolidColorBrush (color);

            SetYourself (image.Width, image.Height, image.TopOffset, image.LeftOffset, brush);
        }


        public ImageViewModel ( ImageViewModel prototype )
        {
            Path = prototype.Path;
            Id = prototype.Id;
            BitMap = NameToImage [Path];
            Binding = prototype.Binding;
            IsAboveOfBinding = prototype.IsAboveOfBinding;

            SetYourself (prototype.Width, prototype.Height, prototype.TopOffset, prototype.LeftOffset
                                                                               , prototype.outlineColorStorage);
        }


        internal void ZoomOn ( double coefficient )
        {
            base.ZoomOn (coefficient);
        }


        internal void ZoomOut ( double coefficient )
        {
            base.ZoomOut (coefficient);
        }


        internal ImageViewModel Clone ( )
        {
            return new ImageViewModel ( this );
        }
    }



    public abstract class BoundToTextLine : BadgeMember
    {
        public int Id { get; protected set; }

        private string ? _binding;
        public string ? Binding
        {
            get { return _binding; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref _binding, value, nameof (Binding));
            }
        }

        private bool _isAboveOfBinding;
        public bool IsAboveOfBinding
        {
            get { return _isAboveOfBinding; }
            protected set
            {
                this.RaiseAndSetIfChanged (ref _isAboveOfBinding, value, nameof (IsAboveOfBinding));
            }
        }
    }
}
