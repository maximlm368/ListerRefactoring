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

class BadgeViewModel : ViewModelBase
{
    private const double coefficient = 1.1;
    internal Badgee BadgeModel { get; private set; }
    internal Bitmap ImageBitmap { get; private set; }

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
    internal ObservableCollection <ImageViewModel> InsideRectangles
    {
        get { return rs; }
        set
        {
            this.RaiseAndSetIfChanged (ref rs, value, nameof (InsideRectangles));
        }
    }

    private double borderThickness;
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


    internal BadgeViewModel ( Badgee badgeModel )
    {
        BadgeModel = badgeModel;
        BadgeWidth = badgeModel.BadgeLayout.OutlineSize.Width;
        BadgeHeight = badgeModel.BadgeLayout.OutlineSize.Height;
        TextLines = new ObservableCollection<TextLineViewModel> ();
        InsideImages = new ObservableCollection<ImageViewModel> ();
        InsideRectangles = new ObservableCollection<ImageViewModel> ();
        IsCorrect = true;
        borderThickness = 1;
        BorderThickness = new Avalonia.Thickness (borderThickness);

        List<TextualAtom> atoms = badgeModel.BadgeLayout. TextualFields;
        OrderTextlinesByVertical (atoms);
        SetUpTextLines (atoms);


    }


    internal void ShowBackgroundImage ()
    {
        string path = BadgeModel. BackgroundImagePath;
        Uri uri = new Uri (path);
        this.ImageBitmap = ImageHelper.LoadFromResource (uri);
    }


    internal void HideBackgroundImage ()
    {
        this.ImageBitmap = null;
    }


    internal void ZoomOn ( double coefficient )
    {
        BadgeWidth *= coefficient;
        BadgeHeight *= coefficient;


        //TextAreaLeftShift *= coefficient;
        //TextAreaTopShift *= coefficient;
        //TextAreaWidth *= coefficient;
        //TextAreaHeight *= coefficient;
        //FirstLevelFontSize *= coefficient;
        //SecondLevelFontSize *= coefficient;
        //ThirdLevelFontSize *= coefficient;
        //FirstLevelTBHeight *= coefficient;
        //SecondLevelTBHeight *= coefficient;
        //ThirdLevelTBHeight *= coefficient;
        //DepartmentTopPadding *= coefficient;
        //PostTopPadding *= coefficient;

        borderThickness *= coefficient;
        BorderThickness = new Avalonia.Thickness (borderThickness);
    }


    internal void ZoomOut ( double coefficient )
    {
        //BadgeWidth /= coefficient;
        //BadgeHeight /= coefficient;
        //TextAreaLeftShift /= coefficient;
        //TextAreaTopShift /= coefficient;
        //TextAreaWidth /= coefficient;
        //TextAreaHeight /= coefficient;
        //FirstLevelFontSize /= coefficient;
        //SecondLevelFontSize /= coefficient;
        //ThirdLevelFontSize /= coefficient;
        //FirstLevelTBHeight /= coefficient;
        //SecondLevelTBHeight /= coefficient;
        //ThirdLevelTBHeight /= coefficient;
        //DepartmentTopPadding /= coefficient;
        //PostTopPadding /= coefficient;

        borderThickness /= coefficient;
        BorderThickness = new Avalonia.Thickness (borderThickness);
    }


    internal BadgeViewModel Clone ()
    {
        BadgeViewModel clone = new BadgeViewModel (this.BadgeModel);
        return clone;
    }


    internal void Zoom ( double coefficient )
    {
        if ( coefficient != 1 )
        {
            ZoomOn (coefficient);
        }
    }


    private void SetUpTextLines ( List<TextualAtom> textualFields )
    {
        double summaryVerticalOffset = 0;

        for ( int index = 0;   index < textualFields.Count;   index++ ) 
        {
            TextualAtom text = textualFields [index];
            double fontSize = text.FontSize;
            double lineLength = text.Width;
            double topOffset = text.TopOffset;
            string beingProcessedLine = text.Content;
            string additionalLine = string.Empty;

            while ( true )
            {
                FormattedText formatted = new FormattedText (beingProcessedLine, CultureInfo.CurrentCulture
                                                           , FlowDirection.LeftToRight, Typeface.Default, fontSize, null);
                
                double usefulTextBlockWidth = formatted.Width * coefficient;
                bool lineIsOverflow = ( usefulTextBlockWidth >= lineLength );

                if ( ! lineIsOverflow ) 
                {
                    TextLineViewModel textLine = new TextLineViewModel (text.Width, text.Height, topOffset,
                                                                        text.LeftOffset, text.Alignment, text.FontSize,
                                                                        text.FontFamily, beingProcessedLine,
                                                                        text.IsShiftableBelow);
                    TextLines.Add(textLine);
                    summaryVerticalOffset += text.Height;
                    topOffset += text.Height;

                    if ( text.IsShiftableBelow ) 
                    {
                        topOffset += summaryVerticalOffset;
                    }

                    if ( additionalLine != string.Empty ) 
                    {
                        beingProcessedLine = additionalLine;
                        continue;
                    }
                    else 
                    {
                        break;
                    }
                }

                List<string> splited = beingProcessedLine.SeparateIntoMainAndTail ();

                if ( splited.Count > 0 )
                {
                    beingProcessedLine = splited [0];
                    additionalLine = splited [1] + " " + additionalLine;
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
        for ( int index = 0;   index < textualFields. Count - 1;   index++ )
        {
            TextualAtom line = textualFields [index];
            TextualAtom next = textualFields [index + 1];

            if ( line.TopOffset > next.TopOffset ) 
            {
                TextualAtom current = textualFields [index];
                textualFields [index] = next;
                textualFields [index + 1] = current;
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
                InsideRectangles.Add (imageVM);
            }
        }
    }
}