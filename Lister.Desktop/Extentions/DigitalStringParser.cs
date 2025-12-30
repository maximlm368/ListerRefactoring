namespace Lister.Desktop.Extentions;

public static class DigitalStringParser
{
    public static int ParseToInt ( string? parsable )
    {
        if ( string.IsNullOrEmpty ( parsable ) || !int.TryParse ( parsable, out int result ) )
        {
            return 0;
        }

        return result;
    }
}
