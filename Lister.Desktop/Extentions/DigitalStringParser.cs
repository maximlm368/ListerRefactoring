namespace Lister.Desktop.Extentions;

public static class DigitalStringParser
{
    public static int ParseToInt(string parsable)
    {
        if (string.IsNullOrEmpty( parsable ))
        {
            return 0;
        }

        int result = 0;

        bool isInt = int.TryParse( parsable, out result );

        if (!isInt)
        {
            return 0;
        }

        return result;
    }
}