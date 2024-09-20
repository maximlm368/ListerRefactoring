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
using DataGateway;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Fluent;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Lister.ViewModels;


public class BadgeViewModel : ViewModelBase
{
    private static Dictionary<string, Bitmap> _pathToImage;

    private readonly string _semiProtectedTypeName = "Lister.ViewModels.BadgeEditorViewModel";
    private readonly double _interLineAddition = 1;
    private string _badLineColor;
    private IBadLineColorProvider _badLineColorProvider;

    internal int Id { get; private set; }
    internal double Scale { get; private set; }
    internal double LeftSpan { get; private set; }
    internal double TopSpan { get; private set; }
    internal double RightSpan { get; private set; }
    internal double BottomSpan { get; private set; }
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

    private double hC;
    internal double HeightCrutch
    {
        get { return hC; }
        set
        {
            this.RaiseAndSetIfChanged (ref hC, value, nameof (HeightCrutch));
        }
    }

    private double bHC;
    internal double BorderHeightCrutch
    {
        get { return bHC; }
        set
        {
            this.RaiseAndSetIfChanged (ref bHC, value, nameof (BorderHeightCrutch));
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

    private Thickness margin;
    internal Thickness Margin
    {
        get { return margin; }
        set
        {
            this.RaiseAndSetIfChanged (ref margin, value, nameof (Margin));
        }
    }

    private TextLineViewModel fL;
    internal TextLineViewModel FocusedLine 
    {
        get { return fL; }
        set 
        {
            fL = value;

            if ( (fL == null)   ||   (fL.Content == null) )
            {
                FocusedText = string.Empty;
            }
            else 
            {
                FocusedText = fL.Content;
            }
        } 
    }

    private string fT;
    internal string FocusedText
    {
        get { return fT; }
        set
        {
            this.RaiseAndSetIfChanged (ref fT, value, nameof (FocusedText));
        }
    }

    private List <TextLineViewModel> BorderViolentLines { get; set; }
    private List <TextLineViewModel> OverlayViolentLines { get; set; }
    private ObservableCollection <TextLineViewModel> tL;
    internal ObservableCollection <TextLineViewModel> TextLines
    {
        get { return tL; }
        private set
        {
            this.RaiseAndSetIfChanged (ref tL, value, nameof (TextLines));
        }
    }

    private ObservableCollection <ImageViewModel> iI;
    internal ObservableCollection <ImageViewModel> InsideImages
    {
        get { return iI; }
        private set
        {
            this.RaiseAndSetIfChanged (ref iI, value, nameof (InsideImages));
        }
    }

    private ObservableCollection <ImageViewModel> rs;
    internal ObservableCollection <ImageViewModel> InsideShapes
    {
        get { return rs; }
        private set
        {
            this.RaiseAndSetIfChanged (ref rs, value, nameof (InsideShapes));
        }
    }

    private double _borderThickness;
    private Avalonia.Thickness bT;
    internal Avalonia.Thickness BorderThickness
    {
        get { return bT; }
        private set
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


    public BadgeViewModel ( Badge badgeModel, int id )
    {
        Id = id;

        BadgeModel = badgeModel;
        BadgeLayout layout = badgeModel.BadgeLayout;

        LeftSpan = layout.LeftSpan;
        TopSpan = layout.TopSpan;
        RightSpan = layout.RightSpan;
        BottomSpan = layout.BottomSpan;

        BadgeWidth = layout.OutlineWidth;
        BorderWidth = BadgeWidth + 2;
        _widht = BadgeWidth;
        BadgeHeight = layout.OutlineHeight;
        BorderHeight = BadgeHeight + 2;
        _height = BadgeHeight;
        TextLines = new ObservableCollection<TextLineViewModel> ();
        BorderViolentLines = new List <TextLineViewModel> ( );
        OverlayViolentLines = new List <TextLineViewModel> ();
        InsideImages = new ObservableCollection <ImageViewModel> ();
        InsideShapes = new ObservableCollection <ImageViewModel> ();
        IsCorrect = true;
        IsChanged = false;
        _borderThickness = 1;
        Scale = 1;
        BorderThickness = new Avalonia.Thickness (_borderThickness);
        FocusedFontSize = string.Empty;

        if ( _badLineColorProvider == null ) 
        {
            _badLineColorProvider = App.services.GetRequiredService<IBadLineColorProvider> ();
        }

        _badLineColor = _badLineColorProvider.GetBadLineColor (badgeModel.BadgeLayout.TemplateName);

        List<TextualAtom> atoms = layout.TextualFields;
        OrderTextlinesByVertical (atoms);
       
        SetUpTextLines (atoms);
        GatherIncorrectLines ();

        List <InsideImage> images = layout.InsideImages;
        SetUpImagesAndGeometryElements (images);
    }


    private BadgeViewModel ( BadgeViewModel badge )
    {
        Id = badge.Id;

        BadgeModel = badge.BadgeModel;
        BadgeLayout layout = BadgeModel. BadgeLayout;

        LeftSpan = badge.LeftSpan;
        TopSpan = badge.TopSpan;
        RightSpan = badge.RightSpan;
        BottomSpan = badge.BottomSpan;

        BadgeWidth = badge.BadgeWidth;
        BorderWidth = BadgeWidth + 2;
        BadgeHeight = badge.BadgeHeight;
        BorderHeight = BadgeHeight + 2;
        TextLines = new ObservableCollection <TextLineViewModel> ();
        BorderViolentLines = new List <TextLineViewModel> ();
        OverlayViolentLines = new List <TextLineViewModel> ();
        InsideImages = new ObservableCollection <ImageViewModel> ();
        InsideShapes = new ObservableCollection <ImageViewModel> ();
        IsCorrect = badge.IsCorrect;
        Scale = badge.Scale;
        _borderThickness = 1;
        BorderThickness = new Avalonia.Thickness (_borderThickness);
        FocusedFontSize = string.Empty;
        _badLineColor = badge._badLineColor;

        foreach ( TextLineViewModel line   in   badge.TextLines )
        {
            TextLineViewModel clone = line.Clone ();
            TextLines.Add (clone);

            if ( line.isBorderViolent ) 
            {
                clone.isBorderViolent = true;
                BorderViolentLines.Add (clone);
            }

            if ( line.isOverLayViolent )
            {
                clone.isOverLayViolent = true;
                OverlayViolentLines.Add (clone);
            }
        }

        Scale = badge.Scale;

        //foreach ( TextLineViewModel line in badge.TextLines )
        //{
        //    TextLineViewModel original = line.GetDimensionalOriginal ();
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
            original.ZoomOut (Scale, false);
        }
        else if ( original.Scale < 1 )
        {
            original.ZoomOn (Scale, false);
        }

        //original.ZoomOut (Scale);

        return original;
    }


    internal BadgeViewModel Clone ()
    {
        BadgeViewModel clone = new BadgeViewModel (this);
        return clone;
    }


    internal void ResetPrototype ( BadgeViewModel prototype )
    {
        prototype.IsCorrect = IsCorrect;
        prototype.TextLines = TextLines;
    }


    internal TextLineViewModel ? GetCoincidence ( string focusedContent, int elementNumber )
    {
        ObservableCollection<TextLineViewModel> lines = TextLines;
        string lineContent = string.Empty;
        TextLineViewModel goalLine = null;
        int counter = 0;

        foreach ( TextLineViewModel line   in   lines )
        {
            lineContent = line.Content;

            if ( (lineContent == focusedContent)   &&   (elementNumber == counter) )
            {
                goalLine = line;
                break;
            }

            counter++;
        }

        return goalLine;
    }


    internal void SetFocusedLine ( string focusedContent, int elementNumber )
    {
        FocusedLine = GetCoincidence (focusedContent, elementNumber);

        if ( FocusedLine != null )
        {
            int visibleFontSize = ( int ) Math.Round (FocusedLine. FontSize / Scale);
            FocusedFontSize = visibleFontSize.ToString ();
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


    internal void ZoomOn ( double coefficient, bool isToStandard )
    {
        BadgeWidth *= coefficient;
        BadgeHeight *= coefficient;

        BorderWidth = BadgeWidth + 2;
        BorderHeight = BadgeHeight + 2;

        Scale *= coefficient;

        LeftSpan *= coefficient;
        TopSpan *= coefficient;
        RightSpan *= coefficient;
        BottomSpan *= coefficient;

        foreach ( TextLineViewModel line   in   TextLines ) 
        {
            line.ZoomOn (coefficient, isToStandard);
        }

        //foreach ( ImageViewModel image   in   InsideImages )
        //{
        //    image.ZoomOn (coefficient);
        //}

        //foreach ( ImageViewModel shape   in   InsideShapes )
        //{
        //    shape.ZoomOn (coefficient);
        //}
    }


    internal void ZoomOut ( double coefficient, bool isToStandard )
    {
        BadgeWidth /= coefficient;
        BadgeHeight /= coefficient;

        BorderWidth = BadgeWidth + 2;
        BorderHeight = BadgeHeight + 2;

        Scale /= coefficient;

        LeftSpan /= coefficient;
        TopSpan /= coefficient;
        RightSpan /= coefficient;
        BottomSpan /= coefficient;

        foreach ( TextLineViewModel line   in   TextLines )
        {
            line.ZoomOut (coefficient, isToStandard);
        }

        //foreach ( ImageViewModel image   in   InsideImages )
        //{
        //    image.ZoomOut (coefficient);
        //}

        //foreach ( ImageViewModel shape   in   InsideShapes )
        //{
        //    shape.ZoomOut (coefficient);
        //}
    }


    internal void SetCorrectScale ( double scale )
    {
        if ( scale != 1 )
        {
            ZoomOn (scale, false);
        }
    }


    internal void Split ( double scale )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        string content = ( string ) FocusedLine. Content;
        List<string> strings = content.SplitBySeparators (new List<char> () { ' ', '-' });
        double layoutWidth = BadgeWidth;
        List <TextLineViewModel> splitted = FocusedLine.SplitYourself (strings, scale, layoutWidth);
        ReplaceTextLine (FocusedLine, splitted);
        FocusedLine = null;
    }


    private void ReplaceTextLine ( TextLineViewModel replaceble, List <TextLineViewModel> replacings )
    {
        if ( replaceble.isBorderViolent )
        {
            BorderViolentLines.Remove (replaceble);
        }

        if ( replaceble.isOverLayViolent )
        {
            OverlayViolentLines.Remove (replaceble);
        }

        if ( ! replaceble.isBorderViolent   &&   ! replaceble.isOverLayViolent ) 
        {
            DeleteBadColor (replaceble);
        }

        TextLines.Remove (replaceble);

        foreach ( TextLineViewModel line   in   replacings )
        {
            CheckLineCorrectness ( line );
            TextLines.Add (line);
        }

        IsChanged = true;
    }


    //private void PreSetUpTextLines ( List <TextualAtom> textualFields )
    //{
    //    for ( int index = 0;   index < textualFields.Count;   index++ )
    //    {
    //        TextualAtom atom = textualFields [index];
    //        TextLineViewModel textLine = new TextLineViewModel (atom);
    //        TextLines.Add (textLine);
    //    }
    //}


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
            string beingProcessedLine = textAtom.Content.Trim();
            string additionalLine = string.Empty;

            while ( true )
            {
                double usefulTextBlockWidth;

                if ( ModernMainViewModel.MainViewIsWaiting )
                {
                    var result =
                    Dispatcher.UIThread.InvokeAsync<double>
                    (() => { return TextLineViewModel.CalculateWidth (beingProcessedLine, textAtom); });
                    usefulTextBlockWidth = result.Result;
                }
                else 
                {
                    usefulTextBlockWidth = TextLineViewModel.CalculateWidth (beingProcessedLine, textAtom);
                }

                bool lineIsOverflow = ( usefulTextBlockWidth >= lineLength );

                if ( ! lineIsOverflow ) 
                {
                    if ( isTimeToShiftNextLine )
                    {
                        summaryVerticalOffset += (textAtom.FontSize + _interLineAddition);
                        topOffset += ( textAtom.FontSize + _interLineAddition );
                    }

                    if ( isSplitingOccured )
                    {
                        isTimeToShiftNextLine = true;
                    }

                    TextualAtom atom = new TextualAtom (textAtom, topOffset, beingProcessedLine);
                    atom.TopOffset = topOffset;
                    atom.Content = beingProcessedLine;

                    TextLineViewModel textLine;

                    if ( ModernMainViewModel.MainViewIsWaiting )
                    {
                        var result2 =
                        Dispatcher.UIThread.InvokeAsync<TextLineViewModel>
                        (() => { return new TextLineViewModel (atom); });
                        textLine = result2.Result;
                    }
                    else 
                    {
                        textLine = new TextLineViewModel (atom);
                    }
                    
                    TextLines.Add (textLine);

                    if ( additionalLine != string.Empty ) 
                    {
                        beingProcessedLine = additionalLine.Trim();
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
                    beingProcessedLine = splited [0].Trim();
                    additionalLine = splited [1] + " " + additionalLine;
                    isSplitingOccured = true;
                }
                else
                {
                    TextualAtom atom = new TextualAtom (textAtom, topOffset, beingProcessedLine);
                    atom.TopOffset = topOffset;
                    atom.Content = beingProcessedLine;

                    TextLineViewModel textLine;

                    if ( ModernMainViewModel.MainViewIsWaiting )
                    {
                        var result2 =
                        Dispatcher.UIThread.InvokeAsync<TextLineViewModel>
                        (() => { return new TextLineViewModel (atom); });
                        textLine = result2.Result;
                    }
                    else 
                    {
                        textLine = new TextLineViewModel (atom);
                    }

                    
                    TextLines.Add (textLine);
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


    internal void CheckFocusedLineCorrectness ( )
    {
        if ( FocusedLine == null ) 
        {
            return;
        }

        CheckLineCorrectness (FocusedLine);

        bool borderViolentsExist = (BorderViolentLines. Count > 0);
        bool overlayingExist = (OverlayViolentLines. Count > 0);

        if ( OverlayViolentLines.Count > 0 ) 
        {
            TextLineViewModel line = OverlayViolentLines [0];
        }

        IsCorrect = ! (borderViolentsExist   ||   overlayingExist);
    }


    internal void CheckCorrectnessAfterCancelation ()
    {
        IsCorrect = false;
        IsChanged = false;
    }


    private void CheckLineCorrectness ( TextLineViewModel checkable )
    {
        bool isBorderViolent = CheckInsideSpansViolation (checkable);

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
                SetBadColor ( checkable );
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
                SetBadColor ( checkable );
            }
        }

        if ( ! checkable.isBorderViolent   &&   ! checkable.isOverLayViolent )
        {
            DeleteBadColor (checkable);
        }
    }


    private void GatherIncorrectLines ( )
    {
        foreach ( TextLineViewModel line   in   TextLines )
        {
            bool isViolent = CheckInsideSpansViolation ( line );

            if ( isViolent )
            {
                BorderViolentLines.Add ( line );
                line.isBorderViolent = true;
                SetBadColor ( line );
                IsCorrect = false;
            }
        }

        CheckOverlayViolation ();
    }


    //private void GatherIncorrectLinesqq ()
    //{
    //    foreach ( TextLineViewModel line in TextLines )
    //    {
    //        bool isViolent = CheckInsideSpansViolation (line);

    //        if ( isViolent )
    //        {
    //            BorderViolentLines.Add (line);
    //            line.isBorderViolent = true;

    //            Task task = new Task
    //                (
    //                   () =>
    //                   {
    //                       SetBadColor (line);
    //                   }

    //                );

    //            task.Start (TemplateChoosingViewModel.TaskScheduler);
    //            task.Wait ();

    //            IsCorrect = false;
    //        }
    //    }

    //    CheckOverlayViolation ();
    //}


    private void SetBadColor ( TextLineViewModel setable )
    {
        if ( _badLineColor == null )
        {
            setable.Background = null;
        }
        else 
        {
            List<int> rgb = _badLineColor.TranslateIntoIntList ();

            if ( rgb.Count == 3 )
            {
                byte r = ( byte ) rgb [0];
                byte g = ( byte ) rgb [1];
                byte b = ( byte ) rgb [2];

                IBrush brush;

                if ( ModernMainViewModel.MainViewIsWaiting )
                {
                    var result =
                    Dispatcher.UIThread.InvokeAsync<IBrush>
                    (() => { return new SolidColorBrush (new Color (255, r, g, b)); });
                    brush = result.Result;
                }
                else 
                {
                    brush = new SolidColorBrush (new Color (255, r, g, b));
                }

                setable.Background = brush;
            }
            else 
            {
                setable.Background = null;
            }
        }
    }


    private void DeleteBadColor ( TextLineViewModel setable )
    {
        setable.Background = null;
    }


    private bool CheckInsideSpansViolation ( TextLineViewModel line )
    {
        double rest = BadgeWidth - ( line.LeftOffset + line.UsefullWidth );
        bool notExceedToRight = ( rest >= RightSpan );
        bool notExceedToLeft = ( line.LeftOffset >= LeftSpan );
        bool notExceedToTop = ( line.TopOffset >= TopSpan );
        rest = BadgeHeight - ( line.TopOffset + line.FontSize );
        bool notExceedToBottom = ( rest >= BottomSpan );

        bool isViolent = ! ( notExceedToRight   &&   notExceedToLeft   &&   notExceedToTop   &&   notExceedToBottom );

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
                    SetBadColor ( overlaying );
                }

                break;
            }
        }

        return isViolent;
    }


    #region FontSizeChange

    internal void IncreaseFontSize ( double changeSize )
    {
        ChangeFontSize (changeSize, false);
    }


    internal void ReduceFontSize ( double changeSize )
    {
        ChangeFontSize (changeSize, true);
    }


    private void ChangeFontSize ( double changeSize, bool toReduce )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        if ( toReduce )
        {
            FocusedLine.Reduce (changeSize);
        }
        else
        {
            FocusedLine.Increase (changeSize);
        }

        IsChanged = true;
        int visibleFontSize = (int) Math.Round (FocusedLine. FontSize / Scale);
        FocusedFontSize = visibleFontSize.ToString ();
        CheckFocusedLineCorrectness ();
    }
    #endregion

