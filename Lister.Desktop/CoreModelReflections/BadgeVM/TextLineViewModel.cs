using Avalonia.Controls;
using Avalonia.Media;
using Lister.Core.Models.Badge;
using QuestPDF.Infrastructure;
using ReactiveUI;

namespace Lister.Desktop.CoreModelReflections.BadgeVM;

public class TextLineViewModel : BadgeComponentBase
{
    private static readonly double _maxFontSizeLimit = 30;
    private static readonly double _minFontSizeLimit = 6;
    private static TextBlock _textBlock;
    private string _alignmentName;

    internal TextLine Model { get; private set; }
    internal string StartContent { get; private set; }

    private HorizontalAlignment _alignment;
    internal HorizontalAlignment Alignment
    {
        get { return _alignment; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _alignment, value, nameof( Alignment ) );
        }
    }

    private Avalonia.Thickness _padding;
    internal Avalonia.Thickness Padding
    {
        get { return _padding; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _padding, value, nameof( Padding ) );
        }
    }

    private double _fontSize;
    internal double FontSize
    {
        get { return _fontSize; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _fontSize, value, nameof( FontSize ) );
        }
    }

    private FontFamily _fontFamily;
    internal FontFamily FontFamily
    {
        get { return _fontFamily; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _fontFamily, value, nameof( FontFamily ) );
        }
    }

    private Avalonia.Media.FontWeight _fontWeight;
    internal Avalonia.Media.FontWeight FontWeight
    {
        get { return _fontWeight; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _fontWeight, value, nameof( FontWeight ) );
        }
    }

    private string _content;
    internal string Content
    {
        get { return _content; }
        set
        {
            this.RaiseAndSetIfChanged( ref _content, value, nameof( Content ) );
        }
    }

    private IBrush _foreground;
    internal IBrush Foreground
    {
        get { return _foreground; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _foreground, value, nameof( Foreground ) );
        }
    }

    private IBrush _background;
    internal IBrush Background
    {
        get { return _background; }
        set
        {
            this.RaiseAndSetIfChanged( ref _background, value, nameof( Background ) );
        }
    }

    private bool _isSplitable;
    internal bool IsSplitable
    {
        get { return _isSplitable; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _isSplitable, value, nameof( IsSplitable ) );
        }
    }

    private double _usefullWidth;
    internal double UsefullWidth
    {
        get { return _usefullWidth; }
        set
        {
            this.RaiseAndSetIfChanged( ref _usefullWidth, value, nameof( UsefullWidth ) );
            Width = value;
        }
    }

    private double _usefullHeight;
    internal double UsefullHeight
    {
        get { return _usefullHeight; }
        set
        {
            this.RaiseAndSetIfChanged( ref _usefullHeight, value, nameof( UsefullHeight ) );
        }
    }

    internal bool IsBorderViolent = false;
    internal bool IsOverLayViolent = false;


    public TextLineViewModel(TextLine model)
    {
        _alignmentName = model.Alignment;
        Model = model;
        base.Model = model;

        bool fontSizeIsOutOfRange = model.FontSize < _minFontSizeLimit
                                    ||
                                    model.FontSize > _maxFontSizeLimit;
        if (fontSizeIsOutOfRange)
        {

        }

        FontSize = model.FontSize;
        FontWeight = GetFontWeight( model.FontWeight );
        string fontName = model.FontName;
        FontFamily = new FontFamily( fontName );

        Content = model.Content;
        StartContent = model.Content;
        IsSplitable = model.IsSplitable;

        Avalonia.Media.Color color;
        bool isColor = Avalonia.Media.Color.TryParse( model.ForegroundHexStr, out color );

        if (!Avalonia.Media.Color.TryParse( model.ForegroundHexStr, out color ))
        {
            color = new Avalonia.Media.Color( 255, 200, 200, 200 );
        }

        SolidColorBrush foreground = new SolidColorBrush( color );
        Foreground = foreground;

        Height = model.Height;
        Padding = new Avalonia.Thickness( model.Padding.Left, model.Padding.Top );

        SetUp( model.Width, FontSize, model.TopOffset, model.LeftOffset );
        SetUsefullWidth();

        Model.Changed += SetViaModel;
        Model.TextChanged += SetViaModel;
    }


    private TextLineViewModel(TextLineViewModel source)
    {
        _alignmentName = source._alignmentName;
        Model = source.Model;
        FontSize = source.FontSize;
        FontFamily = source.FontFamily;
        FontWeight = source.FontWeight;
        Content = source.Content;
        StartContent = source.StartContent;
        IsSplitable = source.IsSplitable;
        Padding = source.Padding;
        UsefullWidth = source.UsefullWidth;
        UsefullHeight = source.HeightWithBorder;
        Foreground = source.Foreground;
        Background = source.Background;
        Padding = source.Padding;

        SetUp( source.UsefullWidth, source.Height, source.TopOffset, source.LeftOffset );

        Model.TextChanged += SetViaModel;
    }


    private static Avalonia.Media.FontWeight GetFontWeight(string weightName)
    {
        Avalonia.Media.FontWeight weight = Avalonia.Media.FontWeight.Normal;

        if (weightName == "Bold")
        {
            weight = Avalonia.Media.FontWeight.Bold;
        }
        else if (weightName == "Thin")
        {
            weight = Avalonia.Media.FontWeight.Thin;
        }

        return weight;
    }


    private void SetUsefullWidth()
    {
        UsefullWidth = Model.UsefullWidth * _scale;
        UsefullHeight = HeightWithBorder * _scale;
    }


    internal TextLineViewModel Clone()
    {
        TextLineViewModel clone = new TextLineViewModel( this );
        return clone;
    }


    internal void ZoomOn(double coefficient)
    {
        base.ZoomOn( coefficient );
        FontSize *= coefficient;
        UsefullWidth *= coefficient;
        UsefullHeight *= coefficient;
        Padding = new Avalonia.Thickness( Padding.Left * coefficient, Padding.Top * coefficient );
    }


    internal void ZoomOut(double coefficient)
    {
        base.ZoomOut( coefficient );
        FontSize /= coefficient;
        UsefullWidth /= coefficient;
        UsefullHeight /= coefficient;
        Padding = new Avalonia.Thickness( Padding.Left / coefficient, Padding.Top / coefficient );
    }


    private void SetViaModel(LayoutComponentBase source)
    {
        FontSize = Model.FontSize * _scale;
        Content = Model.Content;
        SetUp( Model.Width, Model.FontSize, Model.TopOffset, Model.LeftOffset );
        SetUsefullWidth();

        IsBorderViolent = Model.IsPaddingViolent;
        IsOverLayViolent = Model.IsOverLayViolent;
    }
}
