using Lister.Core.DocumentProcessor.Abstractions;
using Lister.Core.ExtentionsAndAuxiliary;

namespace Lister.Core.Models.Badge;

/// <summary>
/// Represents component of badge that contains only text.
/// </summary>
public class TextLine : LayoutComponentBase
{
    internal static ITextWidthMeasurer Measurer { get; set; }

    private static readonly double _divider = 8;
    private static readonly double _maxFontSizeLimit = 30;
    private static readonly double _minFontSizeLimit = 6;

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
        internal set
        {
            _content = value;
            ContentIsSet = true;
        }
    }
    public double UsefullWidth { get; private set; }
    public List<string> IncludedLines { get; private set; }
    public bool IsSplitable { get; private set; }
    public bool ContentIsSet { get; private set; }
    public bool isNeeded;

    private bool _isBorderViolent = false;
    public bool IsPaddingViolent 
    {
        get { return _isBorderViolent; }
        internal set 
        {
            _isBorderViolent = value;
            TextChanged?.Invoke ( this );
        } 
    }

    private bool _isOverLayViolent = false;
    public bool IsOverLayViolent 
    {
        get { return _isOverLayViolent; }
        internal set 
        {
            _isOverLayViolent = value;
            TextChanged?.Invoke ( this );
        }
    }

    public delegate void TextChangedHandler (TextLine source);
    public event TextChangedHandler ? TextChanged;


    public TextLine ( string name, double width, double height, double topOffset, double leftOffset, string alignment
                       , double fontSize, string fontName, string foregroundHexStr, string fontWeight
                       , List<string>? includedLines, bool isSplitable, int numberToLocate )
    {
        Content = "";
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
        IncludedLines = includedLines ?? new List<string> ();
        IsSplitable = isSplitable;
        NumberToLocate = numberToLocate;
        isNeeded = true;
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
        IncludedLines = source.IncludedLines ?? new List<string> ();
        IsSplitable = source.IsSplitable;
        NumberToLocate = source.NumberToLocate;
        isNeeded = true;
        IsPaddingViolent = source.IsPaddingViolent;
        IsOverLayViolent = source.IsOverLayViolent;

        if ( isJustCopying )
        {
            UsefullWidth = source.UsefullWidth;
            Padding = source.Padding;
        }
        else 
        {
            UsefullWidth = Measurer.Measure ( Content, FontWeight, FontSize, FontName );
            SetAlignment ();
            Width = UsefullWidth;
            Padding = GetPadding ();
        }
    }


    internal TextLine CloneAsDescription ()
    {
        TextLine clone = new TextLine ( Name, Width, Height, TopOffset, LeftOffset, Alignment, FontSize
                                             , FontName, ForegroundHexStr, FontWeight, IncludedLines, IsSplitable
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
            Content = Content.TrimStart ( symbol );
            Content = Content.TrimEnd ( symbol );
        }
    }


    internal void IncreaseFontSize ( )
    {
        double oldFontSize = FontSize;
        double newFontSize = oldFontSize + 1;

        if ( newFontSize > _maxFontSizeLimit ) return;

        FontSize += 1;
        UsefullWidth = Measurer.Measure ( Content, FontWeight, FontSize, FontName );
        double proportion = FontSize / oldFontSize;
        Height *= proportion;
        Padding = GetPadding ();
        TextChanged?.Invoke (this);
    }


    internal void ReduceFontSize ( )
    {
        double fontSize = FontSize;

        if ( ( fontSize - 1 ) < _minFontSizeLimit ) return;

        double insideLeftRest = UsefullWidth - Math.Abs ( LeftOffset );
        double insideTopRest = Height - Math.Abs ( TopOffset );
        double oldWidth = UsefullWidth;
        double oldFontSize = FontSize;
        FontSize -= 1;
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

        TextChanged?.Invoke (this);
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


    public void ResetContent ( string newText )
    {
        Content = newText;
        UsefullWidth = Measurer.Measure ( Content, FontWeight, FontSize, FontName );
        TextChanged?.Invoke (this);
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