using Core.DocumentProcessor;
using Core.DocumentProcessor.Abstractions;
using System.Diagnostics;
using System.Drawing.Printing;

namespace View.CoreAbstractionsImplimentations.DocumentProcessor;

public class PdfPrinterImplementation : IPdfPrinter
{
    private static PdfPrinterImplementation _instance = null;

    private string _osName;
    private IPdfCreator _pdgCreator;


    private PdfPrinterImplementation (string osName, IPdfCreator pdfCreator)
    {
        _osName = osName;
        _pdgCreator = pdfCreator;
    }


    internal static PdfPrinterImplementation GetInstance(string osName, IPdfCreator pdfCreator)
    {
        PdfPrinterImplementation instance = _instance ?? new PdfPrinterImplementation( osName, pdfCreator );

        return instance;
    }


    public void Print ( List<Page> printables, IPdfCreator creator, string printerName, int copiesAmount )
    {
        if (_osName == "Windows")
        {
            PrintOnWindows ( printables, creator, printerName, copiesAmount );
        }
        else if (_osName == "Linux")
        {
            PrintOnLinux ( printables, creator, printerName, copiesAmount );
        }
    }


    internal static string ExecuteBashCommand ( string command )
    {
        using ( Process process = new Process () )
        {
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
    }


    private void PrintOnWindows ( List<Page> printables, IPdfCreator creator, string printerName, int copiesAmount )
    {
        IEnumerable<byte []> intermediateBytes = creator.Create ( printables );

        if ( intermediateBytes != null   ||   intermediateBytes.Count () > 0 )
        {
            foreach ( byte [] pageBytes   in   intermediateBytes )
            {
                using Stream intermediateStream = new MemoryStream ( pageBytes );
                using PrintDocument pd = new PrintDocument ();

                pd.PrinterSettings.PrinterName = printerName;
                pd.PrinterSettings.Copies = ( short ) copiesAmount;

                pd.PrintPage += ( sender, args ) =>
                {
                    System.Drawing.Image img = System.Drawing.Image.FromStream ( intermediateStream );
                    args.Graphics.DrawImage ( img, args.Graphics.VisibleClipBounds );
                };

                pd.Print ();
            }
        }
    }


    private void PrintOnLinux (List<Page> printables, IPdfCreator creator, string printerName, int copiesAmount)
    {
        string pdfProxyName = @"./proxy.pdf";
        bool dataIsGenerated = creator.CreateAndSave( printables, pdfProxyName );

        if (dataIsGenerated)
        {
            string printer = printerName;
            string copies = copiesAmount.ToString();
            string bashPrintCommand = "lp -d " + printer + " -n " + copies + " " + pdfProxyName;

            ExecuteBashCommand( bashPrintCommand );
        }
    }
}