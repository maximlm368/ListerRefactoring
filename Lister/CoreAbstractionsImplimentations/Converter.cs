using Core.DocumentProcessor;
using Core.DocumentProcessor.Abstractions;
using Core.Models.Badge;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SkiaSharp;


namespace View.CoreAbstractionsImplimentations;

public class PdfCreator : IPdfCreator
{
    private static PdfCreator _instance = null;

    private readonly string _osName;
    private Dictionary<string, Image> pathToInsideImage = new();
    public IEnumerable<byte[]> bytes = null;
    public List<string> intermidiateFiles = new();


    private PdfCreator(string osName)
    {
        _osName = osName;
    }


    public static PdfCreator GetInstance(string osName)
    {
        if (_instance == null)
        {
            _instance = new PdfCreator( osName );
        }

        return _instance;
    }


    public bool CreateAndSave(List<Page> pages, string filePathToSave)
    {
        bool isNothingToDo = pages == null || pages.Count == 0;

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
        Document doc = CreateDocument( pages );
        var settings = new ImageGenerationSettings();
        settings.ImageFormat = ImageFormat.Jpeg;

        return doc.GenerateImages( settings );
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
                    float width = (float)currentPage.Width;
                    float height = (float)currentPage.Height;

                    page.Size( width, height, Unit.Point );
                    page.MarginLeft( 0, Unit.Point );
                    page.MarginTop( 0, Unit.Point );
                    page.PageColor( QuestPDF.Helpers.Colors.White );
                    page.DefaultTextStyle( x => x.FontSize( 10 ) );

                    List<BadgeLine> lines = currentPage.Lines;

                    page.Content().PaddingTop( 20 ).PaddingLeft( (float)currentPage.ContentLeftOffset ).Column
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
                                  float badgeWidth = (float)beingRendered.Layout.Width;
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
        float badgeWidth = 0;

        try
        {
            badgeWidth = (float)beingRendered.Layout.Width;
        }
        catch (Exception ex) { }

        float badgeHeight = (float)beingRendered.Layout.Height;
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
            string text = textLine.Content;

            float paddingLeft = (float)textLine.LeftOffset;
            float paddingTop = (float)(textLine.TopOffset + 2 * textLine.Padding.Top);

            string fontName = textLine.FontName;
            float fontSize = (float)textLine.FontSize;
            float maxWidth = (float)textLine.Width;

            TextBlockDescriptor textBlock = layers
            .Layer()
            .PaddingLeft( paddingLeft, Unit.Point )
            .PaddingTop( paddingTop, Unit.Point )
            .Text( text )
            .ClampLines( 1, "." )
            .FontFamily( fontName )
            .FontColor( Color.FromHex( textLine.ForegroundHexStr ) )
            .FontSize( fontSize );

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
            float paddingLeft = (float)image.LeftOffset;
            float paddingTop = (float)image.TopOffset;
            float imageWidth = (float)image.Width;
            float imageHeight = (float)image.Height;
            Image img = GetImageByPath( image.Path );

            if (img == null) continue;

            layers
                .Layer()
                .PaddingLeft( paddingLeft )
                .PaddingTop( paddingTop )
                .Container()
                .Width( imageWidth )
                .Image( img )
                .FitArea();
        }
    }


    private void RenderInsideShapes(LayersDescriptor layers, IEnumerable<ComponentShape> insideShapes)
    {
        foreach (ComponentShape shape in insideShapes)
        {
            float paddingLeft = (float)shape.LeftOffset;
            float paddingTop = (float)shape.TopOffset;

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
                            canvas.DrawRect( (float)shape.LeftOffset, (float)shape.TopOffset
                                            , (float)shape.Width, (float)shape.Height, paint );
                        }
                        else if (shape.Type == ShapeType.ellipse)
                        {
                            float centerVerticalCoordinate = (float)(shape.TopOffset + shape.Height / 2);
                            float centerHorizontalCoordinate = (float)(shape.LeftOffset + shape.Width / 2);

                            canvas.DrawOval( centerHorizontalCoordinate, centerVerticalCoordinate
                                             , (float)shape.Width / 2, (float)shape.Height / 2, paint );
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





