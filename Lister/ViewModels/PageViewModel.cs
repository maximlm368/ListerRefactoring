using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Core.BadgesProvider;
using Avalonia;
using ReactiveUI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static QuestPDF.Helpers.Colors;
using Lister.Extentions;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Layout;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SkiaSharp;
using QuestPDF.Helpers;
using DynamicData;
using System.Collections.Generic;

namespace Lister.ViewModels;

public class PageViewModel : ReactiveObject
{
    private static double _topOffsetOfContent;
    private static double _leftOffsetOfContent;
    internal static Size PageSize { get; private set; }

    private bool _isShown;
    private int _badgeCount;
    private double _scale;
    private BadgeLine _fillableLine;

    private double _pageWidth;
    internal double PageWidth
    {
        get { return _pageWidth; }
        set
        {
            this.RaiseAndSetIfChanged (ref _pageWidth, value, nameof (PageWidth));
        }
    }

    private double _pageHeight;
    internal double PageHeight
    {
        get { return _pageHeight; }
        set
        {
            this.RaiseAndSetIfChanged (ref _pageHeight, value, nameof (PageHeight));
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
        PageSize = new Size (794, 1123);
        _topOffsetOfContent = 20;
        _leftOffsetOfContent = 45;
    }


    public PageViewModel ( double desiredScale )
    {
        Lines = new ObservableCollection<BadgeLine> ();
        _scale = desiredScale;
        _badgeCount = 0;
        PageWidth = PageSize. Width;
        PageHeight = PageSize. Height;
        ContentTopOffset = _topOffsetOfContent;
        ContentLeftOffset = _leftOffsetOfContent;
        double usefullHeight = PageHeight - 20;
        BorderHeight = PageHeight + 2;
        BorderWidth = PageWidth + 2;

        SetCorrectScale ();

        _fillableLine = new BadgeLine ( PageWidth , _scale , usefullHeight, true );
        Lines.Add ( _fillableLine );
    }


    private PageViewModel ( PageViewModel page )
    {
        Lines = new ObservableCollection <BadgeLine> ();
        _scale = 1;
        _badgeCount = page._badgeCount;
        PageWidth = PageSize. Width;
        PageHeight = PageSize. Height;
        ContentTopOffset = _topOffsetOfContent;
        ContentLeftOffset = _leftOffsetOfContent;
        BorderHeight = PageHeight + 2;
        BorderWidth = PageWidth + 2;

        foreach ( BadgeLine line   in   page.Lines ) 
        {
            BadgeLine originalLine = line.GetDimensionalOriginal ();
            Lines.Add (originalLine);
        }
    }


    internal PageViewModel GetDimendionalOriginalClone () 
    {
        return new PageViewModel ( this );
    }


    internal static List <PageViewModel> ? PlaceIntoPages ( List <BadgeViewModel> placebleBadges,
                                                            double desiredScale, PageViewModel ? scratchPage )
    {
        bool areArgumentsInvalid = AreArgumentsInvalid (placebleBadges, desiredScale);

        if ( areArgumentsInvalid )
        {
            return null;
        }
        
        //PageViewModel fillablePage = scratchPage ?? new PageViewModel (pageSize, badgeExample, desiredScale);

        PageViewModel fillablePage = DefineFillablePage(scratchPage, desiredScale);
        List <PageViewModel> pages = Place (placebleBadges, desiredScale, fillablePage);

        return pages;
    }


    internal PageViewModel AddBadge ( BadgeViewModel badge )
    {
        PageViewModel beingProcessedPage = this;
        bool shouldScaleBadge = (badge.Scale != this._scale);
        ActionSuccess additionSuccess = _fillableLine.AddBadge (badge, shouldScaleBadge);

        if ( additionSuccess == ActionSuccess.FailureByWidth ) 
        {
            double restHeight = GetRestHeight ();
            bool isFirstLine = (Lines. Count == 0);
            BadgeLine newLine = new BadgeLine (PageWidth, _scale, restHeight, isFirstLine);
            additionSuccess = newLine.AddBadge (badge, false);

            if ( additionSuccess == ActionSuccess.FailureByWidth )
            {
                throw new PageException ();
            }
            else if ( additionSuccess == ActionSuccess.FailureByHeight ) 
            {
                beingProcessedPage = new PageViewModel (_scale);
                beingProcessedPage.AddBadge (badge);
            }

            beingProcessedPage.Lines.Add (newLine);
            beingProcessedPage._fillableLine = newLine;
        }

        if ( additionSuccess == ActionSuccess.FailureByHeight )
        {
            bool isPageBlank = (beingProcessedPage.Lines. Count < 2);

            if ( isPageBlank ) 
            {
                throw new PageException ();
            }

            beingProcessedPage = new PageViewModel (_scale);
            beingProcessedPage.AddBadge (badge);
        }

        beingProcessedPage._badgeCount++;
        return beingProcessedPage;
    }


    private bool IsTimeForNewPage () 
    {
        double restHeight = GetRestHeight ();
        bool itIsTime = restHeight > _fillableLine.Height;
        return itIsTime;
    }


    private double GetRestHeight ( ) 
    {
        double summaryHeight = 0;

        foreach ( BadgeLine line   in   Lines )
        {
            summaryHeight += line.Height;
        }

        double restHeight = PageHeight - summaryHeight;
        return restHeight;
    }


    internal void Clear ()
    {
        _badgeCount = 0;

        foreach( BadgeLine line   in   Lines ) 
        {
            line.Clear ();
        }
    }


    internal void ZoomOn ( double scaleCoefficient )
    {
        this._scale *= scaleCoefficient;
        PageHeight *= scaleCoefficient;
        PageWidth *= scaleCoefficient;
        ContentTopOffset *= scaleCoefficient;
        ContentLeftOffset *= scaleCoefficient;
        BorderHeight = PageHeight + 2;
        BorderWidth = PageWidth + 2;

        for ( int index = 0;   index < Lines. Count;   index++ )
        {
            Lines [index].ZoomOn (scaleCoefficient);
        }
    }


    internal void ZoomOut ( double scaleCoefficient )
    {
        _scale /= scaleCoefficient;
        PageHeight /= scaleCoefficient;
        PageWidth /= scaleCoefficient;
        ContentTopOffset /= scaleCoefficient;
        ContentLeftOffset /= scaleCoefficient;
        BorderHeight = PageHeight + 2;
        BorderWidth = PageWidth + 2;

        for ( int index = 0;   index < Lines. Count;   index++ )
        {
            Lines [index].ZoomOut (scaleCoefficient);
        }
    }


    private void SetCorrectScale ()
    {
        if ( _scale != 1 )
        {
            PageHeight *= _scale;
            PageWidth *= _scale;
            ContentTopOffset *= _scale;
            ContentLeftOffset *= _scale;
            BorderHeight = PageHeight + 2;
            BorderWidth = PageWidth + 2;
        }
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


    private static bool AreArgumentsInvalid ( List<BadgeViewModel> placebleBadges, double desiredScale )
    {
        bool areArgumentsInvalid = ( placebleBadges == null );
        areArgumentsInvalid = areArgumentsInvalid   ||   ( placebleBadges.Count < 1 );
        areArgumentsInvalid = areArgumentsInvalid   ||   ( desiredScale == 0 );

        return areArgumentsInvalid;
    }


    private static PageViewModel DefineFillablePage ( PageViewModel ? scratchPage, double desiredScale ) 
    {
        bool isBadgeInsertionFirstTime = (scratchPage == null);

        if ( isBadgeInsertionFirstTime ) 
        {
            return new PageViewModel ( desiredScale );
        }
        else 
        {
            return scratchPage;
        }
    }


    private static List <PageViewModel> Place ( List <BadgeViewModel> placebleBadges, double desiredScale
                                                                            , PageViewModel fillablePage ) 
    {
        List <PageViewModel> result = new ();

        for ( int index = 0;   index < placebleBadges.Count;   index++ )
        {
            BadgeViewModel beingProcessedBadge = placebleBadges [index];

            PageViewModel posibleNewPadge = fillablePage.AddBadge (beingProcessedBadge);
            bool timeToAddNewPage = ! posibleNewPadge.Equals (fillablePage);

            if ( timeToAddNewPage )
            {
                result.Add (fillablePage);
                fillablePage = posibleNewPadge;
            }

            if ( index == placebleBadges.Count - 1 )
            {
                result.Add (fillablePage);
            }
        }

        return result;
    }
}



public class PageException : Exception {}


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