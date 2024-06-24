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
using Avalonia.Controls.Shapes;

namespace Lister.ViewModels;

public class BadgeViewModel : ViewModelBase
{
    private static Dictionary<string, Bitmap> _pathToImage;
    private static double _rightSpan = 5;

    private const double coefficient = 1.03;
    internal double Scale { get; private set; }
    internal Badge BadgeModel { get; private set; }

    private Bitmap _bitMap = null;
    private Bitmap bM;
    internal Bitmap ImageBitmap
    {
        get { return bM; }
        private set
        {
            this.RaiseAndSetIfChanged (ref bM, value, nameof (ImageBitmap));
        }

    }

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

    private double boW;
    internal double BorderWidth
    {
        get { return boW; }
        set
        {
            this.RaiseAndSetIfChanged (ref boW, value, nameof (BorderWidth));
        }
    }

    private double boH;
    internal double BorderHeight
    {
        get { return boH; }
        set
        {
            this.RaiseAndSetIfChanged (ref boH, value, nameof (BorderHeight));
        }
    }

    internal TextLineViewModel FocusedLine { get; set; }
    private List <TextLineViewModel> BorderViolentLines { get; set; }
    private List <TextLineViewModel> OverlayViolentLines { get; set; }
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

    internal bool IsCorrect { get; private set; }


    static BadgeViewModel ( )
    {
        _pathToImage = new Dictionary<string , Bitmap> ( );
    }


    public BadgeViewModel ( Badge badgeModel )
    {
        BadgeModel = badgeModel;
        BadgeLayout layout = badgeModel.BadgeLayout;
        BadgeWidth = layout.OutlineSize. Width;
        BorderWidth = BadgeWidth + 2;
        _widht = BadgeWidth;
        BadgeHeight = layout.OutlineSize. Height;
        BorderHeight = BadgeHeight + 3;
        _height = BadgeHeight;
        TextLines = new ObservableCollection<TextLineViewModel> ();
        BorderViolentLines = new List <TextLineViewModel> ( );
        OverlayViolentLines = new List <TextLineViewModel> ();
        InsideImages = new ObservableCollection<ImageViewModel> ();
        InsideShapes = new ObservableCollection<ImageViewModel> ();
        IsCorrect = true;
        _borderThickness = 1;
        Scale = 1;
        BorderThickness = new Avalonia.Thickness (_borderThickness);

        List<TextualAtom> atoms = layout.TextualFields;
        OrderTextlinesByVertical (atoms);
        SetUpTextLines (atoms);
        GatherIncorrectLines ( );

        List<InsideImage> images = layout.InsideImages;
        SetUpImagesAndGeometryElements (images);
    }


    private BadgeViewModel ( BadgeViewModel badge )
    {
        BadgeModel = badge.BadgeModel;
        BadgeLayout layout = BadgeModel. BadgeLayout;
        BadgeWidth = layout.OutlineSize. Width;
        BorderWidth = BadgeWidth + 2;
        BadgeHeight = layout.OutlineSize. Height;
        BorderHeight = BadgeHeight + 2;
        TextLines = new ObservableCollection<TextLineViewModel> ();
        BorderViolentLines = new List<TextLineViewModel> ();
        OverlayViolentLines = new List<TextLineViewModel> ();
        InsideImages = new ObservableCollection<ImageViewModel> ();
        InsideShapes = new ObservableCollection<ImageViewModel> ();
        IsCorrect = true;
        _borderThickness = 1;
        Scale = 1;
        BorderThickness = new Avalonia.Thickness (_borderThickness);

        foreach ( TextLineViewModel line   in   badge.TextLines ) 
        {
            TextLineViewModel original = line.GetDimensionalOriginal ();
            TextLines.Add (original);
        }

        //foreach ( ImageViewModel image in badge.InsideImages )
        //{
        //    TextLineViewModel original = image.GetDimensionalOriginal ();
        //    TextLines.Add (original);
        //}

        //foreach ( ImageViewModel shape in badge.InsideShapes )
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
        if( _bitMap == null )
        {
            string path = BadgeModel. BackgroundImagePath;
            Uri uri = new Uri ( path );
            string absolutePath = uri.AbsolutePath;

            if ( ! _pathToImage.ContainsKey ( absolutePath ) ) 
            {
                _bitMap = ImageHelper.LoadFromResource ( uri );
                _pathToImage.Add ( absolutePath , _bitMap );
            }
            else
            {
                _bitMap = _pathToImage [ absolutePath ];
            }
        }

        this.ImageBitmap = _bitMap;
    }


