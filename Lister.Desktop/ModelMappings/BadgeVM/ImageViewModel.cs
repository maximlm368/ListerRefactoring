using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Models.Badge;
using Lister.Desktop.Extentions;

namespace Lister.Desktop.ModelMappings.BadgeVM;

/// <summary>
/// Maps ComponentImage model into visible entity.
/// </summary>
public sealed partial class ImageViewModel : BoundToTextLineBase
{
    private static readonly Dictionary<string, Bitmap?> _nameToImage = [];

    internal ComponentImage Model { get; private set; }
    internal string Path { get; private set; }

    [ObservableProperty]
    private Bitmap? _bitMap;

    internal ImageViewModel ( int id, ComponentImage model )
    {
        Path = model.Path;
        Model = model;

        if ( !_nameToImage.TryGetValue ( model.Path, out Bitmap? value ) || value == null )
        {
            value =  ImageHelper.LoadFromResource ( model.Path );
            _nameToImage [model.Path] = value;
        }

        Id = id;
        BitMap = value;
        Binding = model.Binding;
        IsAboveOfBinding = model.IsAboveOfBinding;

        SetUp ( model.Width, model.Height, model.TopOffset, model.LeftOffset );

        Model.Changed += HandleModelChanged;
    }
}
