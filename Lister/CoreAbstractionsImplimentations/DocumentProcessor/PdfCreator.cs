using Core.DocumentProcessor;
using Core.DocumentProcessor.Abstractions;
using Core.Models.Badge;
using QuestPDF;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SkiaSharp;


namespace View.CoreAbstractionsImplimentations.DocumentProcessor;

public class PdfCreator : IPdfCreator
{
    private static PdfCreator _instance = null;

    private readonly string _osName;
    private Dictionary<string, Image> pathToInsideImage = new();
    private List<string> _registeredFonts = new ();
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


    public bool CreateAndSave (List<Page> pages, string filePathToSave)
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


    public IEnumerable<byte[]> Create (List<Page> pages)
    {
        Settings.License = LicenseType.Community;
        Document doc = CreateDocument( pages );
        var settings = new ImageGenerationSettings();
        settings.ImageFormat = ImageFormat.Jpeg;

        IEnumerable<byte []> result = doc.GenerateImages ( settings );

        return result;
    }


    private Document CreateDocument(List<Page> pages)
    {
        Document doc = Document.Create
        ( container =>
        {
            for (int pageNumber = 0;   pageNumber < pages.Count;   pageNumber++)
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
                            foreach (BadgeLine currentLine  in  lines)
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


    private void RenderTextLines (LayersDescriptor layers, IEnumerable<TextLine> textLines, Badge renderable)
    {
        foreach ( TextLine textLine   in   textLines )
        {
            RegisterFontFor ( textLine );

            TextBlockDescriptor textBlock = layers
            .Layer ()
            .PaddingLeft ( ( float ) textLine.LeftOffset, Unit.Point )
            .PaddingTop ( ( float ) ( textLine.TopOffset + 2 * textLine.Padding.Top ), Unit.Point )
            .Text ( textLine.Content )
            .ClampLines ( 1, "." )
            .FontFamily ( textLine.FontName )
            .FontColor ( Color.FromHex ( textLine.ForegroundHexStr ) )
            .FontSize ( ( float ) textLine.FontSize );

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


    private void RegisterFontFor ( TextLine textLine )
    {
        if ( ( _osName == "Linux" )   &&   !_registeredFonts.Contains ( textLine.FontName ) )
        {
            _registeredFonts.Add ( textLine.FontName );
            string fontName = textLine.FontName;

            if ( textLine.FontName.Contains ( ' ' ) )
            {
                fontName = string.Empty;

                for ( int index = 0;   index < textLine.FontName.Length;   index++ )
                {
                    if ( textLine.FontName [index] == ' ' )
                    {
                        fontName += "\'";
                    }

                    fontName += textLine.FontName [index];
                }
            }

            string command = "fc-list | grep " + fontName;

            string result = PdfPrinterImplementation.ExecuteBashCommand ( command );

            if ( ! string.IsNullOrWhiteSpace ( result ) )
            {
                List<string> fonts = new ();
                int substrStart = 0;

                for ( int index = 0; index < result.Length; index++ )
                {
                    if ( ( result [index] == '\n' ) && ( index != result.Length - 1 ) )
                    {
                        fonts.Add ( result.Substring ( substrStart, index ) );
                        substrStart = index + 1;
                    }
                }

                List<string> fontPathes = new ();

                foreach ( string font   in   fonts )
                {
                    for ( int index = 0;   index < font.Length;   index++ )
                    {
                        if ( font [index] == ':' )
                        {
                            fontPathes.Add ( font.Substring ( 0, index ) );
                            break;
                        }
                    }
                }

                foreach ( string fontPath   in   fontPathes )
                {
                    using FileStream stream = new FileStream ( fontPath, FileMode.OpenOrCreate );
                    FontManager.RegisterFont ( stream );
                }
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
                .PaddingLeft( ( float ) image.LeftOffset )
                .PaddingTop( ( float ) image.TopOffset )
                .Container()
                .Width( ( float ) image.Width )
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

                        if ( ! SKColor.TryParse( shape.FillHexStr, out color ))
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


    private Image ? GetImageByPath(string path)
    {
        if (_osName == "Linux")
        {
            path = "/" + path;
        }

        if (! pathToInsideImage.ContainsKey( path ))
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





