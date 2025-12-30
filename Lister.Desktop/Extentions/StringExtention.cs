namespace Lister.Desktop.Extentions;

internal static class StringExtention
{
    public static string TrimLastNewLineChar ( this string beingProcessed )
    {
        if ( !string.IsNullOrEmpty ( beingProcessed ) )
        {
            char lastChar = beingProcessed [^1];
            bool isGoal = lastChar == '\n';

            if ( isGoal )
            {
                beingProcessed = beingProcessed [..^1];
            }
        }

        return beingProcessed;
    }
}