    internal void Hide ()
    {
        this.ImageBitmap = null;
    }


    internal void ZoomOn ( double coefficient )
    {
        BadgeWidth *= coefficient;
        BadgeHeight *= coefficient;
        BorderWidth *= coefficient;
        BorderHeight *= coefficient;
        Scale *= coefficient;

        foreach ( TextLineViewModel line   in   TextLines ) 
        {
            line.ZoomOn (coefficient);
        }

        //foreach ( ImageViewModel image   in   InsideImages )
        //{
        //    image.ZoomOn (coefficient);
        //}

        //foreach ( ImageViewModel shape   in   InsideShapes )
        //{
        //    shape.ZoomOn (coefficient);
        //}

        _borderThickness *= coefficient;
        BorderThickness = new Avalonia.Thickness (_borderThickness);
    }


    internal void ZoomOut ( double coefficient )
    {
        BadgeWidth /= coefficient;
        BadgeHeight /= coefficient;
        BorderWidth /= coefficient;
        BorderHeight /= coefficient;
        Scale /= coefficient;

        foreach ( TextLineViewModel line   in   TextLines )
        {
            line.ZoomOut (coefficient);
        }

        //foreach ( ImageViewModel image   in   InsideImages )
        //{
        //    image.ZoomOut (coefficient);
        //}

        //foreach ( ImageViewModel shape   in   InsideShapes )
        //{
        //    shape.ZoomOut (coefficient);
        //}

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


    internal void ReplaceTextLine ( TextLineViewModel replaceble, List <TextLineViewModel> replacing )
    {
        TextLines.Remove (replaceble);

        foreach ( TextLineViewModel line   in   replacing )
        {
            TextLines.Add (line);
        }
    }


    private void SetUpTextLines ( List <TextualAtom> orderedTextualFields )
    {
        double summaryVerticalOffset = 0;

        for ( int index = 0;   index < orderedTextualFields.Count;   index++ ) 
        {
            bool isSplitingOccured = false;
            bool isTimeToShiftNextLine = false;
            TextualAtom textAtom = orderedTextualFields [index];
            double fontSize = textAtom.FontSize;
            double lineLength = textAtom.Width;
            textAtom.TopOffset += summaryVerticalOffset;
            double topOffset = textAtom.TopOffset;
            string beingProcessedLine = textAtom.Content;
            string additionalLine = string.Empty;

            while ( true )
            {
                Typeface face = new Typeface (new FontFamily("arial"), FontStyle.Normal, FontWeight.Normal);
                FormattedText formatted = new FormattedText (beingProcessedLine, CultureInfo.CurrentCulture
                                                                    , FlowDirection.LeftToRight, face, fontSize, null);
                double usefulTextBlockWidth = formatted.Width;
                bool lineIsOverflow = ( usefulTextBlockWidth >= lineLength );

                if ( ! lineIsOverflow ) 
                {
                    if ( isTimeToShiftNextLine )
                    {
                        summaryVerticalOffset += textAtom.FontSize;
                        topOffset += textAtom.FontSize;
                    }

                    if ( isSplitingOccured )
                    {
                        isTimeToShiftNextLine = true;
                    }

                    TextualAtom atom = new TextualAtom (textAtom, topOffset, beingProcessedLine);
                    atom.TopOffset = topOffset;
                    atom.Content = beingProcessedLine;
                    TextLineViewModel textLine = new TextLineViewModel (atom);
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

                if ( (splited.Count > 0)   &&   textAtom.IsSplitable )
                {
                    beingProcessedLine = splited [0];
                    additionalLine = splited [1] + " " + additionalLine;
                    isSplitingOccured = true;
                }
                else
                {
                    TextualAtom atom = new TextualAtom (textAtom, topOffset, beingProcessedLine);
                    atom.TopOffset = topOffset;
                    atom.Content = beingProcessedLine;
                    TextLineViewModel textLine = new TextLineViewModel (atom);
                    TextLines.Add (textLine);
                    IsCorrect = false;
                    textLine.isBorderViolent = true;
                    break;
                }
            }
        }
    }


    private void OrderTextlinesByVertical ( List <TextualAtom> textualFields )
    {
        for ( int index = 0;   index < textualFields.Count;   index++ )
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


    private void SetUpImagesAndGeometryElements ( List <InsideImage> insideImages ) 
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


    internal void CheckCorrectness ( )
    {
        bool isCorrect = true;

        if ( FocusedLine == null ) 
        {
            return;
        }

        isCorrect = CheckBorderViolation (FocusedLine);

        foreach ( TextLineViewModel line in TextLines )
        {
            if ( line.Equals (FocusedLine) )
            {
                continue;
            }

            double topDifference = Math.Abs (FocusedLine.TopOffset - line.TopOffset);
            double maxFontsize = Math.Max (FocusedLine. FontSize, line.FontSize);
            bool isNotOverlayed = topDifference > maxFontsize;

            isCorrect = isCorrect && isNotOverlayed;

            if ( !isCorrect )
            {
                break;
            }
        }

        IsCorrect = isCorrect;
    }


    private void GatherIncorrectLines ( )
    {
        foreach ( TextLineViewModel line   in   TextLines )
        {
            bool isCorrect = CheckBorderViolation ( line );

            if ( ! isCorrect )
            {
                BorderViolentLines.Add ( line );
                line.isBorderViolent = true;
            }
        }

        foreach ( TextLineViewModel line   in   TextLines )
        {
            
        }
    }


    private bool CheckBorderViolation ( TextLineViewModel line )
    {
        Typeface face = new Typeface (new FontFamily ("arial"), FontStyle.Normal, FontWeight.Normal);
        FormattedText formatted = new FormattedText (line.Content, CultureInfo.CurrentCulture
                                                            , FlowDirection.LeftToRight, face, line.FontSize, null);

        double rest = BadgeWidth - ( line.LeftOffset + formatted.WidthIncludingTrailingWhitespace );
        bool notExceedToRight = ( rest > 0 );
        bool notExceedToLeft = ( line.LeftOffset > 0 );
        bool notExceedToTop = ( line.TopOffset > 0 );
        rest = BadgeHeight - ( line.TopOffset + line.FontSize );
        bool notExceedToBottom = ( rest > 0 );

        bool isCorrect = ( notExceedToRight && notExceedToLeft && notExceedToTop && notExceedToBottom );

        return isCorrect;
    }


    private bool CheckOverlayViolation ( )
    {
        for ( int index = 0;   index < TextLines. Count;   index++ )
        {
            TextLineViewModel overlaying = TextLines [ index ];


            for ( int ind = index;   ind < TextLines. Count;   ind++ )
            {
                TextLineViewModel underlaying = TextLines [ind];

                double topDifference = Math.Abs (overlaying.TopOffset - underlaying.TopOffset);
                double maxFontsize = Math.Max (overlaying.FontSize, underlaying.FontSize);
                bool isOverlaying = topDifference < maxFontsize;

                if (isOverlaying) 
                {
                    if ( ! overlaying.isOverLayViolent ) 
                    {
                        overlaying.isOverLayViolent = true;
                        OverlayViolentLines.Add (overlaying);
                    }

                    
                }


            }


        }








        Typeface face = new Typeface (new FontFamily ("arial"), FontStyle.Normal, FontWeight.Normal);
        FormattedText formatted = new FormattedText (line.Content, CultureInfo.CurrentCulture
                                                            , FlowDirection.LeftToRight, face, line.FontSize, null);

        





        bool isCorrect = false;

        return isCorrect;
    }
}