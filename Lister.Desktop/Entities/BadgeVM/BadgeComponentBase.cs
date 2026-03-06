using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lister.Desktop.Entities.BadgeVM;

/// <summary>
/// Represents base for any movable badge component.
/// </summary>
abstract public partial class BadgeComponentBase : ObservableObject
{
    protected double _scale = 1;

    [ObservableProperty]
    private double _topOffsetWithBorder;

    [ObservableProperty]
    private double _leftOffsetWithBorder;

    [ObservableProperty]
    private SolidColorBrush? _borderColor;

    private double _width;
    public double Width
    {
        get 
        { 
            return _width; 
        }

        protected set
        {
            _width = value;
            OnPropertyChanged ();
            WidthWithBorder = Width + BorderThickness.Left + BorderThickness.Right;
        }
    }

    private double _height;
    public double Height
    {
        get 
        {
            return _height; 
        }

        protected set
        {
            _height = value;
            OnPropertyChanged ();
            HeightWithBorder = Height + BorderThickness.Top + BorderThickness.Bottom;
        }
    }

    private double _widthWithBorder;
    public double WidthWithBorder
    {
        get 
        { 
            return _widthWithBorder;
        }

        protected set
        {
            _widthWithBorder = value;
            OnPropertyChanged ();
        }
    }

    private double _heightWithBorder;
    public double HeightWithBorder
    {
        get 
        { 
            return _heightWithBorder;
        }

        protected set
        {
            _heightWithBorder = value;
            OnPropertyChanged ();
        }
    }

    private Avalonia.Thickness _borderThickness;
    public Avalonia.Thickness BorderThickness
    {
        get 
        { 
            return _borderThickness;
        }

        protected set
        {
            _borderThickness = value;
            OnPropertyChanged ();

            LeftOffsetWithBorder = LeftOffset - BorderThickness.Left;
            TopOffsetWithBorder = TopOffset - BorderThickness.Top;
            HeightWithBorder = Height + BorderThickness.Top + BorderThickness.Bottom;
            WidthWithBorder = Width + BorderThickness.Left + BorderThickness.Right;
        }
    }

    private double _topOffset;
    public double TopOffset
    {
        get 
        {
            return _topOffset; 
        }

        set
        {
            _topOffset = value;
            OnPropertyChanged ();
            TopOffsetWithBorder = TopOffset - BorderThickness.Top;
        }
    }

    private double _leftOffset;
    public double LeftOffset
    {
        get 
        { 
            return _leftOffset; 
        }

        set
        {
            _leftOffset = value;
            OnPropertyChanged ();
            LeftOffsetWithBorder = LeftOffset - BorderThickness.Left;
        }
    }

    protected void SetUp ( double width, double height, double topOffset, double leftOffset )
    {
        Width = width * _scale;
        Height = height * _scale;
        TopOffset = topOffset * _scale;
        LeftOffset = leftOffset * _scale;
    }

    public void Focus ( SolidColorBrush borderColor, Avalonia.Thickness borderThickness )
    {
        BorderColor = borderColor ?? new SolidColorBrush ( new Color ( 255, 0, 0, 0 ) );
        BorderThickness = borderThickness;
    }

    public void DisFocus ()
    {
        BorderColor = null;
        BorderThickness = new Avalonia.Thickness ( 0, 0, 0, 0 );
    }

    public void Mark ( SolidColorBrush borderColor, Avalonia.Thickness borderThickness )
    {
        BorderColor = borderColor;
        BorderThickness = borderThickness;
    }

    internal virtual void ZoomOn ( double coefficient )
    {
        _scale *= coefficient;
        Width *= coefficient;
        Height *= coefficient;
        TopOffset *= coefficient;
        LeftOffset *= coefficient;
    }

    internal virtual void ZoomOut ( double coefficient )
    {
        _scale /= coefficient;
        Width /= coefficient;
        Height /= coefficient;
        TopOffset /= coefficient;
        LeftOffset /= coefficient;
    }
}
