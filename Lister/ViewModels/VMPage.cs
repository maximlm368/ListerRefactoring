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

class VMPage : ViewModelBase
{
    private readonly int badgeLimit;
    private int badgeCount;
    internal Size pageSize;

    private ObservableCollection<VMBadge> eB;
    internal ObservableCollection<VMBadge> evenBadges
    {
        get { return eB; }
        set
        {
            this.RaiseAndSetIfChanged (ref eB, value, nameof (evenBadges));
        }
    }

    private ObservableCollection<VMBadge> oB;
    internal ObservableCollection<VMBadge> oddBadges
    {
        get { return oB; }
        set
        {
            this.RaiseAndSetIfChanged (ref oB, value, nameof (oddBadges));
        }
    }

    private double pW;
    internal double pageWidth
    {
        get { return pW; }
        set
        {
            this.RaiseAndSetIfChanged (ref pW, value, nameof (pageWidth));
        }
    }

    private double pH;
    internal double pageHeight
    {
        get { return pH; }
        set
        {
            this.RaiseAndSetIfChanged (ref pH, value, nameof (pageHeight));
        }
    }

    private VMBadge bE;
    internal VMBadge badgeExample
    {
        get { return bE; }
        set
        {
            this.RaiseAndSetIfChanged (ref bE, value, nameof (badgeExample));
        }
    }
    private VMBadge badgeForVerifying;

    private double eLS;
    internal double evenLeftShift
    {
        get { return eLS; }
        set
        {
            this.RaiseAndSetIfChanged (ref eLS, value, nameof (evenLeftShift));
        }
    }

    private double oLS;
    internal double oddLeftShift
    {
        get { return oLS; }
        set
        {
            this.RaiseAndSetIfChanged (ref oLS, value, nameof (oddLeftShift));
        }
    }

    private double bTS;
    internal double badgeStackTopShift
    {
        get { return bTS; }
        set
        {
            this.RaiseAndSetIfChanged (ref bTS, value, nameof (badgeStackTopShift));
        }
    }

    internal List<VMBadge> includedBadges { get; private set; }
    private double scale;


    internal VMPage ( Size pageSize, VMBadge badgeExample, double desiredScale )
    {
        this.badgeExample = badgeExample;
        this.pageSize = pageSize;
        scale = desiredScale;
        badgeCount = 0;
        evenBadges = new ObservableCollection<VMBadge> ();
        oddBadges = new ObservableCollection<VMBadge> ();
        includedBadges = new List<VMBadge> ();
        pageWidth = pageSize.width;
        pageHeight = pageSize.height;
        evenLeftShift = 46;
        oddLeftShift = 398;
        badgeStackTopShift = 30;
        badgeLimit = GetAmountOfBadgesOnPage ( );
        Zoom ();
    }


    internal static List<VMPage> PlaceIntoPages ( List<VMBadge> splitableList,  Size pageSize,  double desiredScale
                                                                                            , VMPage ? scratchPage ) 
    {
        List<VMPage> result = new List<VMPage> ();

        if ( splitableList.Count == 0 )
        {
            return result;
        }

        VMBadge badgeExample = splitableList [0].Clone ();
        VMPage fillablePage = scratchPage ?? new VMPage ( pageSize, badgeExample, desiredScale );
       
        for ( int badgeCounter = 0;   badgeCounter < splitableList.Count;   badgeCounter++ ) 
        {
            VMBadge beingProcessedBadge = splitableList [badgeCounter];
            beingProcessedBadge.Zoom (desiredScale);
            VMPage posibleNewPadge = fillablePage.AddBadge (beingProcessedBadge, false);
            bool timeToAddNewPage = ! posibleNewPadge.Equals(fillablePage);

            if (timeToAddNewPage) 
            {
                result.Add (fillablePage);
                fillablePage = posibleNewPadge;
            }

            if(badgeCounter == splitableList.Count - 1 ) 
            {
                result.Add (fillablePage);
            }
        }

        return result;
    }


    //private VMPage GetPageWithDesiredScale ( Size pageSize, VMBadge badgeExample, double desiredScale ) 
    //{
    //    VMPage fillablePage = new VMPage (pageSize, badgeExample, desiredScale);
    //    fillablePage.Zoom ();
    //    return fillablePage;
    //}


    internal VMPage AddBadge ( VMBadge badge, bool mustBeZoomed )
    {
        VerifyBadgeSizeAccordence (badge);

        if ( mustBeZoomed ) 
        {
            badge.Zoom (scale);
        }

        VMPage beingProcessedPage = this;
        bool timeToStartNewPage = (beingProcessedPage.badgeCount == badgeLimit);

        if ( timeToStartNewPage )
        {
            beingProcessedPage = new VMPage (pageSize, badgeExample, scale);
        }

        bool shouldPutInOddBadges = (evenBadges. Count) > (oddBadges. Count);

        if ( shouldPutInOddBadges )
        {
            beingProcessedPage.oddBadges.Add (badge);
        }
        else
        {
            beingProcessedPage.evenBadges.Add (badge);
        }

        beingProcessedPage.badgeCount++;
        return beingProcessedPage;
    }


