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

class PageViewModel : ViewModelBase
{
    private readonly int lineLimit;
    private int badgeCount;
    internal Size pageSize;


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

    private BadgeViewModel bE;
    internal BadgeViewModel BadgeExample
    {
        get { return bE; }
        set
        {
            this.RaiseAndSetIfChanged (ref bE, value, nameof (BadgeExample));
        }
    }
    private VMBadge badgeForVerifying;



    internal List<VMBadge> IncludedBadges { get; private set; }
    private double scale;

    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>


    private double lCLS;
    internal double LinesContainerLeftShift
    {
        get { return lCLS; }
        set
        {
            this.RaiseAndSetIfChanged (ref lCLS, value, nameof (LinesContainerLeftShift));
        }
    }

    private double lCTS;
    internal double LinesContainerTopShift
    {
        get { return lCTS; }
        set
        {
            this.RaiseAndSetIfChanged (ref lCTS, value, nameof (LinesContainerTopShift));
        }
    }

    private ObservableCollection<BadgeLine> llines;
    internal ObservableCollection<BadgeLine> Lines
    {
        get { return llines; }
        set
        {
            this.RaiseAndSetIfChanged (ref llines, value, nameof (Lines));
        }
    }

    private BadgeLine currentLine;


    internal PageViewModel ( Size pageSize, BadgeViewModel badgeExample, double desiredScale )
    {
        Lines = new ObservableCollection<BadgeLine> ();
        currentLine = new BadgeLine ();
        Lines.Add (currentLine);
        BadgeExample = badgeExample;
        scale = desiredScale;
        badgeCount = 0;
        IncludedBadges = new List<VMBadge> ();
        PageWidth = pageSize.width;
        PageHeight = pageSize.height;
        LinesContainerLeftShift = CalculateLeftShift ();
        LinesContainerTopShift = CalculateTopShift ();
        lineLimit = GetMaxLineAmount ();
        Zoom ();
    }


    internal static List<PageViewModel> PlaceIntoPagesss ( List<BadgeLine> placebleBadges, Size pageSize, double desiredScale
                                                                                            , PageViewModel ? scratchPage )
    {
        List<PageViewModel> result = new ();

        //if ( placebleBadges.Count == 0 )
        //{
        //    return result;
        //}

        //VMBadge badgeExample = placebleBadges [0].Clone ();
        //PageViewModel fillablePage = scratchPage ?? new PageViewModel (pageSize, badgeExample, desiredScale);

        //for ( int badgeCounter = 0; badgeCounter < placebleBadges.Count; badgeCounter++ )
        //{
        //    BadgeLine beingProcessedBadge = placebleBadges [badgeCounter];
        //    beingProcessedBadge.Zoom (desiredScale);
        //    PageViewModel posibleNewPadge = fillablePage.AddBadge (beingProcessedBadge, false);
        //    bool timeToAddNewPage = !posibleNewPadge.Equals (fillablePage);

        //    if ( timeToAddNewPage )
        //    {
        //        result.Add (fillablePage);
        //        fillablePage = posibleNewPadge;
        //    }

        //    if ( badgeCounter == placebleBadges.Count - 1 )
        //    {
        //        result.Add (fillablePage);
        //    }
        //}

        return result;
    }


    internal static List<PageViewModel> ? PlaceIntoPages ( List<BadgeViewModel> placebleBadges,
                                                         Size pageSize, double desiredScale, PageViewModel ? scratchPage )
    {
        bool areArgumentsInvalid = AreArgumentsInvalid (placebleBadges, pageSize, desiredScale);

        if ( areArgumentsInvalid )
        {
            return null;
        }

        BadgeViewModel badgeExample = placebleBadges [0].Clone ();
        
        //PageViewModel fillablePage = scratchPage ?? new PageViewModel (pageSize, badgeExample, desiredScale);

        PageViewModel fillablePage = DefineFillablePage(scratchPage, pageSize, desiredScale, badgeExample);

        return Place (placebleBadges, desiredScale, fillablePage);
    }


    internal PageViewModel AddBadge ( BadgeViewModel badge, bool mustBeZoomed )
    {
        VerifyBadgeSizeAccordence (badge);

        if ( mustBeZoomed )
        {
            badge.Zoom (scale);
        }

        PageViewModel beingProcessedPage = this;

        ActionSuccess additionSuccess = currentLine.AddBadge (badge);

        if ( additionSuccess == ActionSuccess.Failure ) 
        {
            currentLine = new BadgeLine ();
            additionSuccess = currentLine.AddBadge (badge);

            if ( additionSuccess == ActionSuccess.Failure )
            {
                throw new PageException ( "Badge has width succeeds pages width" );
            }

            bool timeToStartNewPage = ( Lines. Count == lineLimit );

            if ( timeToStartNewPage )
            {
                beingProcessedPage = new PageViewModel (pageSize, BadgeExample, scale);
            }

            beingProcessedPage.Lines.Add (currentLine);
        }

        beingProcessedPage.badgeCount++;
        return beingProcessedPage;
    }


    internal void Clear ()
    {
        badgeCount = 0;
    }


