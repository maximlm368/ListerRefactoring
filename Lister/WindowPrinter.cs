using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    internal class WindowPrinter
    {
        //private void PrintOnWindows ( List<PageViewModel> printables )
        //{
        //    IEnumerable<byte []> intermediateBytes = null;

        //    bool dataIsGenerated = _converter.ConvertToExtention (printables, null, out intermediateBytes);

        //    if ( dataIsGenerated )
        //    {
        //        foreach ( byte [] pageBytes in intermediateBytes )
        //        {
        //            using Stream intermediateStream = new MemoryStream (pageBytes);
        //            using PrintDocument pd = new System.Drawing.Printing.PrintDocument ();

        //            pd.PrinterSettings.PrinterName = _printAdjusting.PrinterName;
        //            pd.PrinterSettings.Copies = ( short ) _printAdjusting.CopiesAmount;

        //            pd.PrintPage += ( sender, args ) =>
        //            {
        //                System.Drawing.Image img = System.Drawing.Image.FromStream (intermediateStream);
        //                args.Graphics.DrawImage (img, args.Graphics.VisibleClipBounds);
        //            };

        //            //pd.Print ();
        //        }
        //    }
        //}
    }
}