    internal void Clear () 
    {
        evenBadges.Clear ();
        oddBadges.Clear ();
        badgeCount = 0;
    }


    private void VerifyBadgeSizeAccordence ( VMBadge badge ) 
    {
        Size verifiebleSize = badge.badgeModel. badgeDescription. badgeDimensions. outlineSize;
        int verifiebleWidth = (int) (scale * verifiebleSize.width);
        int verifiebleHeight = (int) (scale * verifiebleSize.height);

        bool isNotAccordent = ( verifiebleWidth != (int) this.badgeExample.badgeWidth ) 
                              ||
                              ( verifiebleHeight != (int) this.badgeExample.badgeHeight );

        int dffd = 0;

        if ( isNotAccordent )
        {
            throw new Exception ("Size of passed on badge is not according set for this page");
        }
    }


    private int GetAmountOfBadgesOnPage ( )
    {
        int amountInRow = ( int ) ( scale * pageSize.width / badgeExample. badgeWidth );
        int amountOfRows = ( int ) ( scale * pageSize.height / badgeExample. badgeHeight );
        int resultAmount = amountInRow * amountOfRows;
        return resultAmount;
    }


    internal void ZoomOn ( double scaleCoefficient ) 
    {
        this.scale *= scaleCoefficient;
        pageHeight *= scaleCoefficient;
        pageWidth *= scaleCoefficient;
        evenLeftShift *= scaleCoefficient;
        oddLeftShift *= scaleCoefficient;
        badgeStackTopShift *= scaleCoefficient;

        if ( evenBadges.Count > 0 ) 
        {
            for( int badgeCounter = 0;   badgeCounter < evenBadges. Count;   badgeCounter++ ) 
            {
                evenBadges [badgeCounter].ZoomOn ( scaleCoefficient );
            }
        }

        if ( oddBadges.Count > 0 )
        {
            for ( int badgeCounter = 0;   badgeCounter < oddBadges. Count;   badgeCounter++ )
            {
                oddBadges [badgeCounter].ZoomOn (scaleCoefficient);
            }
        }
    }


    internal void ZoomOut ( double scaleCoefficient )
    {
        scale /= scaleCoefficient;
        pageHeight /= scaleCoefficient;
        pageWidth /= scaleCoefficient;
        evenLeftShift /= scaleCoefficient;
        oddLeftShift /= scaleCoefficient;
        badgeStackTopShift /= scaleCoefficient;

        if ( evenBadges.Count > 0 )
        {
            for ( int badgeCounter = 0; badgeCounter < evenBadges.Count; badgeCounter++ )
            {
                evenBadges [badgeCounter].ZoomOut (scaleCoefficient);
            }
        }

        if ( oddBadges.Count > 0 )
        {
            for ( int badgeCounter = 0; badgeCounter < oddBadges.Count; badgeCounter++ )
            {
                oddBadges [badgeCounter].ZoomOut (scaleCoefficient);
            }
        }
    }


    internal void ZoomOnExampleBadge ( double scaleCoefficient )
    {
        badgeExample.ZoomOn (scaleCoefficient);
    }


    internal void ZoomOutExampleBadge ( double scaleCoefficient )
    {
        badgeExample.ZoomOut (scaleCoefficient);
    }


    private void Zoom ( ) 
    {
        if ( scale != 1 ) 
        {
            pageHeight *= scale;
            pageWidth *= scale;
            evenLeftShift *= scale;
            oddLeftShift *= scale;
            badgeStackTopShift *= scale;
        }
    }


    internal void ShowBadges ()
    {
        bool evenStackIsNotEmpty = evenBadges. Count > 0;
        bool oddStackIsNotEmpty = oddBadges. Count > 0;

        if( evenStackIsNotEmpty ) 
        {
            for ( int badgeCounter = 0;   badgeCounter < evenBadges.Count;   badgeCounter++ )
            {
                evenBadges [badgeCounter].ShowBackgroundImage ();
            }
        }

        if ( oddStackIsNotEmpty )
        {
            for ( int badgeCounter = 0; badgeCounter < oddBadges.Count; badgeCounter++ )
            {
                oddBadges [badgeCounter].ShowBackgroundImage ();
            }
        }
    }


    internal void HideBadges ()
    {
        bool evenStackIsNotEmpty = evenBadges.Count > 0;
        bool oddStackIsNotEmpty = oddBadges.Count > 0;

        if ( evenStackIsNotEmpty ) 
        {
            for ( int badgeCounter = 0;   badgeCounter < evenBadges.Count;   badgeCounter++ )
            {
                evenBadges [badgeCounter].HideBackgroundImage ();
            }
        }

        if ( oddStackIsNotEmpty ) 
        {
            for ( int badgeCounter = 0; badgeCounter < oddBadges.Count; badgeCounter++ )
            {
                oddBadges [badgeCounter].HideBackgroundImage ();
            }
        }
    }
}