using Avalonia.Media.Imaging;
using Core.Models.Badge;
using View.Extentions;
using ReactiveUI;

namespace View.CoreModelReflection.Badge;

internal class ImageViewModel : BoundToTextLine
{
    private static Dictionary <string, Bitmap> NameToImage = new ();

    internal ComponentImage Model { get; private set; }
    internal string Path { get; private set; }

    private Bitmap _bitMap;
    internal Bitmap BitMap
    {
        get { return _bitMap; }
        private set
        {
            this.RaiseAndSetIfChanged (ref _bitMap, value, nameof (BitMap));
        }
    }


    internal ImageViewModel ( int id, ComponentImage model )
    {
        Path = model.Path;
        Model = model;

        if ( ! NameToImage.ContainsKey (model.Path)   ||   ( NameToImage [model.Path] == null ) )
        {
            NameToImage [model.Path] = ImageHelper.LoadFromResource (model.Path);
        }

        Id = id;
        BitMap = NameToImage [model.Path];
        Binding = model.Binding;
        IsAboveOfBinding = model.IsAboveOfBinding;

        SetUp (model.Width, model.Height, model.TopOffset, model.LeftOffset);

        Model.Changed += HandleModelChanged;
    }


    internal ImageViewModel ( ImageViewModel prototype )
    {
        Path = prototype.Path;
        Id = prototype.Id;
        BitMap = NameToImage [Path];
        Binding = prototype.Binding;
        IsAboveOfBinding = prototype.IsAboveOfBinding;

        SetUp (prototype.Width, prototype.Height, prototype.TopOffset, prototype.LeftOffset);

        Model.Changed += HandleModelChanged;
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



public abstract class BoundToTextLine : BadgeComponent
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


    protected void HandleModelChanged ( LayoutComponentBase model )
    {
        SetUp ( model.Width, model.Height, model.TopOffset, model.LeftOffset );
    }
}
