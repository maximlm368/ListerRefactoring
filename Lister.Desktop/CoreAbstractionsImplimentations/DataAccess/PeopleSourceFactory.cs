using Lister.Core.DataAccess.Abstractions;

namespace Lister.Desktop.CoreAbstractionsImplimentations.DataAccess;

public class PeopleSourceFactory : IPeopleSourceFactory
{
    private static PeopleSourceFactory _instance;


    private PeopleSourceFactory() { }


    public static PeopleSourceFactory GetInstance()
    {
        if (_instance == null)
        {
            _instance = new PeopleSourceFactory();
        }

        return _instance;
    }


    public IPeopleSource GetPeopleSource(string? filePath)
    {
        IPeopleSource result = PeopleCsvSource.GetInstance();

        if (string.IsNullOrWhiteSpace( filePath ))
        {
            return result;
        }

        bool fileIsCSV = filePath.Last() == 'v'
                         ||
                         filePath.Last() == 'V';

        if (fileIsCSV)
        {
            result = PeopleCsvSource.GetInstance();
        }
        else
        {
            result = PeopleXlsxSource.GetInstance();
        }

        return result;
    }
}