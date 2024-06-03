using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ContentAssembler;
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

public class PageViewModel : ViewModelBase
{
    private readonly int _lineLimit;
    private int _badgeCount;
    private double _scale;
    private BadgeLine _currentLine;
    internal static Size pageSize;

    private double pW;
    internal double PageWidth
    {
        get { return pW; }
        set
        {
            this.RaiseAndSetIfChanged (ref pW, value, nameof (PageWidth));
        }
    }

    private double pH;
    internal double PageHeight
    {
        get { return pH; }
        set
        {
            this.RaiseAndSetIfChanged (ref pH, value, nameof (PageHeight));
        }
    }

    private ObservableCollection <BadgeLine> llines;
    internal ObservableCollection <BadgeLine> Lines
    {
        get { return llines; }
        set
        {
            this.RaiseAndSetIfChanged (ref llines, value, nameof (Lines));
        }
    }


    static PageViewModel () 
    {
        pageSize = new Size (794, 1123);
    }


    public PageViewModel ( double desiredScale )
    {
        Lines = new ObservableCollection<BadgeLine> ();
        _scale = desiredScale;
        _badgeCount = 0;
        PageWidth = pageSize.Width;
        PageHeight = pageSize.Height;
        //_lineLimit = GetMaxLineAmount ();////////////////////////////////////////////////////////////////////////////////
        _currentLine = new BadgeLine (PageWidth, _scale);
        Lines.Add (_currentLine);
        SetCorrectScale ();
    }


    internal static List <PageViewModel> ? PlaceIntoPages ( List<BadgeViewModel> placebleBadges,
                                                            double desiredScale, PageViewModel ? scratchPage )
    {
        bool areArgumentsInvalid = AreArgumentsInvalid (placebleBadges, desiredScale);

        if ( areArgumentsInvalid )
        {
            return null;
        }
        
        //PageViewModel fillablePage = scratchPage ?? new PageViewModel (pageSize, badgeExample, desiredScale);

        PageViewModel fillablePage = DefineFillablePage(scratchPage, desiredScale);

        return Place (placebleBadges, desiredScale, fillablePage);
    }


    internal PageViewModel AddBadge ( BadgeViewModel badge, bool mustBeZoomed )
    {
        PageViewModel beingProcessedPage = this;
        ActionSuccess additionSuccess = _currentLine.AddBadge (badge);

        if ( additionSuccess == ActionSuccess.Failure ) 
        {
            _currentLine = new BadgeLine (PageWidth, _scale);
            additionSuccess = _currentLine.AddBadge (badge);

            if ( additionSuccess == ActionSuccess.Failure )
            {
                throw new PageException ( );
            }

            bool timeToStartNewPage = ( Lines. Count == _lineLimit );

            if ( timeToStartNewPage )
            {
                beingProcessedPage = new PageViewModel ( _scale);
            }

            beingProcessedPage.Lines.Add (_currentLine);
        }

        beingProcessedPage._badgeCount++;
        return beingProcessedPage;
    }


    internal void Clear ()
    {
        _badgeCount = 0;
    }


    //private int GetMaxLineAmount ()
    //{
    //    double badgeHeight = BadgeExample. BadgeHeight;
    //    return ( int ) ( PageHeight / badgeHeight );
    //}


    internal void ZoomOn ( double scaleCoefficient )
    {
        this._scale *= scaleCoefficient;
        PageHeight *= scaleCoefficient;
        PageWidth *= scaleCoefficient;

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
        }
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
        for ( int index = 0;   index < Lines. Count;   index++ )
        {
            Lines [index].Hide ();
        }
    }


    private static bool AreArgumentsInvalid ( List<BadgeViewModel> placebleBadges, double desiredScale )
    {
        bool areArgumentsInvalid = ( placebleBadges == null );
        areArgumentsInvalid = areArgumentsInvalid || ( placebleBadges.Count < 1 );
        areArgumentsInvalid = areArgumentsInvalid || ( desiredScale == 0 );

        return areArgumentsInvalid;
    }


    private static PageViewModel DefineFillablePage ( PageViewModel? scratchPage, double desiredScale ) 
    {
        bool isBadgeInsertionFirstTime = (scratchPage == null);

        if ( isBadgeInsertionFirstTime ) 
        {
            return new PageViewModel ( desiredScale);
        }
        else 
        {
            return scratchPage;
        }
    }


    private static List<PageViewModel> Place ( List<BadgeViewModel> placebleBadges, double desiredScale
                                             , PageViewModel fillablePage ) 
    {
        List<PageViewModel> result = new ();

        for ( int index = 0;   index < placebleBadges.Count;   index++ )
        {
            BadgeViewModel beingProcessedBadge = placebleBadges [index];
            beingProcessedBadge.SetCorrectScale (desiredScale);
            PageViewModel posibleNewPadge = fillablePage.AddBadge (beingProcessedBadge, false);
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