using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Document;
using System.Collections.ObjectModel;
using View = Lister.Desktop.ModelMappings.BadgeVM;

namespace Lister.Desktop.ModelMappings;

/// <summary>
/// Maps Page model into visible entity.
/// </summary>
public sealed partial class PageViewModel : ObservableObject
{
    private double _scale;

    [ObservableProperty]
    private double _width;

    [ObservableProperty]
    private double _height;

    [ObservableProperty]
    private double _borderWidth;

    [ObservableProperty]
    private double _borderHeight;

    [ObservableProperty]
    private double _contentTopOffset;

    [ObservableProperty]
    private double _contentLeftOffset;

    [ObservableProperty]
    private ObservableCollection<BadgeLine> _lines = [];

    internal Page Model { get; private set; }

    public PageViewModel ( Page source, double desiredScale )
    {
        Model = source;
        _scale = desiredScale;

        List<Core.Document.BadgeLine> sourceLines = source.Lines;

        foreach ( Core.Document.BadgeLine line in sourceLines )
        {
            Lines.Add ( new BadgeLine ( line, _scale ) );
        }

        Width = source.Width;
        Height = source.Height;

        ContentTopOffset = source.ContentTopOffset;
        ContentLeftOffset = source.ContentLeftOffset;

        BorderHeight = Height + 2;
        BorderWidth = Width + 2;

        SetCorrectScale ();
    }

    internal void ZoomOn ( double scaleCoefficient )
    {
        _scale *= scaleCoefficient;
        Height *= scaleCoefficient;
        Width *= scaleCoefficient;
        ContentTopOffset *= scaleCoefficient;
        ContentLeftOffset *= scaleCoefficient;
        BorderHeight = Height + 2;
        BorderWidth = Width + 2;

        for ( int index = 0; index < Lines.Count; index++ )
        {
            Lines [index].ZoomOn ( scaleCoefficient );
        }
    }

    internal void ZoomOut ( double scaleCoefficient )
    {
        _scale /= scaleCoefficient;
        Height /= scaleCoefficient;
        Width /= scaleCoefficient;
        ContentTopOffset /= scaleCoefficient;
        ContentLeftOffset /= scaleCoefficient;
        BorderHeight = Height + 2;
        BorderWidth = Width + 2;

        for ( int index = 0; index < Lines.Count; index++ )
        {
            Lines [index].ZoomOut ( scaleCoefficient );
        }
    }

    private void SetCorrectScale ()
    {
        if ( _scale != 1 )
        {
            Height *= _scale;
            Width *= _scale;
            ContentTopOffset *= _scale;
            ContentLeftOffset *= _scale;
            BorderHeight = Height + 2;
            BorderWidth = Width + 2;
        }
    }

    internal List<View.BadgeViewModel> GetBadges ()
    {
        List<View.BadgeViewModel> result = [];

        foreach ( var line in Lines )
        {
            result.AddRange ( line.Badges );
        }

        return result;
    }

    internal void Show ()
    {
        for ( int index = 0; index < Lines.Count; index++ )
        {
            Lines [index].Show ();
        }
    }

    internal void Hide ()
    {
        for ( int index = 0; index < Lines.Count; index++ )
        {
            Lines [index].Hide ();
        }
    }
}
