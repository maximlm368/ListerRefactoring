using Avalonia;
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
using System;

namespace Lister.ViewModels;

public class BadgeViewModel : ViewModelBase
{
    private static Dictionary<string, Bitmap> _pathToImage;
    private static double _rightSpan = 5;

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

    private string fS;
    internal string FocusedFontSize
    {
        get { return fS; }
        set
        {
            this.RaiseAndSetIfChanged (ref fS, value, nameof (FocusedFontSize));
        }
    }

    internal bool IsCorrect { get; private set; }

    private bool iC;
    internal bool IsChanged
    {
        get { return iC; }
        private set
        {
            this.RaiseAndSetIfChanged (ref iC, value, nameof (IsChanged));
        }
    }


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
        IsChanged = false;
        _borderThickness = 1;
        Scale = 1;
        BorderThickness = new Avalonia.Thickness (_borderThickness);
        FocusedFontSize = string.Empty;

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
        BadgeWidth = badge.BadgeWidth;
        BorderWidth = BadgeWidth + 2;
        BadgeHeight = badge.BadgeHeight;
        BorderHeight = BadgeHeight + 2;
        TextLines = new ObservableCollection<TextLineViewModel> ();
        BorderViolentLines = new List<TextLineViewModel> ();
        OverlayViolentLines = new List<TextLineViewModel> ();
        InsideImages = new ObservableCollection<ImageViewModel> ();
        InsideShapes = new ObservableCollection<ImageViewModel> ();
        IsCorrect = badge.IsCorrect;
        Scale = badge.Scale;
        _borderThickness = 1;
        BorderThickness = new Avalonia.Thickness (_borderThickness);
        FocusedFontSize = string.Empty;

        foreach ( TextLineViewModel line in badge.TextLines )
        {
            TextLineViewModel clone = line.Clone ();
            TextLines.Add (clone);
        }

        Scale = badge.Scale;


        //foreach ( TextLineViewModel line in badge.TextLines )
        //{
        //    //TextLineViewModel original = line.GetDimensionalOriginal ();
        //    TextLines.Add (line);
        //}


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
        BadgeViewModel original = new BadgeViewModel (this);

        if ( original.Scale > 1 )
        {
            original.ZoomOut (Scale);
        }
        else if ( original.Scale < 1 ) 
        {
            original.ZoomOn (Scale);
        }
        
