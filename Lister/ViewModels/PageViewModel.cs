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

namespace Lister.ViewModels;

class PageViewModel : ViewModelBase
{
    private readonly int badgeLimit;
    private int badgeCount;
    internal Size pageSize;

    private ObservableCollection<VMBadge> eB;
    internal ObservableCollection<VMBadge> EvenBadges
    {
        get { return eB; }
        set
        {
            this.RaiseAndSetIfChanged (ref eB, value, nameof (EvenBadges));
        }
    }

    private ObservableCollection<VMBadge> oB;
    internal ObservableCollection<VMBadge> OddBadges
    {
        get { return oB; }
        set
        {
            this.RaiseAndSetIfChanged (ref oB, value, nameof (OddBadges));
        }
    }

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

    private VMBadge bE;
    internal VMBadge BadgeExample
    {
        get { return bE; }
        set
        {
            this.RaiseAndSetIfChanged (ref bE, value, nameof (BadgeExample));
        }
    }
    private VMBadge badgeForVerifying;

    private double eLS;
    internal double EvenLeftShift
    {
        get { return eLS; }
        set
        {
            this.RaiseAndSetIfChanged (ref eLS, value, nameof (EvenLeftShift));
        }
    }

    private double oLS;
    internal double OddLeftShift
    {
        get { return oLS; }
        set
        {
            this.RaiseAndSetIfChanged (ref oLS, value, nameof (OddLeftShift));
        }
    }

    private double bTS;
    internal double BadgeStackTopShift
    {
        get { return bTS; }
        set
        {
            this.RaiseAndSetIfChanged (ref bTS, value, nameof (BadgeStackTopShift));
        }
    }

    internal List<VMBadge> IncludedBadges { get; private set; }
    private double scale;





    private ObservableCollection<BadgeLine> badges;
    internal ObservableCollection<BadgeLine> Badges
    {
        get { return badges; }
        set
        {
            this.RaiseAndSetIfChanged (ref badges, value, nameof (Badges));
        }
    }


    internal PageViewModel ( Size pageSize, VMBadge badgeExample, double desiredScale )
    {
        this.BadgeExample = badgeExample;
        this.pageSize = pageSize;
        scale = desiredScale;
        badgeCount = 0;
        EvenBadges = new ObservableCollection<VMBadge> ();
        OddBadges = new ObservableCollection<VMBadge> ();
        IncludedBadges = new List<VMBadge> ();
        PageWidth = pageSize.width;
        PageHeight = pageSize.height;
        EvenLeftShift = 46;
        OddLeftShift = 397;
        BadgeStackTopShift = 20;
        badgeLimit = GetAmountOfBadgesOnPage ();
        Zoom ();
    }


    internal static List<PageViewModel> PlaceIntoPages ( List<BadgeLine> placebleBadges, Size pageSize, double desiredScale
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


    internal PageViewModel AddBadge ( VMBadge badge, bool mustBeZoomed )
    {
        VerifyBadgeSizeAccordence (badge);

        if ( mustBeZoomed )
        {
            badge.Zoom (scale);
        }

        PageViewModel beingProcessedPage = this;
        bool timeToStartNewPage = ( beingProcessedPage.badgeCount == badgeLimit );

        if ( timeToStartNewPage )
        {
            beingProcessedPage = new PageViewModel (pageSize, BadgeExample, scale);
        }

        bool shouldPutInOddBadges = ( EvenBadges.Count ) > ( OddBadges.Count );

        if ( shouldPutInOddBadges )
        {
            beingProcessedPage.OddBadges.Add (badge);
        }
        else
        {
            beingProcessedPage.EvenBadges.Add (badge);
        }

        beingProcessedPage.badgeCount++;
        return beingProcessedPage;
    }


    internal void Clear ()
    {
        EvenBadges.Clear ();
        OddBadges.Clear ();
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

        int dffd = 0;

        if ( isNotAccordent )
        {
            throw new Exception ("Size of passed on badge is not according set for this page");
        }
    }


    private int GetAmountOfBadgesOnPage ()
    {
        int amountInRow = ( int ) ( scale * pageSize.width / BadgeExample.BadgeWidth );
        int amountOfRows = ( int ) ( scale * pageSize.height / BadgeExample.BadgeHeight );
        int resultAmount = amountInRow * amountOfRows;
        return resultAmount;
    }


    internal void ZoomOn ( double scaleCoefficient )
    {
        this.scale *= scaleCoefficient;
        PageHeight *= scaleCoefficient;
        PageWidth *= scaleCoefficient;
        EvenLeftShift *= scaleCoefficient;
        OddLeftShift *= scaleCoefficient;
        BadgeStackTopShift *= scaleCoefficient;

        if ( EvenBadges.Count > 0 )
        {
            for ( int badgeCounter = 0; badgeCounter < EvenBadges.Count; badgeCounter++ )
            {
                EvenBadges [badgeCounter].ZoomOn (scaleCoefficient);
            }
        }

        if ( OddBadges.Count > 0 )
        {
            for ( int badgeCounter = 0; badgeCounter < OddBadges.Count; badgeCounter++ )
            {
                OddBadges [badgeCounter].ZoomOn (scaleCoefficient);
            }
        }
    }


    internal void ZoomOut ( double scaleCoefficient )
    {
        scale /= scaleCoefficient;
        PageHeight /= scaleCoefficient;
        PageWidth /= scaleCoefficient;
        EvenLeftShift /= scaleCoefficient;
        OddLeftShift /= scaleCoefficient;
        BadgeStackTopShift /= scaleCoefficient;

        if ( EvenBadges.Count > 0 )
        {
            for ( int badgeCounter = 0; badgeCounter < EvenBadges.Count; badgeCounter++ )
            {
                EvenBadges [badgeCounter].ZoomOut (scaleCoefficient);
            }
        }

        if ( OddBadges.Count > 0 )
        {
            for ( int badgeCounter = 0; badgeCounter < OddBadges.Count; badgeCounter++ )
            {
                OddBadges [badgeCounter].ZoomOut (scaleCoefficient);
            }
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
            EvenLeftShift *= scale;
            OddLeftShift *= scale;
            BadgeStackTopShift *= scale;
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
}