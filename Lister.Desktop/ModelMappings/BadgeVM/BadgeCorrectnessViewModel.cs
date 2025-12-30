using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lister.Desktop.ModelMappings.BadgeVM;

/// <summary>
/// Represents visible entity that reflects badge correctness.
/// </summary>
internal sealed partial class BadgeCorrectnessViewModel : ObservableObject
{
    private static readonly string _correctIcon;
    private static readonly string _incorrectIcon;
    private static readonly SolidColorBrush _focusedBackground;
    private static readonly SolidColorBrush _defaultBackground;

    private readonly double _extendedScrollableMaxIconWidth;
    private readonly double _shrinkedIconWidth;

    [ObservableProperty]
    private bool _correctness;

    [ObservableProperty]
    private SolidColorBrush _background = new ( new Color ( 0, 0, 0, 0 ) );

    [ObservableProperty]
    private string _boundPersonName = string.Empty;

    [ObservableProperty]
    private double _personNameExpending;

    [ObservableProperty]
    private double _insideBorderWidth;

    [ObservableProperty]
    private FontFamily _boundFontFamily;

    [ObservableProperty]
    private SolidColorBrush _correctnessColor = new ( new Color ( 0, 0, 0, 0 ) );

    private string _correctnessIcon = string.Empty;
    internal string CorrectnessIcon
    {
        get
        {
            return _correctnessIcon;
        }

        private set
        {
            if ( value == _correctIcon )
            {
                byte red = 0x3a;
                byte green = 0x81;
                byte blue = 0x3A;

                CorrectnessColor = new SolidColorBrush ( new Color ( 255, red, green, blue ) );
            }
            else
            {
                byte red = 0xd2;
                byte green = 0x36;
                byte blue = 0x50;

                CorrectnessColor = new SolidColorBrush ( new Color ( 255, red, green, blue ) );
            }

            _correctnessIcon = value;
            OnPropertyChanged ();
        }
    }

    private double _width;
    internal double Width
    {
        get
        {
            return _width;
        }

        set
        {
            InsideBorderWidth = value - 2;
            _width = value;
            OnPropertyChanged ();
        }
    }

    private FontWeight _boundFontWeight = FontWeight.Normal;
    internal FontWeight BoundFontWeight
    {
        get
        {
            return _boundFontWeight;
        }

        set
        {
            if ( !Enum.IsDefined ( typeof ( FontWeight ), value ) )
            {
                return;
            }

            if ( value == FontWeight.Bold )
            {
                Background = _focusedBackground;
            }
            else
            {
                Background = _defaultBackground;
            }

            _boundFontWeight = value;
            OnPropertyChanged ();
        }
    }

    internal BadgeViewModel BoundBadge { get; private set; }

    static BadgeCorrectnessViewModel ()
    {
        _correctIcon = "\uf00c";
        _incorrectIcon = "\uf00d";
        _focusedBackground = new SolidColorBrush ( new Color ( 255, 186, 220, 248 ) );
        _defaultBackground = new SolidColorBrush ( new Color ( 255, 238, 238, 238 ) );
    }

    internal BadgeCorrectnessViewModel ( BadgeViewModel badge, double extendedScrollableWidth, double shortWidth, double widthLimit,
        bool isExtended
    )
    {
        BoundBadge = badge;
        BoundFontWeight = FontWeight.Normal;
        BoundFontFamily = FontManager.Current.DefaultFontFamily.Name;

        CalcStringPresentation ( widthLimit );

        if ( badge.IsCorrect )
        {
            Correctness = true;
            CorrectnessIcon = _correctIcon;
        }
        else
        {
            Correctness = false;
            CorrectnessIcon = _incorrectIcon;
        }

        _extendedScrollableMaxIconWidth = extendedScrollableWidth;
        _shrinkedIconWidth = shortWidth;

        if ( isExtended )
        {
            Width = _extendedScrollableMaxIconWidth;
        }
        else
        {
            Width = _shrinkedIconWidth;
        }
    }

    internal void SwitchCorrectness ()
    {
        if ( Correctness )
        {
            Correctness = false;
            CorrectnessIcon = _incorrectIcon;
        }
        else
        {
            Correctness = true;
            CorrectnessIcon = _correctIcon;
        }
    }

    internal void CalcStringPresentation ( double widthLimit )
    {
        string tail = "...";
        string personPresentation = BoundBadge.Model.Person.FullName;

        FormattedText formatted = new ( personPresentation, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Typeface.Default, 16, null
        );

        formatted.SetFontWeight ( BoundFontWeight );

        if ( formatted.Width <= widthLimit )
        {
            BoundPersonName = personPresentation;

            return;
        }
        else
        {
            personPresentation = personPresentation[..^1] + tail;
        }

        for ( int index = personPresentation.Length - 1; index > 0; index-- )
        {
            string subStr = personPresentation [..( index - 4 )] + tail;

            formatted = new ( subStr, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 16, null );

            formatted.SetFontWeight ( BoundFontWeight );
            formatted.SetFontSize ( 16 );
            formatted.SetFontFamily ( BoundFontFamily );

            if ( formatted.Width <= widthLimit )
            {
                personPresentation = subStr;

                break;
            }
        }

        BoundPersonName = personPresentation;
    }
}
