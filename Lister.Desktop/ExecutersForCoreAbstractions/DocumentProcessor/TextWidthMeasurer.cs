using Avalonia.Media;
using Lister.Core.DocumentProcessor.Abstractions;

namespace Lister.Desktop.ExecutersForCoreAbstractions.DocumentProcessor;

/// <summary>
/// Carries out measurement of passed on text line.
/// </summary>
public sealed class TextWidthMeasurer : ITextWidthMeasurer
{
    private static ITextWidthMeasurer _instance;
    private TextWidthMeasurer() { }


    public static ITextWidthMeasurer GetMesurer()
    {
        if (_instance == null)
        {
            _instance = new TextWidthMeasurer();
        }

        return _instance;
    }


    public double Measure(string text, string fontWeightName, double fontSize, string fontName)
    {
        FormattedText formatted = new FormattedText( text
                       , System.Globalization.CultureInfo.CurrentCulture
                       , FlowDirection.LeftToRight, Typeface.Default
                       , fontSize, null );

        formatted.SetFontWeight( GetFontWeight( fontWeightName ) );
        formatted.SetFontSize( fontSize );
        formatted.SetFontFamily( new FontFamily( fontName ) );

        return formatted.Width;
    }


    private static FontWeight GetFontWeight(string weightName)
    {
        FontWeight weight = FontWeight.Normal;

        if (weightName == "Bold")
        {
            weight = FontWeight.Bold;
        }
        else if (weightName == "Thin")
        {
            weight = FontWeight.Thin;
        }

        return weight;
    }
}