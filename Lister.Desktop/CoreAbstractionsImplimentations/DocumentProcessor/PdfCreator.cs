using Lister.Core.DocumentProcessor;
using Lister.Core.DocumentProcessor.Abstractions;
using Lister.Core.Models.Badge;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SkiaSharp;


namespace Lister.Desktop.CoreAbstractionsImplimentations.DocumentProcessor;

public class PdfCreator : IPdfCreator
{
    private static readonly double _coefficient = 0.721;
    private static PdfCreator _instance = null;
    private static string _osName;

    private Dictionary<string, Image> pathToInsideImage = new();
    private List<string> _registeredFonts = new();


    private PdfCreator() { }


    public static PdfCreator GetInstance(string osName)
    {
        if (_instance == null)
        {
            _instance = new PdfCreator();
        }

        _osName = osName;

        return _instance;
    }


    public bool CreateAndSave(List<Page> pages, string filePathToSave)
    {
        bool isNothingToDo = pages == null
                             ||
                             pages.Count == 0;

        if (isNothingToDo)
        {
            return false;
        }

        Settings.License = LicenseType.Community;

        Document doc = CreateDocument( pages );

        try
        {
            doc.GeneratePdf( filePathToSave );
            return true;
        }
        catch (IOException ex)
        {
            return false;
        }
    }


    public IEnumerable<byte[]> Create(List<Page> pages)
    {
        Settings.License = LicenseType.Community;
        Document doc = CreateDocument( pages );
        var settings = new ImageGenerationSettings();
        settings.ImageFormat = ImageFormat.Jpeg;

        IEnumerable<byte[]> result = doc.GenerateImages( settings );

        return result;
    }


    private Document CreateDocument(List<Page> pages)
    {
        Document doc = Document.Create
        ( container =>
        {
            for (int pageNumber = 0; pageNumber < pages.Count; pageNumber++)
            {
                container.Page( page =>
                {
                    Page currentPage = pages[pageNumber];
                    float width = (float)(currentPage.Width * _coefficient);
                    float height = (float)(currentPage.Height * _coefficient);

                    page.Size( width, height, Unit.Point );
                    page.MarginLeft( 0, Unit.Point );
                    page.MarginTop( 0, Unit.Point );
                    page.PageColor( QuestPDF.Helpers.Colors.White );
                    page.DefaultTextStyle( x => x.FontSize( 10 ) );

                    List<BadgeLine> lines = currentPage.Lines;

                    page.Content().PaddingTop( (float)(currentPage.ContentTopOffset * _coefficient) )
                    .PaddingLeft( (float)(currentPage.ContentLeftOffset * _coefficient) ).Column
                    (
                        column =>
                        {
                            foreach (BadgeLine currentLine in lines)
                            {
                                RenderLine( column, currentLine );
                            }
                        }
                    );
                } );
            }
        }
        );

        return doc;
    }


    private void RenderLine(ColumnDescriptor column, BadgeLine line)
    {
        column.Item()
              .Table
              (
                  table =>
                  {
                      table.ColumnsDefinition
                      (
                          columns =>
                          {
                              for (int badgeNumber = 0; badgeNumber < line.Badges.Count; badgeNumber++)
                              {
                                  Badge beingRendered = line.Badges[badgeNumber];
                                  float badgeWidth = (float)(beingRendered.Layout.Width * _coefficient);
                                  columns.ConstantColumn( badgeWidth, Unit.Point );
                                  RenderBadge( table, beingRendered, badgeNumber );
                              }
                          }
                      );
                  }
              );
    }


