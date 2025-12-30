using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Models.Badge;

namespace Lister.Desktop.ModelMappings.BadgeVM;

/// <summary>
/// Maps ComponentShape model into visible entity.
/// </summary>
public sealed partial class ShapeViewModel : BoundToTextLineBase
{
    internal ShapeType Type { get; private set; }
    internal ComponentShape Model { get; private set; }
    internal string FillColorHexStr { get; private set; }

    [ObservableProperty]
    private SolidColorBrush? _fillColor;

    internal ShapeViewModel ( int id, ComponentShape model )
    {
        Id = id;
        Model = model;
        Type = model.Type;

        string hexStr = model.FillHexStr;

        if ( !Color.TryParse ( model.FillHexStr, out Color color ) )
        {
            color = new Color ( 255, 0, 0, 0 );
            hexStr = "#000000";
        }

        FillColorHexStr = hexStr;
        FillColor = new SolidColorBrush ( color );
        Binding = model.Binding;
        IsAboveOfBinding = model.IsAboveOfBinding;

        SetUp ( model.Width, model.Height, model.TopOffset, model.LeftOffset );

        Model.Changed += HandleModelChanged;
    }
}