        return original;
    }


    internal BadgeViewModel Clone ()
    {
        BadgeViewModel clone = new BadgeViewModel (this);
        return clone;
    }


    internal TextLineViewModel ? GetCoincidence ( string focusedContent )
    {
        ObservableCollection<TextLineViewModel> lines = TextLines;
        string lineContent = string.Empty;
        TextLineViewModel goalLine = null;

        foreach ( TextLineViewModel line   in   lines )
        {
            lineContent = line.Content;

            if ( lineContent == focusedContent )
            {
                goalLine = line;
                break;
            }
        }

        return goalLine;
    }


    internal void SetFocusedLine ( string focusedContent )
    {
        FocusedLine = GetCoincidence (focusedContent);

        if ( FocusedLine != null )
        {
            FocusedFontSize = FocusedLine. FontSize.ToString ();
        }
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


    internal void SetCorrectScale ( double scale )
    {
        if ( scale != 1 )
        {
            ZoomOn (scale);
        }
    }


    internal void Split ( double scale )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        string content = ( string ) FocusedLine. Content;
        List<string> strings = content.SplitBySeparators ();
        double layoutWidth = BadgeWidth;
        List <TextLineViewModel> splitted = FocusedLine.SplitYourself (strings, scale, layoutWidth);
        ReplaceTextLine (FocusedLine, splitted);
    }


    private void ReplaceTextLine ( TextLineViewModel replaceble, List <TextLineViewModel> replacing )
    {
        if ( replaceble.isBorderViolent )
        {
            BorderViolentLines.Remove (replaceble);
        }

        if ( replaceble.isOverLayViolent )
        {
            OverlayViolentLines.Remove (replaceble);
        }

        TextLines.Remove (replaceble);

        foreach ( TextLineViewModel line   in   replacing )
        {
            CheckLineCorrectness ( line );
            TextLines.Add (line);
        }

        IsChanged = true;
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
                double usefulTextBlockWidth = formatted.WidthIncludingTrailingWhitespace + 4;
                bool lineIsOverflow = ( usefulTextBlockWidth > lineLength );

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
        if ( FocusedLine == null ) 
        {
            return;
        }

        CheckLineCorrectness (FocusedLine);

        bool borderViolentsExist = (BorderViolentLines. Count > 0);
        bool overlayingExist = (OverlayViolentLines. Count > 0);

        IsCorrect = ! (borderViolentsExist   ||   overlayingExist);
    }


    internal void CheckCorrectnessAfterCancelation ()
    {
        foreach ( TextLineViewModel line   in   TextLines )
        {
            CheckLineCorrectness (line);
        }

        bool borderViolentsExist = ( BorderViolentLines.Count > 0 );
        bool overlayingExist = ( OverlayViolentLines.Count > 0 );

        IsCorrect = !( borderViolentsExist || overlayingExist );
    }


    private void CheckLineCorrectness ( TextLineViewModel checkable )
    {
        bool isBorderViolent = CheckBorderViolation (checkable);

        if ( ! isBorderViolent )
        {
            if ( checkable.isBorderViolent )
            {
                BorderViolentLines.Remove (checkable);
                checkable.isBorderViolent = false;
            }
        }
        else
        {
            if ( ! checkable.isBorderViolent )
            {
                BorderViolentLines.Add (checkable);
                checkable.isBorderViolent = true;
            }
        }

        bool isOverlayViolent = CheckSingleOverlayViolation (0, checkable);

        if ( ! isOverlayViolent )
        {
            if ( checkable.isOverLayViolent )
            {
                OverlayViolentLines.Remove (checkable);
                checkable.isOverLayViolent = false;
            }
        }
        else
        {
            if ( ! checkable.isOverLayViolent )
            {
                OverlayViolentLines.Add (checkable);
                checkable.isOverLayViolent = true;
            }
        }
    }


    private void GatherIncorrectLines ( )
    {
        foreach ( TextLineViewModel line   in   TextLines )
        {
            bool isViolent = CheckBorderViolation ( line );

            if ( isViolent )
            {
                BorderViolentLines.Add ( line );
                line.isBorderViolent = true;
            }
        }

        CheckOverlayViolation ();
    }


    private bool CheckBorderViolation ( TextLineViewModel line )
    {
        double rest = BadgeWidth - ( line.LeftOffset + line.UsefullWidth );
        bool notExceedToRight = ( rest > 0 );
        bool notExceedToLeft = ( line.LeftOffset > 0 );
        bool notExceedToTop = ( line.TopOffset > 0 );
        rest = BadgeHeight - ( line.TopOffset + line.FontSize );
        bool notExceedToBottom = ( rest > 0 );

        bool isViolent = ! ( notExceedToRight && notExceedToLeft && notExceedToTop && notExceedToBottom );

        return isViolent;
    }


    private void CheckOverlayViolation ( )
    {
        for ( int index = 0;   index < TextLines. Count;   index++ )
        {
            TextLineViewModel overlaying = TextLines [ index ];
            CheckSingleOverlayViolation (index, overlaying );
        }
    }


    private bool CheckSingleOverlayViolation ( int scratchInLines, TextLineViewModel overlaying )
    {
        bool isViolent = false;

        for ( int ind = scratchInLines;   ind < TextLines. Count;   ind++ )
        {
            TextLineViewModel underlaying = TextLines [ind];

            if ( underlaying.Equals (overlaying) ) continue; 
            
            double verticalDifference = Math.Abs (overlaying.TopOffset - underlaying.TopOffset);
            double topOffsetOfAbove = Math.Min (overlaying.TopOffset, underlaying.TopOffset);
            TextLineViewModel above = overlaying;

            if ( topOffsetOfAbove == underlaying.TopOffset )
            {
                above = underlaying;
            }

            isViolent = ( verticalDifference < above.FontSize );

            double horizontalDifference = Math.Abs (overlaying.LeftOffset - underlaying.LeftOffset);
            double leftOffsetOfLeftist = Math.Min (overlaying.LeftOffset, underlaying.LeftOffset);
            TextLineViewModel leftist = overlaying;

            if ( leftOffsetOfLeftist == underlaying.LeftOffset )
            {
                leftist = underlaying;
            }

            isViolent = isViolent   &&   ( horizontalDifference < leftist.UsefullWidth );

            if ( isViolent )
            {
                if ( ! overlaying.isOverLayViolent )
                {
                    OverlayViolentLines.Add (overlaying);
                    overlaying.isOverLayViolent = true;
                }

                break;
            }
        }

        return isViolent;
    }


    #region FontSizeChange

    internal void IncreaseFontSize ( double increasing )
    {
        ChangeFontSize (increasing, false);
    }


    internal void ReduceFontSize ( double increasing )
    {
        ChangeFontSize (increasing, true);
    }


    private void ChangeFontSize ( double increasing, bool toReduce )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        if ( toReduce )
        {
            FocusedLine.Reduce (increasing);
        }
        else
        {
            FocusedLine.Increase (increasing);
        }

        IsChanged = true;
        FocusedFontSize = FocusedLine. FontSize.ToString ();
        CheckCorrectness ();
    }
    #endregion

    #region Moving

    internal void MoveCaptured ( string capturedContent, Point delta )
    {
        ObservableCollection <TextLineViewModel> lines = TextLines;
        string lineContent = string.Empty;
        TextLineViewModel goalLine = null;

        foreach ( TextLineViewModel line in lines )
        {
            lineContent = line.Content;

            if ( lineContent == capturedContent )
            {
                goalLine = line;
                break;
            }
        }

        if ( goalLine != null )
        {
            goalLine.TopOffset -= delta.Y;
            goalLine.LeftOffset -= delta.X;
            IsChanged = true;
        }
    }


    internal void ToSide ( string focusedContent, string direction, double shift )
    {
        ObservableCollection<TextLineViewModel> lines = TextLines;
        string lineContent = string.Empty;
        TextLineViewModel goalLine = null;

        foreach ( TextLineViewModel line in lines )
        {
            lineContent = line.Content;

            if ( lineContent == focusedContent )
            {
                goalLine = line;
                break;
            }
        }

        if ( goalLine != null )
        {
            if ( direction == "Left" )
            {
                goalLine.LeftOffset -= shift;
            }

            if ( direction == "Right" )
            {
                goalLine.LeftOffset += shift;
            }

            if ( direction == "Up" )
            {
                goalLine.TopOffset -= shift;
            }

            if ( direction == "Down" )
            {
                goalLine.TopOffset += shift;
            }

            IsChanged = true;
            CheckCorrectness ();
        }
    }


    internal void Left ( double shift )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        FocusedLine. LeftOffset -= shift;
        IsChanged = true;
        CheckCorrectness ();
    }


    internal void Right ( double shift )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        FocusedLine. LeftOffset += shift;
        IsChanged = true;
        CheckCorrectness ();
    }


    internal void Up ( double shift )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        FocusedLine. TopOffset -= shift;
        IsChanged = true;
        CheckCorrectness ();
    }


    internal void Down ( double shift )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        FocusedLine. TopOffset += shift;
        IsChanged = true;
        CheckCorrectness ();
    }

    #endregion
}