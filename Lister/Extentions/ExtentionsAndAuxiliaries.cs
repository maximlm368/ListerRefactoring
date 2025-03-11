using Avalonia.Media.Imaging;
using View.CoreModelReflection.Badge;
using System.Diagnostics;

namespace View.Extentions;

internal class BadgeComparer : IComparer <BadgeViewModel>
{
    public int Compare ( BadgeViewModel ? x, BadgeViewModel ? y )
    {
        if ((x==null)  ||  (y==null)) 
        {
            return 0;
        }

        string xStringPresentation = x.Model.Person.FullName;
        string yStringPresentation = y.Model.Person.FullName;

        return string.Compare (xStringPresentation, yStringPresentation);
    }
}



internal static class ListExtention
{
    public static List <T> Clone <T> ( this List <T> source )
    {
        List <T> result = new ();

        if ( source == null )
        {
            return result;
        }

        for ( int index = 0;   index < source.Count;   index++ )
        {
            result.Add ( source[index] );
        }

        return result;
    }
}



internal static class ImageHelper
{
    public static Bitmap ? LoadFromResource (string source)
    {
        Bitmap bitmap = null;

        if ( File.Exists (source) ) 
        {
            bitmap = new Bitmap (source);
        }

        return bitmap;
    }
}



internal static class StringExtention 
{
    public static string TrimLastNewLineChar ( this string beingProcessed )
    {
        if ( ! string.IsNullOrEmpty ( beingProcessed ) )
        {
            char lastChar = beingProcessed [beingProcessed.Length - 1];
            bool isGoal = ( lastChar == '\n' );

            if ( isGoal )
            {
                beingProcessed = beingProcessed.Substring ( 0, beingProcessed.Length - 1 );
            }
        }

        return beingProcessed;
    }
}



public static class DigitalStringParser
{
    public static int ParseToInt ( string parsable )
    {
        if ( string.IsNullOrEmpty ( parsable ) )
        {
            return 0;
        }

        int result = 0;

        bool isInt = int.TryParse ( parsable, out result );

        if ( ! isInt )
        {
            return 0;
        }

        return result;
    }
}



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