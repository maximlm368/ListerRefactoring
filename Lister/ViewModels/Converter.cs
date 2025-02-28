﻿using Core.Models.Badge;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Text;
using Pdf = QuestPDF.Infrastructure;


namespace Lister.ViewModels;

public class ConverterToPdf 
{
    private readonly string _osName;
    private Dictionary <string, Pdf.Image> pathToInsideImage = new ();
    public IEnumerable <byte []> bytes = null;
    public List <string> intermidiateFiles = new ();


    public ConverterToPdf ( string osName ) 
    {
        _osName = osName;
    }


    internal bool ConvertToExtention ( List <PageViewModel> pages, string ? filePathToSave
                                                                 , out IEnumerable <byte []> ? arraysToPassResult )
    {
        arraysToPassResult = null;
        bool result = true;
        bool isNothingToDo = ( pages == null )   ||   ( pages.Count == 0 );

        if ( isNothingToDo )
        {
            return false;
        }

        Settings.License = LicenseType.Community;

        var doc = QuestPDF.Fluent.Document.Create (container =>
        {
            for ( int pageNumber = 0;   pageNumber < pages.Count;   pageNumber++ )
            {
                container.Page (page =>
                {
                    PageViewModel currentPage = pages [pageNumber];
                    float width = (float) PageViewModel.Size.Width;
                    float height = (float) PageViewModel.Size.Height;

                    page.Size (width, height, Unit.Point);
                    page.MarginLeft (0, Unit.Point);
                    page.MarginTop (0, Unit.Point);
                    page.PageColor ( QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle (x => x.FontSize (10));

                    ObservableCollection <BadgeLine> lines = currentPage.Lines;

                    page.Content ().PaddingTop(20).PaddingLeft ( (float) currentPage.ContentLeftOffset).Column
                    (
                        column =>
                        {
                            foreach ( BadgeLine currentLine   in   lines )
                            {
                                RenderLine (column, currentLine);   
                            }
                        }
                    );
                });
            }
        });

        try
        {
            if ( filePathToSave == null )
            {
                var settings = new ImageGenerationSettings ();
                settings.ImageFormat = ImageFormat.Jpeg;

                arraysToPassResult = doc.GenerateImages (settings);
            }
            else
            {
                doc.GeneratePdf (filePathToSave);
            }

            result = true;
        }
        catch ( IOException ex )
        {
            result = false;
        }

        return result;
    }


    private void RenderLine ( ColumnDescriptor column, BadgeLine line )
    {
        column.Item ()
              .Table
              (
                  table =>
                  {
                      table.ColumnsDefinition 
                      (
                          columns =>
                          {
                               for ( int badgeNumber = 0;   badgeNumber < line.Badges. Count;   badgeNumber++ )
                               {
                                   BadgeViewModel beingRendered = line.Badges [badgeNumber];
                                   float badgeWidth = ( float ) beingRendered.BadgeWidth;
                                   columns.ConstantColumn (badgeWidth, Unit.Point);
                                   RenderBadge (table, beingRendered, badgeNumber);
                               }
                          }
                      );
                  }
              );
    }


    private void RenderBadge ( TableDescriptor tableForLine, BadgeViewModel beingRendered, int badgeIndex )
    {
        if ( beingRendered == null ) return;
        float badgeWidth = 0;

        try
        {
            badgeWidth = ( float ) beingRendered.BadgeWidth;
        }
        catch ( Exception ex ){ }

        float badgeHeight = ( float ) beingRendered.BadgeHeight;
        string imagePath = beingRendered.Model. BackgroundImagePath;

        Pdf.Image image = GetImageByPath (imagePath);

        tableForLine.Cell ().Row (1).Column (( uint ) badgeIndex + 1)
            .Width (badgeWidth, Unit.Point).Height (badgeHeight, Unit.Point)
            .Layers
            (
                layers =>
                {
                    IContainer container = layers.PrimaryLayer ().Border (0.5f, Unit.Point)
                                          .BorderColor (QuestPDF.Helpers.Colors.Grey.Medium);

                    if ( image != null )
                    {
                        container.Image (image).FitArea ();
                    }

                    RenderTextLines (layers, beingRendered.TextLines, beingRendered);
                    RenderInsideImages (layers, beingRendered.InsideImages);
                    RenderInsideShapes (layers, beingRendered.InsideRectangles);
                    RenderInsideShapes (layers, beingRendered.InsideEllipses);
                }
            );
    }


    private void RenderTextLines ( LayersDescriptor layers, IEnumerable <TextLineViewModel> textLines
                                                                                          , BadgeViewModel renderable )
    {
        foreach ( TextLineViewModel textLine   in   textLines )
        {
            string text = textLine.Content;

            float paddingLeft = ( float ) textLine.LeftOffset;
            float paddingTop = ( float ) ( textLine.TopOffset + 2 * ( textLine.Padding.Top ) );

            string fontName = textLine.FontFamily.Name;
            Avalonia.Media.FontWeight fontWeight = textLine.FontWeight;
            float fontSize = ( float ) textLine.FontSize;
            float maxWidth = ( float ) textLine.Width;

            TextBlockDescriptor textBlock = layers
            .Layer ()
            .PaddingLeft (paddingLeft, Unit.Point)
            .PaddingTop (paddingTop, Unit.Point)
            .Text (text)
            .ClampLines (1, ".")
            .FontFamily (fontName)
            .FontColor (Pdf.Color.FromHex (textLine.Model.ForegroundHexStr))
            .FontSize (fontSize);

            if ( fontWeight == Avalonia.Media.FontWeight.Thin )
            {
                textBlock.Thin ();
            }
            else if ( fontWeight == Avalonia.Media.FontWeight.Bold )
            {
                textBlock.Bold ();
            }
        }
    }


    private void RenderInsideImages ( LayersDescriptor layers, IEnumerable <ImageViewModel> insideImages )
    {
        foreach ( ImageViewModel image   in   insideImages )
        {
            float paddingLeft = ( float ) image.LeftOffset;
            float paddingTop = ( float ) image.TopOffset;
            float imageWidth = (float) image.Width;
            float imageHeight = (float) image.Height;
            Pdf.Image img = GetImageByPath (image.Path);

            if ( img == null ) continue;

            layers
                .Layer ()
                .PaddingLeft (paddingLeft)
                .PaddingTop (paddingTop)
                .Container()
                .Width (imageWidth)
                .Image (img)
                .FitArea ();
        }
    }


    private void RenderInsideShapes ( LayersDescriptor layers, IEnumerable <ShapeViewModel> insideShapes )
    {
        foreach ( ShapeViewModel shape   in   insideShapes )
        {
            float paddingLeft = ( float ) shape.LeftOffset;
            float paddingTop = ( float ) shape.TopOffset;

            layers
                .Layer ()
                .SkiaSharpCanvas 
                (
                    ( canvas, size ) =>
                    {
                        using SKPaint paint = new SKPaint ();

                        SKColor color;

                        if ( ! SKColor.TryParse (shape.FillColorHexStr, out color) ) 
                        {
                            color = new SKColor (0, 0, 0, 255);
                        }

                        paint.Color = color;

                        if ( shape.Type == ShapeType.rectangle )
                        {
                            canvas.DrawRect (( float ) shape.LeftOffset, ( float ) shape.TopOffset
                                            , ( float ) shape.Width, ( float ) shape.Height, paint);
                        }
                        else if ( shape.Type == ShapeType.ellipse ) 
                        {
                            float centerVerticalCoordinate = ( float ) ( shape.TopOffset + shape.Height / 2 );
                            float centerHorizontalCoordinate = ( float ) ( shape.LeftOffset + shape.Width / 2 );

                            canvas.DrawOval (centerHorizontalCoordinate, centerVerticalCoordinate
                                             , ( float ) shape.Width / 2, ( float ) shape.Height / 2, paint);
                        }
                    }
                );
        }
    }


    private Pdf.Image ? GetImageByPath ( string path )
    {
        if ( _osName == "Linux" )
        {
            path = "/" + path;
        }

        if ( ! pathToInsideImage.ContainsKey (path) )
        {
            if ( File.Exists (path) )
            {
                pathToInsideImage.Add (path, Pdf.Image.FromFile (path));
            }
            else 
            {
                return null;
            }
        }

        return pathToInsideImage [path];
    }

}



public static class IContainerExtentions
{
    public static void SkiaSharpCanvas ( this IContainer container, Action<SKCanvas, Pdf.Size> drawOnCanvas )
    {
        container.Svg (size =>
        {
            using var stream = new MemoryStream ();

            using ( var canvas = SKSvgCanvas.Create (new SKRect (0, 0, size.Width, size.Height), stream) )
                drawOnCanvas (canvas, size);

            var svgData = stream.ToArray ();
            return Encoding.UTF8.GetString (svgData);
        });
    }
}

