using System;
using System.IO;
using System.Drawing;
using QuestPDF;
using QuestPDF.Drawing;
using QuestPDF.Elements;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using SkiaSharp;
using static System.Net.Mime.MediaTypeNames;


namespace ContentAssembler
{
    //public interface IConverterToParticularExtention
    //{
    //    internal IEnumerable<byte[]> ConvertToExtention (List<Item> items);
    //}
    
    public interface IResultOfSessionSaver
    {
        internal void ConvertToExtention(List<Badge> items,  string filePathToSave);
    }



    //public class ImageWithTime 
    //{
    //    public IEnumerable<byte[]> image = null;
    //    public int seconds;
    //    public int milliSeconds;
    //    public int microseconds;
    //}



    public class ConverterToPdf : IResultOfSessionSaver
    {

        void IResultOfSessionSaver.ConvertToExtention (List<Badge> items, string filePathToSave )
        {
            QuestPDF.Settings.License = LicenseType.Community;

            //int amountOfPages = items.Count / 10;

            //if( (items.Count % 10)  !=  0 ) 
            //{
            //    amountOfPages++;
            //}

            var doc = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content()
                    .Table(
                           table =>
                           {
                               RenderAllItems(table, items);
                           }
                        );

                    //page.Content().Column
                    //(
                    //    column =>
                    //    {
                    //        int rowAmount = items.Count() / 2;

                    //        foreach (var rowNumber in Enumerable.Range(0, rowAmount))
                    //        {
                    //            column.Item().Table(
                    //                table =>
                    //                { table.ColumnsDefinition(
                    //                    columns =>
                    //                    {
                    //                        columns.RelativeColumn();
                    //                        columns.RelativeColumn();
                    //                    });

                    //                    RenderPair(table, undergroundImagePath, items.Count().ToString());


                    //                });
                    //        }

                    //    }
                    //);


                });
            });

            //DateTime start = DateTime.Now;


            string filePath = "D:\\MML\\pdfFromDotNet.pdf";
            doc.GeneratePdf (filePath);

            //DateTime finish = DateTime.Now;
            //TimeSpan extended = finish.Subtract(start);
            //int seconds = extended.Seconds;
            //int miliseconds = extended.Milliseconds;
            //int microseconds = extended.Microseconds;

            //var result = new ImageWithTime();
            //result.seconds = seconds;
            //result.microseconds = microseconds;
            //result.milliSeconds = miliseconds;
            //result.image = image;
        }



        private static void RenderPair( TableDescriptor pairsContainer, string undergroundImagePath, string amount ) 
        {
            var text = amount;

            for ( uint inPairCounter = 1;    inPairCounter < 3;     inPairCounter++ ) 
            {
                pairsContainer.Cell().Row(1).Column(inPairCounter).Layers
                                                  (
                                                      layers =>
                                                      {
                                                          layers.PrimaryLayer().Image(undergroundImagePath);
                                                          layers.Layer()
                                                          .Table
                                                          (
                                                            textTable =>
                                                            {
                                                                textTable.ColumnsDefinition(columns =>
                                                                {
                                                                    columns.RelativeColumn();
                                                                    columns.RelativeColumn();
                                                                });

                                                                textTable.Cell().Row(1).Column(1).Width(100);
                                                                textTable.Cell().Row(1).Column(2).Width(100).Text(text);

                                                            }
                                                          );
                                                      }
                                                  );
            }

            
        }



        private static void RenderAllItems ( TableDescriptor table,  List<Badge> items) 
        {
            var image = "D://Projects/Java/Lister/src/nurses.jpg";
            string text = "hflashflh";
            int itemCounter = 0;

            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            for (uint columnCounter = 1;   columnCounter < 3;   columnCounter++)
            {
                for (uint rowCounter = 1;   rowCounter < 6;   rowCounter++)
                {
                    table.Cell().Row(rowCounter).Column(columnCounter)
                                    .Layers
                                    (
                                       layers =>
                                       {
                                           layers.PrimaryLayer().Image(image);
                                           layers.Layer().Width(50).Height(50)
                                           .Table
                                           (
                                             textTable =>
                                             {
                                                 textTable.ColumnsDefinition(columns =>
                                                 {
                                                     columns.RelativeColumn();
                                                     columns.RelativeColumn();
                                                 });

                                                 textTable.Cell().Row(rowCounter).Column(columnCounter).Width(50).Height(50)
                                                 .Text(text);

                                             }
                                           );


                                       }
                                    );
                    itemCounter++;
                }
            }

        }



    //    void GetExample() 
    //    {
    //        QuestPDF.Fluent.Document.Create(container =>
    //        {
    //            container.Page(page =>
    //            {
    //                page.Size(PageSizes.A4);
    //                page.Margin(2, Unit.Centimetre);
    //                page.PageColor(Colors.White);
    //                page.DefaultTextStyle(x => x.FontSize(20));

    //                page.Header()
    //                    .Text("Hello PDF!")
    //                    .SemiBold().FontSize(36).FontColor(Colors.Blue.Medium);

    //                page.Content()
    //                    .PaddingVertical(1, Unit.Centimetre)
    //                    .Column(x =>
    //                    {
    //                        x.Spacing(20);

    //                        x.Item().Text(Placeholders.LoremIpsum());
    //                        x.Item().Image(Placeholders.Image(200, 100));
    //                    });





    //                page.Content()
    //                    .PaddingVertical(1, Unit.Centimetre)
    //                    .Column(x =>
    //                    {
    //                        x.Spacing(20);


    //                        x.Item().Table(x => { });
    //                    });

    //                page.Content()
    //                    .Table(
    //                       table => 
    //                       {
    //                           table.ColumnsDefinition(columns =>
    //                           {
    //                               columns.RelativeColumn();
    //                               columns.RelativeColumn();
    //                           });


    //                           var image = "D://Projects/Java/Lister/src/nurses.jpg";
    //                           string text = "";

    //                           table.Cell().Row(1).Column(1).Element(Block).Layers(layers => { layers.Layer().Image(image); });
    //                           table.Cell().Row(2).Column(1).Element(Block).Layers(layers => { layers.Layer().Image(image); });
    //                           table.Cell().Row(3).Column(1).Element(Block).Layers(layers => { layers.Layer().Image(image); });
    //                           table.Cell().Row(4).Column(1).Element(Block).Layers(layers => { layers.Layer().Image(image); });
    //                           table.Cell().Row(5).Column(1).Element(Block).Layers(layers => { layers.Layer().Image(image); });

    //                           table.Cell().Row(1).Column(2).Element(Block).Layers(layers => { layers.Layer().Image(image); });
    //                           table.Cell().Row(2).Column(2).Element(Block).Layers(layers => { layers.Layer().Image(image); });
    //                           table.Cell().Row(3).Column(2).Element(Block).Layers(layers => { layers.Layer().Image(image); });
    //                           table.Cell().Row(4).Column(2).Element(Block).Layers(layers => { layers.Layer().Image(image); });


    //                           table.Cell().Row(5).Column(2).Element(Block).Border(1)
    //                                .Layers
    //                                (
    //                                   layers => 
    //                                   { 
    //                                       layers.PrimaryLayer().Image(image);
    //                                       layers.Layer().Width(50).Height(50).AlignMiddle().Text(text);
    //                                   }
    //                                );



    //                           static IContainer Block(IContainer container)
    //                           {
    //                               return container
    //                                   .Border(1)
    //                                   .Background(Colors.Grey.Lighten3)
    //                                   .ShowOnce()
    //                                   .MinWidth(50)
    //                                   .MinHeight(50)
    //                                   .AlignCenter()
    //                                   .AlignMiddle();
    //                           }
    //                       }
    //                    );


    //                page.Content()
    //                .Layers(layers =>
    //                {
    //                    // layer below main content
    //                    layers
    //                        .Layer()
    //                        .Height(100)
    //                        .Width(100)
    //                        .Padding(5)
                            
    //                        .Background(Colors.Grey.Lighten3);

    //                    layers
    //                        .PrimaryLayer()
    //                        .Padding(25)
    //                        .Column(column =>
    //                        {
    //                            column.Spacing(5);

    //                            foreach (var _ in Enumerable.Range(0, 7))
    //                                column.Item().Text(Placeholders.Sentence());
    //                        });

    //                    // layer above the main content    
    //                    layers
    //                        .Layer()
    //                        .AlignCenter()
    //                        .AlignMiddle()
    //                        .Text("Watermark")
    //                        .FontSize(48).Bold().FontColor(Colors.Green.Lighten3);

    //                    //layers
    //                    //    .Layer()
    //                    //    .AlignBottom()
    //                    //    .PageNumber("Page {number}")
    //                    //    .FontSize(16).FontColor(Colors.Green.Medium);
    //                });






    //                page.Footer()
    //                    .AlignCenter()
    //                    .Text(x =>
    //                    {
    //                        x.Span("Page ");
    //                        x.CurrentPageNumber();
    //                    });
    //            });
    //        })
    //.GeneratePdf("hello.pdf");

    //    }


        //public static IDocumentContainer Page(this IDocumentContainer document, Action<PageDescriptor> handler)
        //{
        //    var descriptor = new PageDescriptor();
        //    handler(descriptor);

        //    (document as DocumentContainer).Pages.Add(descriptor.Page);

        //    return document;
        //}

    }
}