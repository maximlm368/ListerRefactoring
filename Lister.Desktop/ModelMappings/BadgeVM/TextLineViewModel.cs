using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Lister.Core.Entities.Badge;
using QuestPDF.Infrastructure;

namespace Lister.Desktop.ModelMappings.BadgeVM;

/// <summary>
/// Maps TextLine model into visible entity.
/// </summary>
public sealed partial class TextLineViewModel : BadgeComponentBase
{
    private readonly string _alignmentName;

    internal TextLine Model { get; private set; }
    internal string StartContent { get; private set; }

    [ObservableProperty]
    private HorizontalAlignment _alignment;

    [ObservableProperty]
    private Avalonia.Thickness _padding;

    [ObservableProperty]
    private double _fontSize;

    [ObservableProperty]
    private FontFamily? _fontFamily;

    [ObservableProperty]
    private Avalonia.Media.FontWeight _fontWeight;

    [ObservableProperty]
    private string? _content;

    [ObservableProperty]
    private IBrush? _foreground;

    [ObservableProperty]
    private IBrush? _background;

    [ObservableProperty]
    private bool _isSplitable;

    [ObservableProperty]
    private double _usefullHeight;

    private double _usefullWidth;
    internal double UsefullWidth
    {
        get { return _usefullWidth; }
        set
        {
            _usefullWidth = value;
            OnPropertyChanged ();
            Width = value;
        }
    }

    internal bool IsBorderViolent = false;
    internal bool IsOverLayViolent = false;

    public TextLineViewModel ( TextLine model )
    {
        _alignmentName = model.Alignment;
        Model = model;
        FontSize = model.FontSize;
        FontWeight = GetFontWeight ( model.FontWeight );
        string fontName = model.FontName;
        FontFamily = new FontFamily ( fontName );

        Content = model.Content;
        StartContent = model.Content;
        IsSplitable = model.IsSplitable;

        bool isColor = Avalonia.Media.Color.TryParse ( model.ForegroundHexStr, out Avalonia.Media.Color color );

        if ( !Avalonia.Media.Color.TryParse ( model.ForegroundHexStr, out color ) )
        {
            color = new Avalonia.Media.Color ( 255, 200, 200, 200 );
        }

        SolidColorBrush foreground = new ( color );
        Foreground = foreground;

        Height = model.Height;
        Padding = new Avalonia.Thickness ( model.Padding.Left, model.Padding.Top );

        SetUp ( model.Width, FontSize, model.TopOffset, model.LeftOffset );
        SetUsefullWidth ();

        Model.Changed += SetViaModel;
        Model.TextChanged += SetViaModel;
    }

    private TextLineViewModel ( TextLineViewModel source )
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

        SetUp ( source.UsefullWidth, source.Height, source.TopOffset, source.LeftOffset );

        Model.TextChanged += SetViaModel;
    }

    private static Avalonia.Media.FontWeight GetFontWeight ( string weightName )
    {
        Avalonia.Media.FontWeight weight = Avalonia.Media.FontWeight.Normal;

        if ( weightName == "Bold" )
        {
            weight = Avalonia.Media.FontWeight.Bold;
        }
        else if ( weightName == "Thin" )
        {
            weight = Avalonia.Media.FontWeight.Thin;
        }

        return weight;
    }

    private void SetUsefullWidth ()
    {
        UsefullWidth = Model.UsefullWidth * _scale;
        UsefullHeight = HeightWithBorder * _scale;
    }

    internal TextLineViewModel Clone ()
    {
        TextLineViewModel clone = new ( this );

        return clone;
    }

    internal override void ZoomOn ( double coefficient )
    {
        base.ZoomOn ( coefficient );
        FontSize *= coefficient;
        UsefullWidth *= coefficient;
        UsefullHeight *= coefficient;
        Padding = new Avalonia.Thickness ( Padding.Left * coefficient, Padding.Top * coefficient );
    }

    internal override void ZoomOut ( double coefficient )
    {
        base.ZoomOut ( coefficient );
        FontSize /= coefficient;
        UsefullWidth /= coefficient;
        UsefullHeight /= coefficient;
        Padding = new Avalonia.Thickness ( Padding.Left / coefficient, Padding.Top / coefficient );
    }

    private void SetViaModel ( LayoutComponentBase source )
    {
        FontSize = Model.FontSize * _scale;
        Content = Model.Content;
        SetUp ( Model.Width, Model.FontSize, Model.TopOffset, Model.LeftOffset );
        SetUsefullWidth ();

        IsBorderViolent = Model.IsBoundViolating;
        IsOverLayViolent = Model.IsOverLaying;
    }
}
