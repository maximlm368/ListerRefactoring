using Lister.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lister.ViewModels
{
    internal class LinuxPrinter
    {
        //public void Print ( List <PageViewModel> printables, PrintAdjustingData printAdjusting )
        //{
        //    string pdfProxyName = @"./proxy.pdf";

        //    bool dataIsGenerated = GeneratePdf (pdfProxyName, printables);

        //    if ( dataIsGenerated )
        //    {
        //        string printer = printAdjusting.PrinterName;
        //        string copies = printAdjusting.CopiesAmount.ToString ();

        //        string bashPrintCommand = "lp -d " + printer + " -n " + copies + " " + pdfProxyName;

        //        ExecuteBashCommand (bashPrintCommand);
        //    }
        //}


        private string ExecuteBashCommand ( string command )
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
    }
}
