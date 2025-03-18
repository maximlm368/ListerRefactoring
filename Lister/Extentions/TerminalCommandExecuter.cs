using System.Diagnostics;

namespace View.Extentions;

public static class TerminalCommandExecuter
{
    public static string ExecuteCommand ( string command )
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