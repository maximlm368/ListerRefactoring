using Avalonia;
using Core.DocumentBuilder;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Lister.ViewModels;

public class PageViewModel : ReactiveObject
{
    private static double _topOffsetOfContent;
    private static double _leftOffsetOfContent;
    internal static Size Size { get; private set; }

    private bool _isShown;
    private int _badgeCount;
    private double _scale;
    private BadgeLine _fillableLine;

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

    private ObservableCollection <BadgeLine> _lines;
    internal ObservableCollection <BadgeLine> Lines
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
        Lines = new ObservableCollection <BadgeLine> ();
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

        //_fillableLine = new BadgeLine ( Width , _scale , usefullHeight, true );
        //Lines.Add ( _fillableLine );
    }


    public PageViewModel ( Page source, double desiredScale )
    {
        Lines = new ObservableCollection <BadgeLine> ();
        _scale = desiredScale;
        _badgeCount = 0;

        List<Core.DocumentBuilder.BadgeLine> sourceLines = source.Lines;

        foreach ( Core.DocumentBuilder.BadgeLine line   in   sourceLines ) 
        {
            Lines.Add ( new BadgeLine ( line, _scale ) );
        }

        Width = source. Width;
        Height = source. Height;
        ContentTopOffset = _topOffsetOfContent;
        ContentLeftOffset = _leftOffsetOfContent;
        double usefullHeight = Height - 20;
        BorderHeight = Height + 2;
        BorderWidth = Width + 2;

        SetCorrectScale ();

        //_fillableLine = new BadgeLine ( Width , _scale , usefullHeight, true );
        //Lines.Add ( _fillableLine );
    }


    //private PageViewModel ( PageViewModel page )
    //{
    //    Lines = new ObservableCollection <BadgeLine> ();
    //    _scale = 1;
    //    _badgeCount = page._badgeCount;
    //    Width = Size. Width;
    //    Height = Size. Height;
    //    ContentTopOffset = _topOffsetOfContent;
    //    ContentLeftOffset = _leftOffsetOfContent;
    //    BorderHeight = Height + 2;
    //    BorderWidth = Width + 2;

    //    foreach ( BadgeLine line   in   page.Lines ) 
    //    {
    //        BadgeLine originalLine = line.GetDimensionalOriginal ();
    //        Lines.Add (originalLine);
    //    }
    //}


    //internal PageViewModel GetDimendionalOriginalClone () 
    //{
    //    return new PageViewModel ( this );
    //}


    //internal static List <PageViewModel> ? PlaceIntoPages ( List <BadgeViewModel> placebleBadges,
    //                                                        double desiredScale, PageViewModel ? scratchPage )
    //{
    //    bool areArgumentsInvalid = AreArgumentsInvalid (placebleBadges, desiredScale);

    //    if ( areArgumentsInvalid )
    //    {
    //        return null;
    //    }
        
    //    //PageViewModel fillablePage = scratchPage ?? new PageViewModel (pageSize, badgeExample, desiredScale);

    //    PageViewModel fillablePage = DefineFillablePage(scratchPage, desiredScale);
    //    List <PageViewModel> pages = Place (placebleBadges, desiredScale, fillablePage);

    //    return pages;
    //}


    //internal PageViewModel AddBadge ( BadgeViewModel badge )
    //{
    //    PageViewModel beingProcessedPage = this;
    //    bool shouldScaleBadge = (badge.Scale != this._scale);
    //    ActionSuccess additionSuccess = _fillableLine.AddBadge (badge, shouldScaleBadge);

    //    if ( additionSuccess == ActionSuccess.FailureByWidth ) 
    //    {
    //        double restHeight = GetRestHeight ();
    //        bool isFirstLine = (Lines. Count == 0);
    //        BadgeLine newLine = new BadgeLine (Width, _scale, restHeight, isFirstLine);
    //        additionSuccess = newLine.AddBadge (badge, false);

    //        if ( additionSuccess == ActionSuccess.FailureByWidth )
    //        {
    //            throw new PageException ();
    //        }
    //        else if ( additionSuccess == ActionSuccess.FailureByHeight ) 
    //        {
    //            beingProcessedPage = new PageViewModel (_scale);
    //            beingProcessedPage.AddBadge (badge);
    //        }

    //        beingProcessedPage.Lines.Add (newLine);
    //        beingProcessedPage._fillableLine = newLine;
    //    }

    //    if ( additionSuccess == ActionSuccess.FailureByHeight )
    //    {
    //        bool isPageBlank = (beingProcessedPage.Lines. Count < 2);

    //        if ( isPageBlank ) 
    //        {
    //            throw new PageException ();
    //        }

    //        beingProcessedPage = new PageViewModel (_scale);
    //        beingProcessedPage.AddBadge (badge);
    //    }

    //    beingProcessedPage._badgeCount++;
    //    return beingProcessedPage;
    //}


    //private bool IsTimeForNewPage () 
    //{
    //    double restHeight = GetRestHeight ();
    //    bool itIsTime = restHeight > _fillableLine.Height;
    //    return itIsTime;
    //}


    //private double GetRestHeight ( ) 
    //{
    //    double summaryHeight = 0;

    //    foreach ( BadgeLine line   in   Lines )
    //    {
    //        summaryHeight += line.Height;
    //    }

    //    double restHeight = Height - summaryHeight;
    //    return restHeight;
    //}


    //internal void Clear ()
    //{
    //    _badgeCount = 0;

    //    foreach( BadgeLine line   in   Lines ) 
    //    {
    //        line.Clear ();
    //    }
    //}


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


    internal List<BadgeViewModel> GetBadges ()
    {
        List<BadgeViewModel> result = new ();

        foreach ( var line in Lines )
        {
            result.AddRange ( line.Badges );
        }

        return result;
    }


    //internal void Clone ()
    //{
    //    for ( int index = 0;   index < Lines.Count;   index++ )
    //    {
    //        Lines [index].Show ();
    //    }
    //}


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