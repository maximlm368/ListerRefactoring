using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Lister.Desktop.Entities.BadgeVM;

/// <summary>
/// Represents visible entity that reflects badge correctness.
/// </summary>
internal sealed partial class BadgeCorrectnessViewModel : ObservableObject
{
    private static readonly string _correctIcon = "\uf00c";
    private static readonly string _incorrectIcon = "\uf00d";
    private static readonly SolidColorBrush _focusedBackground = new ( new Color ( 0xff, 0xba, 0xdc, 0xf8 ) );
    private static readonly SolidColorBrush _defaultBackground = new ( new Color ( 0xff, 0xee, 0xee, 0xee ) );

    [ObservableProperty]
    private bool _correctness;

    [ObservableProperty]
    private SolidColorBrush _background = new ( new Color ( 0, 0, 0, 0 ) );

    [ObservableProperty]
    private string _boundPersonName = string.Empty;

    [ObservableProperty]
    private double _personNameExpending;

    //default value exists for just load event can happen
    private double _widthLimit = 20;
    internal double WidthLimit 
    {  
        get => _widthLimit; 
        
        set 
        { 
            _widthLimit = value;
            CalcStringPresentation ();
        } 
    }

    [ObservableProperty]
    private FontFamily _boundFontFamily;

    [ObservableProperty]
    private SolidColorBrush _correctnessColor = new ( new Color ( 0, 0, 0, 0 ) );

    private string _correctnessIcon = string.Empty;
    internal string CorrectnessIcon
    {
        get => _correctnessIcon;

        private set
        {
            CorrectnessColor = value == _correctIcon ? new SolidColorBrush ( new Color ( 0xff, 0x3a, 0x81, 0x3A ) ) : 
                new SolidColorBrush ( new Color ( 0xff, 0xd2, 0x36, 0x50 ) );

            _correctnessIcon = value;
            OnPropertyChanged ();
        }
    }

    private FontWeight _boundFontWeight = FontWeight.Normal;
    internal FontWeight BoundFontWeight
    {
        get => _boundFontWeight;

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

    internal BadgeCorrectnessViewModel ( BadgeViewModel badge, bool isExtended )
    {
        BoundBadge = badge;
        BoundFontWeight = FontWeight.Normal;
        BoundFontFamily = FontManager.Current.DefaultFontFamily.Name;
        BoundBadge.CorrectnessChanged += SetCorrectness;

        SetCorrectness ( badge.IsCorrect );
    }

    private void SetCorrectness ( bool isCorrect ) 
    {
        Correctness = isCorrect;
        CorrectnessIcon = isCorrect ? _correctIcon : _incorrectIcon;
    }

    internal void SwitchCorrectness ()
    {
        Correctness = !Correctness;
        CorrectnessIcon = Correctness ? _correctIcon : _incorrectIcon;
    }

    internal void CalcStringPresentation ( )
    {
        string tail = "...";
        string personPresentation = BoundBadge.Model.Person.FullName;

        FormattedText formatted = new ( personPresentation, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            Typeface.Default, 16, null
        );

        formatted.SetFontWeight ( BoundFontWeight );

        if ( formatted.Width <= WidthLimit )
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

            if ( formatted.Width <= WidthLimit )
            {
                personPresentation = subStr;

                break;
            }
        }

        BoundPersonName = personPresentation;
    }
}
