namespace Core.DocumentProcessor.Abstractions;

public interface ITextWidthMeasurer
{
    public double Measure(string text, string fontWeightName, double fontSize, string fontName);
}