using System;
using System.IO;
using System.Drawing;
using QuestPDF;
using QuestPDF.Drawing;
using QuestPDF.Elements;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Pdf = QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;
using ContentAssembler;
using System.Reflection.Metadata;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.Data.Common;
using Lister.Extentions;
using Avalonia.Platform;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Avalonia.Controls.Shapes;
using ExtentionsAndAuxiliary;
using System.Collections.ObjectModel;
using System.Reflection;


namespace Lister.ViewModels;

class ConverterToPdf 
{
    private string ? currentImagePath = null;
    private Pdf.Image? image = null;
    private int step = 0;
    public IEnumerable<byte []> bytes = null;
    public List<string> intermidiateFiles = new ();


    internal bool ConvertToExtentionn ( List<BadgeViewModel> pages, string filePathToSave)
    {
        bool result = true;

        if ( ( pages == null )   ||   (filePathToSave == null) ) 
        {
            result = false;
            return result;
        }

        Settings.License = LicenseType.Community;
        List<BadgeViewModel []> pairs = pages.SeparateIntoPairs ();

        var doc = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size (794, 1123, Unit.Point);
                page.MarginLeft (46, Unit.Point);
                page.MarginTop (30, Unit.Point);
                page.PageColor(Colors.White);
                page.DefaultTextStyle (x => x.FontSize (10));

                page.Content ().Column
                (
                    column =>
                    {
                        for ( int rowNumber = 0;   rowNumber < pairs.Count;   rowNumber++ )
                        {
                            column.Item ()
                            .Table
                            (
                                table =>
                                {
                                    table.ColumnsDefinition (
                                    columns =>
                                    {
                                        columns.ConstantColumn (350, Unit.Point);
                                        columns.ConstantColumn (350, Unit.Point);
                                    });

                                    //RenderPair (table, pairs [rowNumber]);
                                }
                            );
                        }
                    }
                );
            });
        });

        try 
        {
            doc.GeneratePdf (filePathToSave);
        }
        catch ( IOException ex ) 
        {
            result = false;
        }

        return result;
        


        //ImageGenerationSettings imageGenerationSettings = new ImageGenerationSettings ();
        //imageGenerationSettings.RasterDpi = 96;
        //imageGenerationSettings.ImageFormat = ImageFormat.Png;
        //doc.GenerateImages (GetFilePath, imageGenerationSettings);

        //imageGenerationSettings.ImageCompressionQuality = ImageCompressionQuality.VeryLow;
        //bytes = doc.GenerateImages (imageGenerationSettings);
        //FontManager.RegisterFontWithCustomName ("Comic Sans", File.OpenRead (path));
    }


    internal bool ConvertToExtention ( List <PageViewModel> pages, string filePathToSave )
    {
        bool result = true;
        bool isNothingToDo = ( pages == null )   ||   ( pages.Count == 0 )   ||   ( filePathToSave == null );

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
                    float width = (float) PageViewModel.pageSize.Width;
                    float height = (float) PageViewModel.pageSize.Height;

                    page.Size (width, height, Unit.Point);
                    page.MarginLeft (0, Unit.Point);
                    page.MarginTop (0, Unit.Point);
                    page.PageColor (Colors.White);
                    page.DefaultTextStyle (x => x.FontSize (10));

                    ObservableCollection <BadgeLine> lines = currentPage.Lines;

                    page.Content ().Column
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
            doc.GeneratePdf (filePathToSave);
        }
        catch ( IOException ex )
        {
            result = false;
        }

        return result;

    }


    private void RenderPair ( TableDescriptor tableForLine, VMBadge [] pair )
    {
        step++;
        float badgeWidth = 0;

        try 
        {
            badgeWidth = ( float ) pair [0].BadgeModel. badgeDescription. badgeDimensions. outlineSize. Width;
        }
        catch ( Exception ex ) 
        {
            int stp = step;
        }

        float badgeHeight = ( float ) pair [0].BadgeModel. badgeDescription. badgeDimensions. outlineSize. Height;
        float personDataBlockInBadgeWidth = 
                                  ( float ) pair [0].BadgeModel. badgeDescription. badgeDimensions. personTextAreaSize. Width;
        float personDataBlockInBadgeHeight = 
                                  ( float ) pair [0].BadgeModel. badgeDescription. badgeDimensions. personTextAreaSize. Height;
        float personDataBlockLeftPadding = badgeWidth - personDataBlockInBadgeWidth;
        float personDataBlockTopPadding = badgeHeight - personDataBlockInBadgeHeight;

        for ( uint inPairCounter = 0;   inPairCounter < pair.Length;   inPairCounter++ )
        {
            if ( pair [inPairCounter] == null ) 
            {
                break;
            }

            VMBadge beingRenderedBadge = pair [ inPairCounter ];
            string imagePath = beingRenderedBadge.BadgeModel. backgroundImagePath;
            bool firstTime = (currentImagePath == null);
            bool itsTimeToSetNewImage = firstTime   ||   ( currentImagePath != imagePath );

            if ( itsTimeToSetNewImage )
            {
                currentImagePath = imagePath;
                string complitedImagePath = GetImagePath (imagePath);
                image = Pdf.Image.FromFile (complitedImagePath);
            }

            tableForLine.Cell ().Row (1).Column (inPairCounter + 1)
                .Width (badgeWidth, Unit.Point).Height (badgeHeight, Unit.Point)
                .Layers
                            (
                               layers =>
                               {
                                   layers.PrimaryLayer ().Border (0.5f, Unit.Point).BorderColor(Colors.Grey.Medium)
                                    .Image (image)
                                    .FitArea ();

                                   layers
                                    .Layer ()
                                    .PaddingLeft (130, Unit.Point)
                                    .PaddingTop (65, Unit.Point)
                                    .AlignCenter ()
                                    .Column (column =>
                                    {
                                        RenderLastNameLineOnBadge(column, beingRenderedBadge);
                                        RenderFirstAndMiddleNamesLineOnBadge(column, beingRenderedBadge);
                                        RenderDepartmentLineOnBadge(column, beingRenderedBadge);
                                        RenderPositionLineOnBadge(column, beingRenderedBadge);
                                    });  
                               }
                           );
        }
    }


    private void RenderLines ( TableDescriptor tableForLine, BadgeLine line )
    {
        step++;
        float badgeWidth = 0;
        ObservableCollection<BadgeViewModel> badges = line.Badges;

        try
        {
            badgeWidth = ( float ) badges [0].BadgeWidth;
        }
        catch ( Exception ex )
        {
            int stp = step;
        }

        float badgeHeight = ( float ) badges [0].BadgeHeight;

        for ( int index = 0;   index < badges.Count;   index++ )
        {
            BadgeViewModel beingRendered = badges [index];

            if ( beingRendered == null )
            {
                break;
            }

            string imagePath = beingRendered.BadgeModel. BackgroundImagePath;
            bool firstTime = ( currentImagePath == null );
            bool itsTimeToSetNewImage = firstTime || ( currentImagePath != imagePath );

            if ( itsTimeToSetNewImage )
            {
                currentImagePath = imagePath;
                string complitedImagePath = GetImagePath (imagePath);
                image = Pdf.Image.FromFile (complitedImagePath);
            }

            tableForLine.Cell ().Row (1).Column ( (uint) index + 1)
                .Width (badgeWidth, Unit.Point).Height (badgeHeight, Unit.Point)
                .Layers
                            (
                               layers =>
                               {
                                   layers.PrimaryLayer ().Border (0.5f, Unit.Point).BorderColor (Colors.Grey.Medium)
                                    .Image (image)
                                    .FitArea ();

                                   RenderTextLines (layers, beingRendered.TextLines);




                                   

                               }
                           );
        }
    }


    private void RenderLine ( ColumnDescriptor column, BadgeLine line )
    {
        column.Item ()
              .Table
              (
                  table =>
                  {
                      table.ColumnsDefinition (
                          columns =>
                          {
                               for ( int badgeNumber = 0;   badgeNumber < line.Badges.Count;   badgeNumber++ )
                               {
                                   BadgeViewModel beingRendered = line.Badges [badgeNumber];
                                   float badgeWidth = ( float ) beingRendered.BadgeWidth;
                                   columns.ConstantColumn (badgeWidth, Unit.Point);
                                   RenderBadge (table, beingRendered, badgeNumber);
                               }
                          });
                  }
              );
    }


    private void RenderBadge ( TableDescriptor tableForLine, BadgeViewModel beingRendered, int badgeIndex )
    {
        if ( beingRendered == null ) return;

        step++;
        float badgeWidth = 0;

        try
        {
            badgeWidth = ( float ) beingRendered.BadgeWidth;
        }
        catch ( Exception ex )
        {
            int stp = step;
        }

        float badgeHeight = ( float ) beingRendered.BadgeHeight;
        string imagePath = beingRendered.BadgeModel. BackgroundImagePath;
        bool firstTime = ( currentImagePath == null );
        bool itsTimeToSetNewImage = firstTime || ( currentImagePath != imagePath );

        if ( itsTimeToSetNewImage )
        {
            currentImagePath = imagePath;
            string complitedImagePath = GetImagePath (imagePath);
            image = Pdf.Image.FromFile (complitedImagePath);
            //image = Pdf.Image.FromFile (imagePath);
        }

        tableForLine.Cell ().Row (1).Column (( uint ) badgeIndex + 1)
            .Width (badgeWidth, Unit.Point).Height (badgeHeight, Unit.Point)
            .Layers
                        (
                           layers =>
                           {
                               layers.PrimaryLayer ().Border (0.5f, Unit.Point).BorderColor (Colors.Grey.Medium)
                                .Image (image)
                                .FitArea ();

                               RenderTextLines (layers, beingRendered.TextLines);
                               RenderInsideImages (layers, beingRendered.InsideImages);
                               RenderInsideShapes (layers, beingRendered.InsideShapes);
                           }
                       );
    }


    private void RenderTextLines ( ColumnDescriptor column, IEnumerable <TextLineViewModel> textLines )
    {
        foreach ( TextLineViewModel textLine   in   textLines ) 
        {
            string text = textLine.Content;
            float paddingLeft = (float) textLine.LeftOffset;
            float paddingTop = (float) textLine.TopOffset;
            string fontFamily = textLine.FontFamily;
            float fontSize = (float) textLine.FontSize;

            column
                .Item ()
                .PaddingLeft( paddingLeft )
                .PaddingTop( paddingTop )
                .Text (text)
                .FontFamily (fontFamily)
                .FontSize (fontSize);
        }
    }


    private void RenderTextLines ( LayersDescriptor layers, IEnumerable <TextLineViewModel> textLines )
    {
        foreach ( TextLineViewModel textLine   in   textLines )
        {
            string text = textLine.Content;
            float paddingLeft = ( float ) textLine.LeftOffset;
            float paddingTop = ( float ) textLine.TopOffset;
            string fontFamily = textLine.FontFamily;
            float fontSize = ( float ) textLine.FontSize;

            layers
               .Layer ()
               .PaddingLeft (paddingLeft, Unit.Point)
               .PaddingTop (paddingTop, Unit.Point)
               .AlignCenter ()
               .Text (text)
               .FontFamily (fontFamily)
               .FontSize (fontSize);
        }
    }


    private void RenderInsideImages ( LayersDescriptor layers, IEnumerable<ImageViewModel> insideImages )
    {
        foreach ( ImageViewModel image   in   insideImages )
        {
            float paddingLeft = ( float ) image.LeftOffset;
            float paddingTop = ( float ) image.TopOffset;
            string imagePath = image.Path;
            Pdf.Image img = Pdf.Image.FromFile (imagePath);

            layers
                .Layer ()
                .PaddingLeft (paddingLeft)
                .PaddingTop (paddingTop)
                .Image (img);
        }
    }


    private void RenderInsideShapes ( LayersDescriptor layers, IEnumerable <ImageViewModel> insideImages )
    {
        foreach ( ImageViewModel image   in   insideImages )
        {
            float paddingLeft = ( float ) image.LeftOffset;
            float paddingTop = ( float ) image.TopOffset;
            
            layers
                .Layer ()
                .PaddingLeft (paddingLeft)
                .PaddingTop (paddingTop)
                .Canvas (DrawGeometryElement);
                
        }
    }


    private void DrawGeometryElement ( SKCanvas canvas, QuestPDF.Infrastructure.Size size ) 
    {
        SKPaint paint = new SKPaint ();
        paint.Color = new SKColor (877);
        SKRect rect = new SKRect ();
        rect.Size = new SKSize (size.Width, size.Height);
        canvas.DrawRect (rect, paint);
    }


    private void RenderLastNameLineOnBadge ( ColumnDescriptor column, VMBadge beingRenderedItem )
    {
        string lastName = beingRenderedItem.LastName;
        float fontSize = ( float ) beingRenderedItem.FirstLevelFontSize;

        column
        .Item ()
        .AlignCenter ()
        .Text (lastName)
        .FontFamily ("Arial")
        .FontSize (fontSize);

        int amountOfReserveBlocks = beingRenderedItem.ReserveLastNameTextBlocks.Count;

        for ( int reserveBlocksCounter = 0;   reserveBlocksCounter < amountOfReserveBlocks;   reserveBlocksCounter++ )
        {
            lastName = beingRenderedItem.ReserveLastNameTextBlocks [reserveBlocksCounter];

            column
            .Item ()
            .AlignCenter ()
            .Text (lastName)
            .FontFamily ("Arial")
            .FontSize (fontSize);
        }
    }


    private void RenderFirstAndMiddleNamesLineOnBadge ( ColumnDescriptor column, VMBadge beingRenderedItem )
    {
        string firstAndMiddleName = beingRenderedItem.FirstAndSecondName;
        float fontSize = ( float ) beingRenderedItem.SecondLevelFontSize;

        column
        .Item ()
        .AlignCenter ()
        .Text (firstAndMiddleName)
        .FontFamily ("Arial")
        .FontSize (fontSize);

        int amountOfReserveBlocks = beingRenderedItem.ReserveFirstAndMiddleNamesTextBlocks.Count;

        for ( int reserveBlocksCounter = 0;   reserveBlocksCounter < amountOfReserveBlocks;   reserveBlocksCounter++ )
        {
            firstAndMiddleName = beingRenderedItem.ReserveFirstAndMiddleNamesTextBlocks [reserveBlocksCounter];

            column
            .Item ()
            .AlignCenter ()
            .Text (firstAndMiddleName)
            .FontFamily ("Arial")
            .FontSize (fontSize);
        }
    }


    private void RenderDepartmentLineOnBadge ( ColumnDescriptor column, VMBadge beingRenderedItem )
    {
        string departmentName = beingRenderedItem.DepartmentName;
        float fontSize = ( float ) beingRenderedItem.ThirdLevelFontSize;
        int topPadding = ( int ) beingRenderedItem.DepartmentTopPadding;

        column
       .Item ()
       .PaddingTop (topPadding)
       .AlignCenter ()
       .Text (departmentName)
       .FontFamily ("Arial")
       .FontSize (fontSize);

        int amountOfReserveBlocks = beingRenderedItem.ReserveDepartmentTextBlocks.Count;

        for ( int reserveBlocksCounter = 0;   reserveBlocksCounter < amountOfReserveBlocks;   reserveBlocksCounter++ )
        {
            departmentName = beingRenderedItem.ReserveDepartmentTextBlocks [reserveBlocksCounter];

            column
           .Item ()
           .AlignCenter ()
           .Text (departmentName)
           .FontFamily ("Arial")
           .FontSize (fontSize);
        }
    }


    private void RenderPositionLineOnBadge ( ColumnDescriptor column, VMBadge beingRenderedItem )
    {
        string positionName = beingRenderedItem.PositionName;
        float fontSize = ( float ) beingRenderedItem.ThirdLevelFontSize;
        int topPadding = ( int ) beingRenderedItem.PostTopPadding;

        column
       .Item ()
       .PaddingTop (topPadding)
       .AlignCenter ()
       .Text (positionName)
       .FontFamily ("Arial")
       .FontSize (fontSize);

        int amountOfReserveBlocks = beingRenderedItem.ReservePositionTextBlocks.Count;

        for ( int reserveBlocksCounter = 0;   reserveBlocksCounter < amountOfReserveBlocks;   reserveBlocksCounter++ )
        {
            positionName = beingRenderedItem.ReservePositionTextBlocks [reserveBlocksCounter];

            column
           .Item ()
           .AlignCenter ()
           .Text (positionName)
           .FontFamily ("Arial")
           .FontSize (fontSize);
        }
    }


    //private string GetImagePath ( string relativePath )
    //{
    //    var containingDirectory = AppDomain.CurrentDomain. BaseDirectory;

    //    for ( int ancestorDirectoryCounter = 0;   ancestorDirectoryCounter < 5;   ancestorDirectoryCounter++ )
    //    {
    //        containingDirectory = Directory.GetParent (containingDirectory).FullName;
    //    }

    //    string resultPath = containingDirectory + relativePath.Remove (0, 7);
    //    return resultPath;
    //}


    private string GetImagePath ( string relativePath )
    {
        string resultPath = relativePath.Remove (0, 8);
        return resultPath;
    }

}

