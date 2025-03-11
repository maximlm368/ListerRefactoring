namespace Core.DocumentProcessor.Abstractions;

/// <summary>
/// Defines abstraction implementation of wich is needed to measure widht of text in units of current platform.
/// </summary>
public interface ITextWidthMeasurer
{
    public double Measure(string text, string fontWeightName, double fontSize, string fontName);
}