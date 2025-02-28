namespace Core.DocumentBuilder;

public interface ITextWidthMeasurer 
{
    public double Measure ( string text, string fontWeightName, double fontSize, string fontName );
}