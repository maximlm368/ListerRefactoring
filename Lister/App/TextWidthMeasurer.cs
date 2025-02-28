using Avalonia.Media;
using Avalonia.Threading;
using Core.DocumentBuilder;

namespace Lister;

public class TextWidthMeasurer : ITextWidthMeasurer
{
    private static ITextWidthMeasurer Instance;


    private TextWidthMeasurer() {}


    public static ITextWidthMeasurer GetMesurer () 
    {
        if ( Instance == null ) 
        {
            Instance = new TextWidthMeasurer();
        }

        return Instance;
    }


    public double Measure ( string text, string fontWeightName, double fontSize, string fontName )
    {
        FormattedText formatted = new FormattedText ( text
                       , System.Globalization.CultureInfo.CurrentCulture
                       , FlowDirection.LeftToRight, Typeface.Default
                       , fontSize, null );

        formatted.SetFontWeight ( GetFontWeight ( fontWeightName ) );
        formatted.SetFontSize ( fontSize );
        formatted.SetFontFamily ( new Avalonia.Media.FontFamily ( fontName ) );

        return formatted.Width;


        //return result1;
    }


    private static FontWeight GetFontWeight ( string weightName )
    {
        FontWeight weight = Avalonia.Media.FontWeight.Normal;

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
}