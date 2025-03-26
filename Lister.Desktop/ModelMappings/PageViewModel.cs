using Lister.Core.DocumentProcessor;
using view = Lister.Desktop.ModelMappings.BadgeVM;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Lister.Desktop.ModelMappings;

/// <summary>
/// Maps Page model into visible entity.
/// </summary>
public sealed class PageViewModel : ReactiveObject
{
    private double _scale;
    private view.BadgeLine _fillableLine;

    internal Page Model { get; private set; }

    private double _pageWidth;
    internal double Width
    {
        get { return _pageWidth; }
        set
        {
            this.RaiseAndSetIfChanged( ref _pageWidth, value, nameof( Width ) );
        }
    }

    private double _pageHeight;
    internal double Height
    {
        get { return _pageHeight; }
        set
        {
            this.RaiseAndSetIfChanged( ref _pageHeight, value, nameof( Height ) );
        }
    }

    private double _borderWidth;
    internal double BorderWidth
    {
        get { return _borderWidth; }
        set
        {
            this.RaiseAndSetIfChanged( ref _borderWidth, value, nameof( BorderWidth ) );
        }
    }

    private double _borderHeight;
    internal double BorderHeight
    {
        get { return _borderHeight; }
        set
        {
            this.RaiseAndSetIfChanged( ref _borderHeight, value, nameof( BorderHeight ) );
        }
    }

    private double _contentTopOffset;
    internal double ContentTopOffset
    {
        get { return _contentTopOffset; }
        set
        {
            this.RaiseAndSetIfChanged( ref _contentTopOffset, value, nameof( ContentTopOffset ) );
        }
    }

    private double _contentLeftOffset;
    internal double ContentLeftOffset
    {
        get { return _contentLeftOffset; }
        set
        {
            this.RaiseAndSetIfChanged( ref _contentLeftOffset, value, nameof( ContentLeftOffset ) );
        }
    }

    private ObservableCollection<BadgeVM.BadgeLine> _lines;
    internal ObservableCollection<BadgeVM.BadgeLine> Lines
    {
        get { return _lines; }
        set
        {
            this.RaiseAndSetIfChanged( ref _lines, value, nameof( Lines ) );
        }
    }


    public PageViewModel(Page source, double desiredScale)
    {
        Lines = new ObservableCollection<BadgeVM.BadgeLine>();
        Model = source;
        _scale = desiredScale;

        List<BadgeLine> sourceLines = source.Lines;

        foreach (BadgeLine line in sourceLines)
        {
            Lines.Add( new view.BadgeLine( line, _scale ) );
        }

        Width = source.Width;
        Height = source.Height;

        ContentTopOffset = source.ContentTopOffset;
        ContentLeftOffset = source.ContentLeftOffset;

        double usefullHeight = Height - 20;
        BorderHeight = Height + 2;
        BorderWidth = Width + 2;

        SetCorrectScale();
    }


    internal void ZoomOn(double scaleCoefficient)
    {
        _scale *= scaleCoefficient;
        Height *= scaleCoefficient;
        Width *= scaleCoefficient;
        ContentTopOffset *= scaleCoefficient;
        ContentLeftOffset *= scaleCoefficient;
        BorderHeight = Height + 2;
        BorderWidth = Width + 2;

        for (int index = 0; index < Lines.Count; index++)
        {
            Lines[index].ZoomOn( scaleCoefficient );
        }
    }


    internal void ZoomOut(double scaleCoefficient)
    {
        _scale /= scaleCoefficient;
        Height /= scaleCoefficient;
        Width /= scaleCoefficient;
        ContentTopOffset /= scaleCoefficient;
        ContentLeftOffset /= scaleCoefficient;
        BorderHeight = Height + 2;
        BorderWidth = Width + 2;

        for (int index = 0; index < Lines.Count; index++)
        {
            Lines[index].ZoomOut( scaleCoefficient );
        }
    }


    private void SetCorrectScale()
    {
        if (_scale != 1)
        {
            Height *= _scale;
            Width *= _scale;
            ContentTopOffset *= _scale;
            ContentLeftOffset *= _scale;
            BorderHeight = Height + 2;
            BorderWidth = Width + 2;
        }
    }


    internal List<view.BadgeViewModel> GetBadges()
    {
        List<view.BadgeViewModel> result = new();

        foreach (var line in Lines)
        {
            result.AddRange( line.Badges );
        }

        return result;
    }


    internal void Show()
    {
        for (int index = 0; index < Lines.Count; index++)
        {
            Lines[index].Show();
        }
    }


    internal void Hide()
    {
        for (int index = 0; index < Lines.Count; index++)
        {
            Lines[index].Hide();
        }
    }
}



