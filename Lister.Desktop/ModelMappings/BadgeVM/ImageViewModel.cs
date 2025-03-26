using Avalonia.Media.Imaging;
using Lister.Core.Models.Badge;
using Lister.Desktop.Extentions;
using ReactiveUI;

namespace Lister.Desktop.ModelMappings.BadgeVM;

/// <summary>
/// Maps ComponentImage model into visible entity.
/// </summary>
internal sealed class ImageViewModel : BoundToTextLineBase
{
    private static Dictionary<string, Bitmap> _nameToImage = new();

    internal ComponentImage Model { get; private set; }
    internal string Path { get; private set; }

    private Bitmap _bitMap;
    internal Bitmap BitMap
    {
        get { return _bitMap; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _bitMap, value, nameof( BitMap ) );
        }
    }


    internal ImageViewModel(int id, ComponentImage model)
    {
        Path = model.Path;
        Model = model;

        if (!_nameToImage.ContainsKey( model.Path ) || _nameToImage[model.Path] == null)
        {
            _nameToImage[model.Path] = ImageHelper.LoadFromResource( model.Path );
        }

        Id = id;
        BitMap = _nameToImage[model.Path];
        Binding = model.Binding;
        IsAboveOfBinding = model.IsAboveOfBinding;

        SetUp( model.Width, model.Height, model.TopOffset, model.LeftOffset );

        Model.Changed += HandleModelChanged;
    }


    internal ImageViewModel(ImageViewModel prototype)
    {
        Path = prototype.Path;
        Id = prototype.Id;
        BitMap = _nameToImage[Path];
        Binding = prototype.Binding;
        IsAboveOfBinding = prototype.IsAboveOfBinding;

        SetUp( prototype.Width, prototype.Height, prototype.TopOffset, prototype.LeftOffset );

        Model.Changed += HandleModelChanged;
    }


    internal ImageViewModel Clone()
    {
        return new ImageViewModel( this );
    }
}

