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
using ExtentionsAndAuxiliary;

namespace Lister.ViewModels;

public class BadgeViewModel : ViewModelBase
{
    private const double coefficient = 1.1;
    internal Badge BadgeModel { get; private set; }
    internal Bitmap ImageBitmap { get; private set; }

    private double _widht;
    private double _height;

    private double bW;
    internal double BadgeWidth
    {
        get { return bW; }
        set
        {
            this.RaiseAndSetIfChanged (ref bW, value, nameof (BadgeWidth));
        }
    }

    private double bH;
    internal double BadgeHeight
    {
        get { return bH; }
        set
        {
            this.RaiseAndSetIfChanged (ref bH, value, nameof (BadgeHeight));
        }
    }

    private ObservableCollection <TextLineViewModel> tL;
    internal ObservableCollection <TextLineViewModel> TextLines
    {
        get { return tL; }
        set
        {
            this.RaiseAndSetIfChanged (ref tL, value, nameof (TextLines));
        }
    }

    private ObservableCollection <ImageViewModel> iI;
    internal ObservableCollection <ImageViewModel> InsideImages
    {
        get { return iI; }
        set
        {
            this.RaiseAndSetIfChanged (ref iI, value, nameof (InsideImages));
        }
    }

    private ObservableCollection <ImageViewModel> rs;
    internal ObservableCollection <ImageViewModel> InsideShapes
    {
        get { return rs; }
        set
        {
            this.RaiseAndSetIfChanged (ref rs, value, nameof (InsideShapes));
        }
    }

    private double _borderThickness;
    private Avalonia.Thickness bT;
    internal Avalonia.Thickness BorderThickness
    {
        get { return bT; }
        set
        {
            this.RaiseAndSetIfChanged (ref bT, value, nameof (BorderThickness));
        }
    }

    //private double cT;
    //internal double CanvasTop
    //{
    //    get { return cT; }
    //    set
    //    {
    //        this.RaiseAndSetIfChanged (ref cT, value, nameof (CanvasTop));
    //    }
    //}

    //private double cL;
    //internal double CanvasLeft
    //{
    //    get { return cL; }
    //    set
    //    {
    //        this.RaiseAndSetIfChanged (ref cL, value, nameof (CanvasLeft));
    //    }
    //}

    internal bool IsCorrect { get; private set; }


    public BadgeViewModel ( Badge badgeModel )
    {
        BadgeModel = badgeModel;
        BadgeLayout layout = badgeModel.BadgeLayout;
        BadgeWidth = layout.OutlineSize. Width;
        _widht = BadgeWidth;
        BadgeHeight = layout.OutlineSize. Height;
        _height = BadgeHeight;
        TextLines = new ObservableCollection<TextLineViewModel> ();
        InsideImages = new ObservableCollection<ImageViewModel> ();
        InsideShapes = new ObservableCollection<ImageViewModel> ();
        IsCorrect = true;
        _borderThickness = 1;
        BorderThickness = new Avalonia.Thickness (_borderThickness);

        List<TextualAtom> atoms = layout.TextualFields;
        OrderTextlinesByVertical (atoms);
        SetUpTextLines (atoms);

        List<InsideImage> images = layout.InsideImages;
        SetUpImagesAndGeometryElements (images);
    }


    private BadgeViewModel ( BadgeViewModel badge )
    {
        BadgeModel = badge.BadgeModel;
        BadgeLayout layout = BadgeModel. BadgeLayout;
        BadgeWidth = layout.OutlineSize. Width;
        BadgeHeight = layout.OutlineSize. Height;
        TextLines = new ObservableCollection<TextLineViewModel> ();
        InsideImages = new ObservableCollection<ImageViewModel> ();
        InsideShapes = new ObservableCollection<ImageViewModel> ();
        IsCorrect = true;
        _borderThickness = 1;
        BorderThickness = new Avalonia.Thickness (_borderThickness);

        foreach ( TextLineViewModel line   in   badge.TextLines ) 
        {
            TextLineViewModel original = line.GetDimensionalOriginal ();
            TextLines.Add (original);
        }

        //foreach ( ImageViewModel image   in   badge.InsideImages )
        //{
        //    TextLineViewModel original = image.GetDimensionalOriginal ();
        //    TextLines.Add (original);
        //}

        //foreach ( ImageViewModel shape   in   badge.InsideShapes )
        //{
        //    TextLineViewModel original = shape.GetDimensionalOriginal ();
        //    TextLines.Add (original);
        //}
    }


    internal BadgeViewModel GetDimensionalOriginal () 
    {
        BadgeViewModel original = new BadgeViewModel ( this );
        return original;
    }


