using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System.Text;


namespace Lister.Desktop.ExecutersForCoreAbstractions.DocumentProcessor;

public static class IContainerExtentions
{
    public static void SkiaSharpCanvas(this IContainer container, Action<SKCanvas, Size> drawOnCanvas)
    {
        container.Svg( size =>
        {
            using var stream = new MemoryStream();

            using (var canvas = SKSvgCanvas.Create( new SKRect( 0, 0, size.Width, size.Height ), stream ))
                drawOnCanvas( canvas, size );

            var svgData = stream.ToArray();
            return Encoding.UTF8.GetString( svgData );
        } );
    }
}