    private void VerifyBadgeSizeAccordence ( VMBadge badge )
    {
        Size verifiebleSize = badge.BadgeModel.badgeDescription.badgeDimensions.outlineSize;
        int verifiebleWidth = ( int ) ( scale * verifiebleSize.width );
        int verifiebleHeight = ( int ) ( scale * verifiebleSize.height );

        bool isNotAccordent = ( verifiebleWidth != ( int ) this.BadgeExample.BadgeWidth )
                              ||
                              ( verifiebleHeight != ( int ) this.BadgeExample.BadgeHeight );

        if ( isNotAccordent )
        {
            throw new Exception ("Size of passed on badge is not according set for this page");
        }
    }


    private int GetMaxLineAmount ()
    {
        return ( int ) ( PageHeight / BadgeExample.BadgeHeight );
    }


    internal void ZoomOn ( double scaleCoefficient )
    {
        this.scale *= scaleCoefficient;
        PageHeight *= scaleCoefficient;
        PageWidth *= scaleCoefficient;
        LinesContainerLeftShift *= scaleCoefficient;
        LinesContainerTopShift *= scaleCoefficient;

        for ( int index = 0;   index < Lines. Count;   index++ )
        {
            Lines [index].ZoomOn (scaleCoefficient);
        }
    }


    internal void ZoomOut ( double scaleCoefficient )
    {
        scale /= scaleCoefficient;
        PageHeight /= scaleCoefficient;
        PageWidth /= scaleCoefficient;
        LinesContainerLeftShift /= scaleCoefficient;
        LinesContainerTopShift /= scaleCoefficient;

        for ( int index = 0;   index < Lines. Count;   index++ )
        {
            Lines [index].ZoomOut (scaleCoefficient);
        }
    }


    internal void ZoomOnExampleBadge ( double scaleCoefficient )
    {
        BadgeExample.ZoomOn (scaleCoefficient);
    }


    internal void ZoomOutExampleBadge ( double scaleCoefficient )
    {
        BadgeExample.ZoomOut (scaleCoefficient);
    }


    private void Zoom ()
    {
        if ( scale != 1 )
        {
            PageHeight *= scale;
            PageWidth *= scale;
            LinesContainerLeftShift *= scale;
            LinesContainerTopShift *= scale;
        }
    }


    internal void ShowBadges ()
    {
        bool evenStackIsNotEmpty = EvenBadges.Count > 0;
        bool oddStackIsNotEmpty = OddBadges.Count > 0;

        if ( evenStackIsNotEmpty )
        {
            for ( int badgeCounter = 0; badgeCounter < EvenBadges.Count; badgeCounter++ )
            {
                EvenBadges [badgeCounter].ShowBackgroundImage ();
            }
        }

        if ( oddStackIsNotEmpty )
        {
            for ( int badgeCounter = 0; badgeCounter < OddBadges.Count; badgeCounter++ )
            {
                OddBadges [badgeCounter].ShowBackgroundImage ();
            }
        }
    }


    internal void HideBadges ()
    {
        bool evenStackIsNotEmpty = EvenBadges.Count > 0;
        bool oddStackIsNotEmpty = OddBadges.Count > 0;

        if ( evenStackIsNotEmpty )
        {
            for ( int badgeCounter = 0; badgeCounter < EvenBadges.Count; badgeCounter++ )
            {
                EvenBadges [badgeCounter].HideBackgroundImage ();
            }
        }

        if ( oddStackIsNotEmpty )
        {
            for ( int badgeCounter = 0; badgeCounter < OddBadges.Count; badgeCounter++ )
            {
                OddBadges [badgeCounter].HideBackgroundImage ();
            }
        }
    }


    private double CalculateLeftShift () 
    {
        double shift = 0;

        return shift;
    }


    private double CalculateTopShift ()
    {
        double shift = 0;

        return shift;
    }


    private static bool AreArgumentsInvalid ( List<BadgeViewModel> placebleBadges, Size pageSize, double desiredScale )
    {
        bool areArgumentsInvalid = ( placebleBadges == null );
        areArgumentsInvalid = areArgumentsInvalid || ( placebleBadges.Count < 1 );
        areArgumentsInvalid = areArgumentsInvalid || ( pageSize == null );
        areArgumentsInvalid = areArgumentsInvalid || ( pageSize.width == 0 );
        areArgumentsInvalid = areArgumentsInvalid || ( pageSize.height == 0 );
        areArgumentsInvalid = areArgumentsInvalid || ( desiredScale == 0 );

        return areArgumentsInvalid;
    }


    private static PageViewModel DefineFillablePage ( PageViewModel? scratchPage, Size pageSize, double desiredScale,
                                                      BadgeViewModel badgeExample ) 
    {
        bool isBadgeInsertionFirstTime = (scratchPage == null);

        if ( isBadgeInsertionFirstTime ) 
        {
            return new PageViewModel (pageSize, badgeExample, desiredScale);
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
            beingProcessedBadge.Zoom (desiredScale);
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



public class PageException : Exception 
{
    public PageException ( string message ) 
    {
        base.Message = message ?? string.Empty;
    }
}