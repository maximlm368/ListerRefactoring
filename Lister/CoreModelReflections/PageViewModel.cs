using Avalonia;
using Core.DocumentProcessor;
using ReactiveUI;
using System.Collections.ObjectModel;
using view = View.CoreModelReflection.Badge;

namespace View.CoreModelReflection;

public class PageViewModel : ReactiveObject
{
    private static double _topOffsetOfContent;
    private static double _leftOffsetOfContent;
    internal static Size Size { get; private set; }

    private bool _isShown;
    private int _badgeCount;
    private double _scale;
    private Core.DocumentProcessor.BadgeLine _fillableLine;
    
    internal Page Model { get; private set; }

    private double _pageWidth;
    internal double Width
    {
        get { return _pageWidth; }
        set
        {
            this.RaiseAndSetIfChanged (ref _pageWidth, value, nameof (Width));
        }
    }

    private double _pageHeight;
    internal double Height
    {
        get { return _pageHeight; }
        set
        {
            this.RaiseAndSetIfChanged (ref _pageHeight, value, nameof (Height));
        }
    }

    private double _borderWidth;
    internal double BorderWidth
    {
        get { return _borderWidth; }
        set
        {
            this.RaiseAndSetIfChanged (ref _borderWidth, value, nameof (BorderWidth));
        }
    }

    private double _borderHeight;
    internal double BorderHeight
    {
        get { return _borderHeight; }
        set
        {
            this.RaiseAndSetIfChanged (ref _borderHeight, value, nameof (BorderHeight));
        }
    }

    private double _contentTopOffset;
    internal double ContentTopOffset
    {
        get { return _contentTopOffset; }
        set
        {
            this.RaiseAndSetIfChanged (ref _contentTopOffset, value, nameof (ContentTopOffset));
        }
    }

    private double _contentLeftOffset;
    internal double ContentLeftOffset
    {
        get { return _contentLeftOffset; }
        set
        {
            this.RaiseAndSetIfChanged (ref _contentLeftOffset, value, nameof (ContentLeftOffset));
        }
    }

    private ObservableCollection <view.BadgeLine> _lines;
    internal ObservableCollection <view.BadgeLine> Lines
    {
        get { return _lines; }
        set
        {
            this.RaiseAndSetIfChanged (ref _lines, value, nameof (Lines));
        }
    }


    static PageViewModel () 
    {
        Size = new Size (794, 1123);
        _topOffsetOfContent = 20;
        _leftOffsetOfContent = 45;
    }


    public PageViewModel ( double desiredScale )
    {
        Lines = new ObservableCollection<view.BadgeLine> ();
        _scale = desiredScale;
        _badgeCount = 0;

        Width = Size.Width;
        Height = Size.Height;
        ContentTopOffset = _topOffsetOfContent;
        ContentLeftOffset = _leftOffsetOfContent;
        double usefullHeight = Height - 20;
        BorderHeight = Height + 2;
        BorderWidth = Width + 2;

        SetCorrectScale ();
    }


    public PageViewModel ( Page source, double desiredScale )
    {
        Lines = new ObservableCollection<view.BadgeLine> ();
        Model = source;
        _scale = desiredScale;
        _badgeCount = 0;

        List<BadgeLine> sourceLines = source.Lines;

        foreach ( BadgeLine line   in   sourceLines ) 
        {
            Lines.Add ( new view.BadgeLine ( line, _scale ) );
        }

        Width = source. Width;
        Height = source. Height;
        ContentTopOffset = _topOffsetOfContent;
        ContentLeftOffset = _leftOffsetOfContent;
        double usefullHeight = Height - 20;
        BorderHeight = Height + 2;
        BorderWidth = Width + 2;

        SetCorrectScale ();
    }


    internal void ZoomOn ( double scaleCoefficient )
    {
        this._scale *= scaleCoefficient;
        Height *= scaleCoefficient;
        Width *= scaleCoefficient;
        ContentTopOffset *= scaleCoefficient;
        ContentLeftOffset *= scaleCoefficient;
        BorderHeight = Height + 2;
        BorderWidth = Width + 2;

        for ( int index = 0;   index < Lines. Count;   index++ )
        {
            Lines [index].ZoomOn (scaleCoefficient);
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

        for ( int index = 0;   index < Lines. Count;   index++ )
        {
            Lines [index].ZoomOut (scaleCoefficient);
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


    internal List<view.BadgeViewModel> GetBadges ()
    {
        List<view.BadgeViewModel> result = new ();

        foreach ( var line in Lines )
        {
            result.AddRange ( line.Badges );
        }

        return result;
    }


    internal void Show ()
    {
        for ( int index = 0;   index < Lines. Count;   index++ )
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



//public class PageException : Exception {}


//private double lCLS;
//internal double LinesLeftShift
//{
//    get { return lCLS; }
//    set
//    {
//        this.RaiseAndSetIfChanged (ref lCLS, value, nameof (LinesLeftShift));
//    }
//}

//private double lCTS;
//internal double LinesContainerTopShift
//{
//    get { return lCTS; }
//    set
//    {
//        this.RaiseAndSetIfChanged (ref lCTS, value, nameof (LinesContainerTopShift));
//    }
//}



//private void VerifyBadgeSizeAccordence ( VMBadge badge )
//{
//    Size verifiebleSize = badge.BadgeModel.badgeDescription.badgeDimensions.outlineSize;
//    int verifiebleWidth = ( int ) ( _scale * verifiebleSize.Width );
//    int verifiebleHeight = ( int ) ( _scale * verifiebleSize.Height );

//    bool isNotAccordent = ( verifiebleWidth != ( int ) BadgeExample.BadgeWidth )
//                          ||
//                          ( verifiebleHeight != ( int ) BadgeExample.BadgeHeight );

//    if ( isNotAccordent )
//    {
//        throw new Exception ("Size of passed on badge is not according set for this page");
//    }
//}


//bool timeToStartNewPage = IsTimeForNewPage ();

//if ( timeToStartNewPage )
//{
//    beingProcessedPage = new PageViewModel ( _scale);
//}