    internal void Show ()
    {
        string path = BadgeModel. BackgroundImagePath;
        Uri uri = new Uri (path);
        this.ImageBitmap = ImageHelper.LoadFromResource (uri);

        //try 
        //{
        //    Uri uri = new Uri (path);
        //    this.ImageBitmap = ImageHelper.LoadFromResource (uri);
        //}
        //catch( UriFormatException ex ) 
        //{
        //    string mess = ex.Message;
        //    int jk = 0;
        //}

    }


    internal void Hide ()
    {
        this.ImageBitmap = null;
    }


    internal void ZoomOn ( double coefficient )
    {
        BadgeWidth *= coefficient;
        BadgeHeight *= coefficient;

        foreach ( TextLineViewModel line   in   TextLines ) 
        {
            line.ZoomOn (coefficient);
        }

        foreach ( ImageViewModel image   in   InsideImages )
        {
            image.ZoomOn (coefficient);
        }

        foreach ( ImageViewModel shape   in   InsideShapes )
        {
            shape.ZoomOn (coefficient);
        }

        _borderThickness *= coefficient;
        BorderThickness = new Avalonia.Thickness (_borderThickness);
    }


    internal void ZoomOut ( double coefficient )
    {
        BadgeWidth /= coefficient;
        BadgeHeight /= coefficient;

        foreach ( TextLineViewModel line   in   TextLines )
        {
            line.ZoomOut (coefficient);
        }

        foreach ( ImageViewModel image   in   InsideImages )
        {
            image.ZoomOut (coefficient);
        }

        foreach ( ImageViewModel shape   in   InsideShapes )
        {
            shape.ZoomOut (coefficient);
        }

        _borderThickness /= coefficient;
        BorderThickness = new Avalonia.Thickness (_borderThickness);
    }


    internal BadgeViewModel Clone ()
    {
        BadgeViewModel clone = new BadgeViewModel (BadgeModel);
        return clone;
    }


    internal void SetCorrectScale ( double scale )
    {
        if ( scale != 1 )
        {
            ZoomOn (scale);
        }
    }


    private void SetUpTextLines ( List<TextualAtom> orderedTextualFields )
    {
        double summaryVerticalOffset = 0;

        for ( int index = 0;   index < orderedTextualFields.Count;   index++ ) 
        {
            TextualAtom textAtom = orderedTextualFields [index];
            double fontSize = textAtom.FontSize;
            double lineLength = textAtom.Width;
            double topOffset = textAtom.TopOffset;
            string beingProcessedLine = textAtom.Content;
            string additionalLine = string.Empty;

            while ( true )
            {
                FormattedText formatted = new FormattedText (beingProcessedLine, CultureInfo.CurrentCulture
                                                           , FlowDirection.LeftToRight, Typeface.Default, fontSize, null);
                
                double usefulTextBlockWidth = formatted.Width * coefficient;
                bool lineIsOverflow = ( usefulTextBlockWidth >= lineLength );

                if ( ! lineIsOverflow ) 
                {
                    if ( textAtom.IsShiftableBelow )
                    {
                        topOffset += summaryVerticalOffset;
                    }

                    textAtom.TopOffset = topOffset;
                    textAtom.Content = beingProcessedLine;
                    TextLineViewModel textLine = new TextLineViewModel (textAtom);
                    TextLines.Add (textLine);

                    if ( additionalLine != string.Empty ) 
                    {
                        beingProcessedLine = additionalLine;
                        additionalLine = string.Empty;
                        continue;
                    }
                    else 
                    {
                        break;
                    }
                }

                List<string> splited = beingProcessedLine.SeparateTail ();

                if ( splited.Count > 0 )
                {
                    beingProcessedLine = splited [0];
                    additionalLine = splited [1] + " " + additionalLine;

                    summaryVerticalOffset += textAtom.Height;
                    topOffset += textAtom.Height;
                }
                else
                {
                    this.IsCorrect = false;
                    break;
                }
            }
        }
    }


    private void OrderTextlinesByVertical ( List<TextualAtom> textualFields )
    {
        for ( int index = 0;   index < textualFields. Count;   index++ )
        {
            for ( int num = index;   num < textualFields.Count - 1;   num++ ) 
            {
                TextualAtom current = textualFields [index];
                TextualAtom next = textualFields [index + 1];

                if ( current.TopOffset > next.TopOffset )
                {
                    TextualAtom reserve = textualFields [index];
                    textualFields [index] = next;
                    textualFields [index + 1] = reserve;
                }
            }
        }
    }


    private void SetUpImagesAndGeometryElements ( List<InsideImage> insideImages ) 
    {
        for ( int index = 0;   index < insideImages.Count;   index++ )
        {
            InsideImage image = insideImages [index];
            ImageViewModel imageVM = new ImageViewModel (image);

            if (image.ImageKind == ImageType.image)
            {
                InsideImages.Add (imageVM);
            }

            if ( image.ImageKind == ImageType.geometricElement )
            {
                InsideShapes.Add (imageVM);
            }
        }
    }
}