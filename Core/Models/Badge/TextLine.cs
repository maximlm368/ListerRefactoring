using Core.DocumentBuilder;
using ExtentionsAndAuxiliary;

namespace Core.Models.Badge;

public class TextLine : LayoutComponent
{
    internal static ITextWidthMeasurer Measurer { get; set; }

    private static readonly double _divider = 8;

    public string Name { get; private set; }
    public int NumberToLocate { get; private set; }
    public string Alignment { get; private set; }
    public double FontSize { get; private set; }
    public string FontName { get; private set; }
    public string ForegroundHexStr { get; private set; }
    public string FontWeight { get; private set; }
    public Thickness Padding { get; private set; }
    private string _content;
    public string Content
    {
        get
        {
            return _content;
        }
        set
        {
            if ( ! string.IsNullOrWhiteSpace ( value ) )
            {
                _content = value;
                ContentIsSet = true;
            }
        }
    }
    public double UsefullWidth { get; private set; }
    public List<string> IncludedAtoms { get; private set; }
    public bool IsSplitable { get; private set; }
    public bool ContentIsSet { get; private set; }
    public bool isNeeded;

    public bool IsBorderViolent { get; internal set; } = false;
    public bool IsOverLayViolent { get; internal set; } = false;


    public TextLine ( string name, double width, double height, double topOffset, double leftOffset, string alignment
                       , double fontSize, string fontName, string foregroundHexStr
                       , string fontWeight, List<string>? includedAtoms, bool isSplitable, int numberToLocate )
    {
        _content = "";
        ContentIsSet = false;
        Name = name;
        Width = width;
        Height = height;
        TopOffset = topOffset;
        LeftOffset = leftOffset;
        Alignment = alignment;
        FontSize = fontSize;
        FontName = fontName;

        ForegroundHexStr = foregroundHexStr;

        FontWeight = fontWeight;
        IncludedAtoms = includedAtoms ?? new List<string> ();
        IsSplitable = isSplitable;
        NumberToLocate = numberToLocate;
        isNeeded = true;

        //SetAlignment ( );
    }


    internal TextLine ( TextLine source, string content, bool isJustCopying )
    {
        Content = content;
        Name = source.Name;
        Width = source.Width;
        Height = source.Height;
        TopOffset = source.TopOffset;
        LeftOffset = source.LeftOffset;
        Alignment = source.Alignment;
        FontSize = source.FontSize;
        FontName = source.FontName;
        ForegroundHexStr = source.ForegroundHexStr;
        FontWeight = source.FontWeight;
        IncludedAtoms = source.IncludedAtoms ?? new List<string> ();
        IsSplitable = source.IsSplitable;
        NumberToLocate = source.NumberToLocate;
        isNeeded = true;

        if ( isJustCopying )
        {
            UsefullWidth = source.UsefullWidth;
            Padding = source.Padding;
        }
        else 
        {
            UsefullWidth = Measurer.Measure ( Content, FontWeight, FontSize, FontName );
            SetAlignment ();
            Padding = GetPadding ();
        }
    }


    internal TextLine CloneAsDescription ()
    {
        TextLine clone = new TextLine ( Name, Width, Height, TopOffset, LeftOffset, Alignment, FontSize
                                             , FontName, ForegroundHexStr, FontWeight, IncludedAtoms, IsSplitable
                                             , NumberToLocate );
        return clone;
    }


    internal TextLine Clone ()
    {
        TextLine clone = new TextLine ( this, Content, true );
        return clone;
    }


    internal void TrimUnneededEdgeChar ( List<char> unNeeded )
    {
        bool charsAndContentExist = unNeeded != null 
                                    && 
                                    unNeeded.Count > 0;

        foreach ( char symbol   in   unNeeded )
        {
            _content = _content.TrimStart ( symbol );
            _content = _content.TrimEnd ( symbol );
        }
    }


    public void IncreaseFontSize ( double additable )
    {
        double oldFontSize = FontSize;
        FontSize += additable;

        UsefullWidth = Measurer.Measure ( Content, FontWeight, FontSize, FontName );
        double proportion = FontSize / oldFontSize;
        Height *= proportion;
        Padding = GetPadding ();
    }


    public void ReduceFontSize ( double subtractable )
    {
        double insideLeftRest = UsefullWidth - Math.Abs ( LeftOffset );
        double insideTopRest = Height - Math.Abs ( TopOffset );

        double oldWidth = UsefullWidth;
        double oldFontSize = FontSize;

        FontSize -= subtractable;

        UsefullWidth = Measurer.Measure ( Content, FontWeight, FontSize, FontName );
        double proportion = oldFontSize / FontSize;
        Height /= proportion;
        Padding = GetPadding ();

        double newInsideLeftRest = UsefullWidth - Math.Abs ( LeftOffset );

        if ( ( LeftOffset < 0 ) && ( newInsideLeftRest < insideLeftRest ) )
        {
            LeftOffset += ( insideLeftRest - newInsideLeftRest );
        }

        double newInsideTopRest = Height - Math.Abs ( TopOffset );

        if ( ( TopOffset < 0 ) && ( newInsideTopRest < insideTopRest ) )
        {
            TopOffset += ( insideTopRest - newInsideTopRest );
        }
    }


    public List <TextLine> SplitYourself ( double layoutWidth )
    {
        List<string> pieces = Content.SplitBySeparators ( [' ', '-'], ['-'] );

        List<TextLine> result = new List<TextLine> ();
        double splitableLineLeftOffset = LeftOffset;
        double offsetInQueue = LeftOffset;

        foreach ( string content   in   pieces )
        {
            TextLine newLine = new TextLine ( this, content, false );
            newLine.LeftOffset = offsetInQueue;

            if ( newLine.LeftOffset >= layoutWidth - 10 )
            {
                newLine.LeftOffset = splitableLineLeftOffset;
            }

            newLine.TopOffset = TopOffset;
            offsetInQueue += newLine.UsefullWidth + 1;
            result.Add ( newLine );
        }

        return result;
    }


    internal void ResetContent ( string newText )
    {
        Content = newText;
        CheckFocusedLineCorrectness ();
    }


    private void SetAlignment ( )
    {
        if ( Width <= UsefullWidth )
        {
            return;
        }

        if ( Alignment == "Right" )
        {
            LeftOffset += ( Width - Math.Ceiling ( UsefullWidth ) );
        }
        else if ( Alignment == "Center" )
        {
            LeftOffset += ( Width - UsefullWidth ) / 2;
        }
    }


    private Thickness GetPadding ()
    {
        return new Thickness ( 0, -FontSize / _divider );
    }
}