//step++;
//float badgeWidth = 0;
//ObservableCollection<BadgeViewModel> badges = line.Badges;

//try
//{
//    badgeWidth = ( float ) badges [0].BadgeWidth;
//}
//catch ( Exception ex )
//{
//    int stp = step;
//}

//float badgeHeight = ( float ) badges [0].BadgeHeight;

//for ( int index = 0; index < badges.Count; index++ )
//{
//    BadgeViewModel beingRendered = badges [index];

//    if ( beingRendered == null )
//    {
//        break;
//    }

//    string imagePath = beingRendered.BadgeModel.BackgroundImagePath;
//    bool firstTime = ( currentImagePath == null );
//    bool itsTimeToSetNewImage = firstTime || ( currentImagePath != imagePath );

//    if ( itsTimeToSetNewImage )
//    {
//        currentImagePath = imagePath;
//        string complitedImagePath = GetImagePath (imagePath);
//        image = Pdf.Image.FromFile (complitedImagePath);
//    }

//    tableForLine.Cell ().Row (1).Column (( uint ) index + 1)
//        .Width (badgeWidth, Unit.Point).Height (badgeHeight, Unit.Point)
//        .Layers
//                    (
//                       layers =>
//                       {
//                           layers.PrimaryLayer ().Border (0.5f, Unit.Point).BorderColor (Colors.Grey.Medium)
//                            .Image (image)
//                            .FitArea ();

//                           RenderTextLines (layers, beingRendered.TextLines);






//                       }
//                   );
//}

