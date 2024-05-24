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
    
    private double badgeLineWidth;

    private double lCLS;
    internal double LinesLeftShift
    {
        get { return lCLS; }
        set
        {
            this.RaiseAndSetIfChanged (ref lCLS, value, nameof (LinesLeftShift));
        }
    }

    private double lCTS;
    internal double FirstLineTopShift
    {
        get { return lCTS; }
        set
        {
            this.RaiseAndSetIfChanged (ref lCTS, value, nameof (FirstLineTopShift));
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
        BadgeExample = badgeExample;
        scale = desiredScale;
        badgeCount = 0;
        IncludedBadges = new List<VMBadge> ();
        PageWidth = pageSize.Width;
        PageHeight = pageSize.Height;
        LinesLeftShift = CalculateLeftShift ();
        FirstLineTopShift = CalculateTopShift ();
        lineLimit = GetMaxLineAmount ();
        badgeLineWidth = pageSize.Width - 2 * LinesLeftShift;
        currentLine = new BadgeLine (badgeLineWidth, scale);
        Lines.Add (currentLine);
        SetCorrectScale ();
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
        PageViewModel beingProcessedPage = this;
        ActionSuccess additionSuccess = currentLine.AddBadge (badge);

        if ( additionSuccess == ActionSuccess.Failure ) 
        {
            currentLine = new BadgeLine (badgeLineWidth, scale);
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
        int verifiebleWidth = ( int ) ( scale * verifiebleSize.Width );
        int verifiebleHeight = ( int ) ( scale * verifiebleSize.Height );

        bool isNotAccordent = ( verifiebleWidth != ( int ) BadgeExample. BadgeWidth )
                              ||
                              ( verifiebleHeight != ( int ) BadgeExample. BadgeHeight );

        if ( isNotAccordent )
        {
            throw new Exception ("Size of passed on badge is not according set for this page");
        }
    }


    private int GetMaxLineAmount ()
    {
        double badgeHeight = BadgeExample. BadgeHeight;
        return ( int ) ( PageHeight / badgeHeight );
    }


    internal void ZoomOn ( double scaleCoefficient )
    {
        this.scale *= scaleCoefficient;
        PageHeight *= scaleCoefficient;
        PageWidth *= scaleCoefficient;
        LinesLeftShift *= scaleCoefficient;
        FirstLineTopShift *= scaleCoefficient;

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
        LinesLeftShift /= scaleCoefficient;
        FirstLineTopShift /= scaleCoefficient;

        for ( int index = 0;   index < Lines. Count;   index++ )
        {
            Lines [index].ZoomOut (scaleCoefficient);
        }
    }


    private void SetCorrectScale ()
    {
        if ( scale != 1 )
        {
            PageHeight *= scale;
            PageWidth *= scale;
            LinesLeftShift *= scale;
            FirstLineTopShift *= scale;
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


    private double CalculateLeftShift () 
    {
        double shift = 0

        return shift;
    }


    private double CalculateTopShift ()
    {
        double shift = 0

        return shift;
    }


    private static bool AreArgumentsInvalid ( List<BadgeViewModel> placebleBadges, Size pageSize, double desiredScale )
    {
        bool areArgumentsInvalid = ( placebleBadges == null );
        areArgumentsInvalid = areArgumentsInvalid || ( placebleBadges.Count < 1 );
        areArgumentsInvalid = areArgumentsInvalid || ( pageSize == null );
        areArgumentsInvalid = areArgumentsInvalid || ( pageSize.Width == 0 );
        areArgumentsInvalid = areArgumentsInvalid || ( pageSize.Height == 0 );
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



public class PageException : Exception 
{
    public PageException ( string message ) 
    {
        base.Message = message ?? string.Empty;
    }
}