using Avalonia.Media.Imaging;
using Core.Models.Badge;
using Lister.Extentions;
using ReactiveUI;

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
                NameToImage [image.Path] = ImageHelper.LoadFromResource (image.Path);
            }

            Id = id;
            BitMap = NameToImage [image.Path];
            Binding = image.BindingName;
            IsAboveOfBinding = image.IsAboveOfBinding;

            SetYourself (image.Width, image.Height, image.TopOffset, image.LeftOffset);
        }


        public ImageViewModel ( ImageViewModel prototype )
        {
            Path = prototype.Path;
            Id = prototype.Id;
            BitMap = NameToImage [Path];
            Binding = prototype.Binding;
            IsAboveOfBinding = prototype.IsAboveOfBinding;

            SetYourself (prototype.Width, prototype.Height, prototype.TopOffset, prototype.LeftOffset);
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
