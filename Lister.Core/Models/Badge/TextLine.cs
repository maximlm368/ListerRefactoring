using Lister.Core.Document.AbstractServices;
using Lister.Core.Extentions;

namespace Lister.Core.Models.Badge;

/// <summary>
/// Represents component of badge that contains only text.
/// </summary>
public class TextLine : LayoutComponentBase
{
    internal static ITextWidthMeasurer? Measurer { get; set; }

    private static readonly double _divider = 8;
    private static readonly double _maxFontSizeLimit = 30;
    private static readonly double _minFontSizeLimit = 6;

    public string Name { get; private set; }
    public int NumberToLocate { get; private set; }
    public string Alignment { get; private set; } = "Left";
    public double FontSize { get; private set; }
    public string FontName { get; private set; } = "Century Gothic";
    public string ForegroundHexStr { get; private set; } = "#730000";
    public string FontWeight { get; private set; } = "Normal";
    public Thickness Padding { get; private set; }
    private string _content = string.Empty;
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
    public bool IsBoundViolating 
    {
        get { return _isBorderViolent; }
        internal set 
        {
            _isBorderViolent = value;
            TextChanged?.Invoke ( this );
        } 
    }

    private bool _isOverLayViolent = false;
    public bool IsOverLaying 
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


    public TextLine ( string name, double width, double height, double topOffset, double leftOffset, string alignment, double fontSize,
        string fontName, string foregroundHexStr, string fontWeight, List<string>? includedLines, bool isSplitable, int numberToLocate 
    )
    {
        Content = "";
        ContentIsSet = false;
        Name = name;
        Width = width;
        Height = height;
        TopOffset = topOffset;
        LeftOffset = leftOffset;
        FontSize = fontSize;
        IncludedLines = includedLines ?? [];
        IsSplitable = isSplitable;
        NumberToLocate = numberToLocate;
        Padding = new Thickness ();

        if ( !string.IsNullOrWhiteSpace(alignment) ) 
        {
            Alignment = alignment;
        }

        if ( !string.IsNullOrWhiteSpace ( fontName ) )
        {
            FontName = fontName;
        }

        if ( !string.IsNullOrWhiteSpace ( foregroundHexStr ) )
        {
            ForegroundHexStr = foregroundHexStr;
        }

        if ( !string.IsNullOrWhiteSpace ( fontWeight ) )
        {
            FontWeight = fontWeight;
        }

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
        IncludedLines = source.IncludedLines ?? [];
        IsSplitable = source.IsSplitable;
        NumberToLocate = source.NumberToLocate;
        isNeeded = true;
        IsBoundViolating = source.IsBoundViolating;
        IsOverLaying = source.IsOverLaying;

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
        TextLine clone = new ( Name, Width, Height, TopOffset, LeftOffset, Alignment, FontSize,
            FontName, ForegroundHexStr, FontWeight, IncludedLines, IsSplitable, NumberToLocate 
            );

        return clone;
    }


    internal TextLine Clone ()
    {
        TextLine clone = new ( this, Content, true );

        return clone;
    }


    internal void TrimUnneededEdgeChar ( List<char>? unNeeded )
    {
        if ( unNeeded == null || unNeeded.Count < 1 ) 
        {
            return;
        }

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

        if ( newFontSize > _maxFontSizeLimit ) 
        {
            return;
        }

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
        List<TextLine> result = [];
        double splitableLineLeftOffset = LeftOffset;
        double offsetInQueue = LeftOffset;

        foreach ( string content   in   pieces )
        {
            TextLine newLine = new ( this, content, false )
            {
                LeftOffset = offsetInQueue
            };

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