using Avalonia.Media;
using Lister.Core.Models.Badge;
using ReactiveUI;

namespace Lister.Desktop.CoreModelReflections.BadgeVM;

internal class ShapeViewModel : BoundToTextLineBase
{
    internal ShapeType Type { get; private set; }
    internal ComponentShape Model { get; private set; }
    private SolidColorBrush _fillColor;
    internal SolidColorBrush FillColor
    {
        get { return _fillColor; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _fillColor, value, nameof( FillColor ) );
        }
    }

    internal string FillColorHexStr { get; private set; }


    internal ShapeViewModel(int id, ComponentShape model)
    {
        Id = id;
        Model = model;
        Type = model.Type;

        Color color;
        string hexStr = model.FillHexStr;

        if (!Color.TryParse( model.FillHexStr, out color ))
        {
            color = new Color( 255, 0, 0, 0 );
            hexStr = "#000000";
        }

        FillColorHexStr = hexStr;
        FillColor = new SolidColorBrush( color );
        Binding = model.Binding;
        IsAboveOfBinding = model.IsAboveOfBinding;

        SetUp( model.Width, model.Height, model.TopOffset, model.LeftOffset );

        Model.Changed += HandleModelChanged;
    }


    internal ShapeViewModel(ShapeViewModel prototype)
    {
        Id = prototype.Id;
        Model = prototype.Model;
        Type = prototype.Type;
        FillColor = prototype.FillColor;
        Binding = prototype.Binding;
        IsAboveOfBinding = prototype.IsAboveOfBinding;

        SetUp( prototype.Width, prototype.Height, prototype.TopOffset, prototype.LeftOffset );

        Model.Changed += HandleModelChanged;
    }


    internal ShapeViewModel Clone()
    {
        return new ShapeViewModel( this );
    }
}