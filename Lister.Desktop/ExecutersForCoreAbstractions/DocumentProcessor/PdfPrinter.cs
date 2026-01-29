using Lister.Core.Document;
using Lister.Core.Document.AbstractServices;
using System.Diagnostics;
using System.Drawing.Printing;

namespace Lister.Desktop.ExecutersForCoreAbstractions.DocumentProcessor;

/// <summary>
/// Carries out printing
/// </summary>
public sealed class PdfPrinter : IPdfPrinter
{
    private static readonly PdfPrinter? _instance = null;

    private readonly string _osName;

    private PdfPrinter ( string osName )
    {
        _osName = osName;
    }

    internal static PdfPrinter GetInstance ( string osName )
    {
        return _instance ?? new PdfPrinter ( osName );
    }

    public void Print ( List<Page> printables, IPdfCreator creator, string printerName, int copiesAmount )
    {
        if ( _osName == "Windows" )
        {
            PrintOnWindows ( printables, creator, printerName, copiesAmount );
        }
        else if ( _osName == "Linux" )
        {
            PrintOnLinux ( printables, creator, printerName, copiesAmount );
        }
    }

    internal static string ExecuteBashCommand ( string command )
    {
        using Process process = new ();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start ();

        string result = process.StandardOutput.ReadToEnd ();

        process.WaitForExit ();

        return result;
    }

    private void PrintOnWindows ( List<Page> printables, IPdfCreator creator, string printerName, int copiesAmount )
    {
        if ( _osName == "Windows" && OperatingSystem.IsWindowsVersionAtLeast ( 6, 1 ) ) 
        {
            IEnumerable<byte []> intermediateBytes = creator.Create ( printables );

            if ( intermediateBytes != null && intermediateBytes.Any () )
            {
                foreach ( byte [] pageBytes in intermediateBytes )
                {
                    using Stream intermediateStream = new MemoryStream ( pageBytes );
                    using PrintDocument document = new ();

                    document.PrinterSettings.PrinterName = printerName;
                    document.PrinterSettings.Copies = ( short ) copiesAmount;

                    document.PrintPage += ( sender, args ) =>
                    {
#pragma warning disable CA1416 // Is supported only Windows 6.1 version and later.
                        System.Drawing.Image img = System.Drawing.Image.FromStream ( intermediateStream );
                        args.Graphics?.DrawImage ( img, args.Graphics.VisibleClipBounds );
#pragma warning disable CA1416 // Is supported only Windows 6.1 version and later.
                    };

                    document.Print ();
                }
            }
        }
    }

    private static void PrintOnLinux ( List<Page> printables, IPdfCreator creator, string printerName, int copiesAmount )
    {
        string pdfProxyName = @"./proxy.pdf";
        bool dataIsGenerated = creator.CreateAndSave ( printables, pdfProxyName );

        if ( dataIsGenerated )
        {
            string printer = printerName;
            string copies = copiesAmount.ToString ();
            string bashPrintCommand = "lp -d " + printer + " -n " + copies + " " + pdfProxyName;

            ExecuteBashCommand ( bashPrintCommand );
        }
    }
}
