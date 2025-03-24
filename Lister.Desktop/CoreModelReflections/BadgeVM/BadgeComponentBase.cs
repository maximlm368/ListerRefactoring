using Avalonia.Media;
using Lister.Core.Models.Badge;
using ReactiveUI;

namespace Lister.Desktop.CoreModelReflections.BadgeVM;

abstract public class BadgeComponentBase : ReactiveObject
{
    protected double _scale = 1;

    public LayoutComponentBase Model { get; protected set; }

    private double _width;
    public double Width
    {
        get { return _width; }
        protected set
        {
            this.RaiseAndSetIfChanged( ref _width, value, nameof( Width ) );
            WidthWithBorder = Width + BorderThickness.Left + BorderThickness.Right;
        }
    }

    private double _height;
    public double Height
    {
        get { return _height; }
        protected set
        {
            this.RaiseAndSetIfChanged( ref _height, value, nameof( Height ) );
            HeightWithBorder = Height + BorderThickness.Top + BorderThickness.Bottom;
        }
    }

    private double _widthWithBorder;
    public double WidthWithBorder
    {
        get { return _widthWithBorder; }
        protected set
        {
            this.RaiseAndSetIfChanged( ref _widthWithBorder, value, nameof( WidthWithBorder ) );
        }
    }

    private double _heightWithBorder;
    public double HeightWithBorder
    {
        get { return _heightWithBorder; }
        protected set
        {
            this.RaiseAndSetIfChanged( ref _heightWithBorder, value, nameof( HeightWithBorder ) );
        }
    }

    private Avalonia.Thickness _borderThickness;
    public Avalonia.Thickness BorderThickness
    {
        get { return _borderThickness; }
        protected set
        {
            this.RaiseAndSetIfChanged( ref _borderThickness, value, nameof( BorderThickness ) );
            LeftOffsetWithBorder = LeftOffset - BorderThickness.Left;
            TopOffsetWithBorder = TopOffset - BorderThickness.Top;
            HeightWithBorder = Height + BorderThickness.Top + BorderThickness.Bottom;
            WidthWithBorder = Width + BorderThickness.Left + BorderThickness.Right;
        }
    }

    private double _topOffset;
    public double TopOffset
    {
        get { return _topOffset; }
        set
        {
            this.RaiseAndSetIfChanged( ref _topOffset, value, nameof( TopOffset ) );
            TopOffsetWithBorder = TopOffset - BorderThickness.Top;
        }
    }

    private double _leftOffset;
    public double LeftOffset
    {
        get { return _leftOffset; }
        set
        {
            this.RaiseAndSetIfChanged( ref _leftOffset, value, nameof( LeftOffset ) );
            LeftOffsetWithBorder = LeftOffset - BorderThickness.Left;
        }
    }

    private double _topOffsetWithBorder;
    public double TopOffsetWithBorder
    {
        get { return _topOffsetWithBorder; }
        set
        {
            this.RaiseAndSetIfChanged( ref _topOffsetWithBorder, value, nameof( TopOffsetWithBorder ) );
        }
    }

    private double _leftOffsetWithBorder;
    public double LeftOffsetWithBorder
    {
        get { return _leftOffsetWithBorder; }
        set
        {
            this.RaiseAndSetIfChanged( ref _leftOffsetWithBorder, value, nameof( LeftOffsetWithBorder ) );
        }
    }

    private SolidColorBrush _borderColor;
    public SolidColorBrush BorderColor
    {
        get { return _borderColor; }
        private set
        {
            this.RaiseAndSetIfChanged( ref _borderColor, value, nameof( BorderColor ) );
        }
    }


    protected void SetUp(double width, double height, double topOffset, double leftOffset)
    {
        Width = width * _scale;
        Height = height * _scale;
        TopOffset = topOffset * _scale;
        LeftOffset = leftOffset * _scale;
    }


    public void BecomeFocused(SolidColorBrush borderColor, Avalonia.Thickness borderThickness)
    {
        BorderColor = borderColor ?? new SolidColorBrush( new Color( 255, 0, 0, 0 ) );
        BorderThickness = borderThickness;
    }


    public void BecomeUnFocused()
    {
        BorderColor = null;
        BorderThickness = new Avalonia.Thickness( 0, 0, 0, 0 );
    }


    public void Mark(SolidColorBrush borderColor, Avalonia.Thickness borderThickness)
    {
        BorderColor = borderColor;
        BorderThickness = borderThickness;
    }


    internal void ZoomOn(double coefficient)
    {
        _scale *= coefficient;
        Width *= coefficient;
        Height *= coefficient;
        TopOffset *= coefficient;
        LeftOffset *= coefficient;
    }


    internal void ZoomOut(double coefficient)
    {
        _scale /= coefficient;
        Width /= coefficient;
        Height /= coefficient;
        TopOffset /= coefficient;
        LeftOffset /= coefficient;
    }
}
