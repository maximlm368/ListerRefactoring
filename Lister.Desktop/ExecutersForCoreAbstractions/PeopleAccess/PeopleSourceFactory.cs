using Lister.Core.PeopleAccess;

namespace Lister.Desktop.ExecutersForCoreAbstractions.PeopleAccess;

/// <summary>
/// Gets people source.
/// </summary>
public sealed class PeopleSourceFactory : IPeopleSourceFactory
{
    private static PeopleSourceFactory? _instance;

    private PeopleSourceFactory ()
    {

    }

    public static PeopleSourceFactory GetInstance ()
    {
        _instance ??= new PeopleSourceFactory ();

        return _instance;
    }

    public IPeopleSource GetPeopleSource ( string? filePath )
    {
        IPeopleSource result = PeopleCsvSource.GetInstance ();

        if ( string.IsNullOrWhiteSpace ( filePath ) )
        {
            return result;
        }

        bool fileIsCSV = filePath.Last () == 'v' || filePath.Last () == 'V';

        return fileIsCSV ? PeopleCsvSource.GetInstance () : PeopleXlsxSource.GetInstance ();
    }
}
