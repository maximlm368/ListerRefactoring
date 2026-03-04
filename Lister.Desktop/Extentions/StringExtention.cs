namespace Lister.Desktop.Extentions;

internal static class StringExtention
{
    public static string TrimLastNewLineChar ( this string processable )
    {
        if ( !string.IsNullOrEmpty ( processable ) )
        {
            char lastChar = processable [^1];
            bool isGoal = lastChar == '\n';

            if ( isGoal )
            {
                processable = processable [..^1];
            }
        }

        return processable;
    }
}