    #region Moving

    internal void MoveCaptured ( Avalonia.Point delta )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        FocusedLine. TopOffset -= delta.Y;
        FocusedLine. LeftOffset -= delta.X;
        PreventHiding (FocusedLine);
        IsChanged = true;
    }


    internal void ToSide ( string direction, double shift )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        if ( direction == "Left" )
        {
            FocusedLine. LeftOffset -= shift;
        }

        if ( direction == "Right" )
        {
            FocusedLine. LeftOffset += shift;
        }

        if ( direction == "Up" )
        {
            FocusedLine. TopOffset -= shift;
        }

        if ( direction == "Down" )
        {
            FocusedLine. TopOffset += shift;
        }

        IsChanged = true;
        PreventHiding (FocusedLine);
        CheckFocusedLineCorrectness ();
    }


    private void PreventHiding ( TextLineViewModel preventable )
    {
        bool isHidedBeyondRight = ( preventable.LeftOffset > (BadgeWidth - RightSpan) );

        if ( isHidedBeyondRight ) 
        {
            preventable.LeftOffset = (BadgeWidth - RightSpan);
        }

        bool isHidedBeyondLeft = ( preventable.LeftOffset < ( preventable.UsefullWidth - LeftSpan ) * ( -1 ) );

        if ( isHidedBeyondLeft )
        {
            preventable.LeftOffset = ( preventable.UsefullWidth - LeftSpan ) * ( -1 );
        }

        bool isHidedBeyondBottom = ( preventable.TopOffset > ( BadgeHeight - BottomSpan ) );

        if ( isHidedBeyondBottom )
        {
            preventable.TopOffset = (BadgeHeight - BottomSpan);
        }

        bool isHidedBeyondTop = ( preventable.TopOffset < ( preventable.Height / 4 ) * ( -1 ) );

        if ( isHidedBeyondTop )
        {
            preventable.TopOffset = ( preventable.Height / 4 ) * ( -1 );
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
        CheckFocusedLineCorrectness ();
    }


    internal void Right ( double shift )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        FocusedLine. LeftOffset += shift;
        IsChanged = true;
        CheckFocusedLineCorrectness ();
    }


    internal void Up ( double shift )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        FocusedLine. TopOffset -= shift;
        IsChanged = true;
        CheckFocusedLineCorrectness ();
    }


    internal void Down ( double shift )
    {
        if ( FocusedLine == null )
        {
            return;
        }

        FocusedLine. TopOffset += shift;
        IsChanged = true;
        CheckFocusedLineCorrectness ();
    }

    #endregion


    internal void ResetFocusedText ( string newText )
    {
        if ( FocusedLine == null ) 
        {
            return;
        }

        if ( FocusedLine. Content != newText ) 
        {
            IsChanged = true;
        }

        FocusedLine. Content = newText;
        PreventHiding (FocusedLine);
        CheckFocusedLineCorrectness ();
    }
}