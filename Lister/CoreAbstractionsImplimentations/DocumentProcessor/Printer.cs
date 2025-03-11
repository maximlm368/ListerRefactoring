using Core.DocumentProcessor;
using Core.DocumentProcessor.Abstractions;
using System.Diagnostics;
using System.Drawing.Printing;

namespace View.CoreAbstractionsImplimentations.DocumentProcessor;

public class Printer : IPdfPrinter
{
    private static Printer _instance = null;

    private string _osName;
    private IPdfCreator _pdgCreator;


    private Printer(string osName, IPdfCreator pdfCreator)
    {
        _osName = osName;
        _pdgCreator = pdfCreator;
    }


    internal static Printer GetInstance(string osName, IPdfCreator pdfCreator)
    {
        Printer instance = _instance ?? new Printer( osName, pdfCreator );

        return instance;
    }


    public void Print(List<Page> printables, IPdfCreator creator, string printerName, int copiesAmount)
    {
        if (_osName == "Windows")
        {
            PrintOnWindows( printables, creator, printerName, copiesAmount );
        }
        else if (_osName == "Linux")
        {
            PrintOnLinux( printables, creator, printerName, copiesAmount );
        }
    }


    private void PrintOnWindows(List<Page> printables, IPdfCreator creator, string printerName, int copiesAmount)
    {
        IEnumerable<byte[]> intermediateBytes = creator.Create( printables );

        if (intermediateBytes != null   ||   intermediateBytes.Count() > 0)
        {
            foreach (byte[] pageBytes   in   intermediateBytes)
            {
                using Stream intermediateStream = new MemoryStream( pageBytes );
                using PrintDocument pd = new PrintDocument();

                pd.PrinterSettings.PrinterName = printerName;
                pd.PrinterSettings.Copies = (short)copiesAmount;

                pd.PrintPage += (sender, args) =>
                {
                    System.Drawing.Image img = System.Drawing.Image.FromStream( intermediateStream );
                    args.Graphics.DrawImage( img, args.Graphics.VisibleClipBounds );
                };

                pd.Print();
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


    private string ExecuteBashCommand(string command)
    {
        using (Process process = new Process())
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();

            string result = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            return result;
        }
    }
}