    private void RenderBadge(TableDescriptor tableForLine, Badge beingRendered, int badgeIndex)
    {
        if (beingRendered == null) return;
        float badgeWidth = (float)(beingRendered.Layout.Width * _coefficient);
        float badgeHeight = (float)(beingRendered.Layout.Height * _coefficient);
        string imagePath = beingRendered.BackgroundImagePath;

        Image image = GetImageByPath( imagePath );

        tableForLine.Cell().Row( 1 ).Column( (uint)badgeIndex + 1 )
            .Width( badgeWidth, Unit.Point ).Height( badgeHeight, Unit.Point )
            .Layers
            (
                layers =>
                {
                    IContainer container = layers.PrimaryLayer().Border( 0.5f, Unit.Point )
                                          .BorderColor( QuestPDF.Helpers.Colors.Grey.Medium );

                    if (image != null)
                    {
                        container.Image( image ).FitArea();
                    }

                    RenderTextLines( layers, beingRendered.Layout.TextLines, beingRendered );
                    RenderInsideImages( layers, beingRendered.Layout.Images );
                    RenderInsideShapes( layers, beingRendered.Layout.Shapes );
                }
            );
    }


    private void RenderTextLines(LayersDescriptor layers, IEnumerable<TextLine> textLines, Badge renderable)
    {
        foreach (TextLine textLine in textLines)
        {
            TextBlockDescriptor textBlock = layers
            .Layer()
            .PaddingLeft( (float)(textLine.LeftOffset * _coefficient), Unit.Point )
            .PaddingTop( (float)((textLine.TopOffset + 2 * textLine.Padding.Top) * _coefficient), Unit.Point )
            .Text( textLine.Content )
            .ClampLines( 1, "." )
            .FontFamily( textLine.FontName )
            .FontColor( Color.FromHex( textLine.ForegroundHexStr ) )
            .FontSize( (float)(textLine.FontSize * _coefficient) );

            if (textLine.FontWeight == "Thin")
            {
                textBlock.Thin();
            }
            else if (textLine.FontWeight == "Bold")
            {
                textBlock.Bold();
            }
        }
    }


    private void RenderInsideImages(LayersDescriptor layers, IEnumerable<ComponentImage> insideImages)
    {
        foreach (ComponentImage image in insideImages)
        {
            Image img = GetImageByPath( image.Path );

            if (img == null) continue;

            layers
                .Layer()
                .PaddingLeft( (float)(image.LeftOffset * _coefficient) )
                .PaddingTop( (float)(image.TopOffset * _coefficient) )
                .Container()
                .Width( (float)(image.Width * _coefficient) )
                .Image( img )
                .FitArea();
        }
    }


    private void RenderInsideShapes(LayersDescriptor layers, IEnumerable<ComponentShape> insideShapes)
    {
        foreach (ComponentShape shape in insideShapes)
        {
            layers
                .Layer()
                .SkiaSharpCanvas
                (
                    (canvas, size) =>
                    {
                        using SKPaint paint = new SKPaint();

                        SKColor color;

                        if (!SKColor.TryParse( shape.FillHexStr, out color ))
                        {
                            color = new SKColor( 0, 0, 0, 255 );
                        }

                        paint.Color = color;

                        if (shape.Type == ShapeType.rectangle)
                        {
                            canvas.DrawRect( (float)(shape.LeftOffset * _coefficient)
                                           , (float)(shape.TopOffset * _coefficient)
                                           , (float)(shape.Width * _coefficient)
                                           , (float)(shape.Height * _coefficient), paint );
                        }
                        else if (shape.Type == ShapeType.ellipse)
                        {
                            float centerVerticalCoordinate = (float)(shape.TopOffset * _coefficient
                                                           + shape.Height * _coefficient / 2);
                            float centerHorizontalCoordinate = (float)(shape.LeftOffset * _coefficient
                                                              + shape.Width * _coefficient / 2);

                            canvas.DrawOval( centerHorizontalCoordinate, centerVerticalCoordinate
                                           , (float)(shape.Width * _coefficient) / 2
                                           , (float)(shape.Height * _coefficient) / 2, paint );
                        }
                    }
                );
        }
    }


    private Image? GetImageByPath(string path)
    {
        if (_osName == "Linux")
        {
            path = "/" + path;
        }

        if (!pathToInsideImage.ContainsKey( path ))
        {
            if (File.Exists( path ))
            {
                pathToInsideImage.Add( path, Image.FromFile( path ) );
            }
            else
            {
                return null;
            }
        }

        return pathToInsideImage[path];
    }
}





