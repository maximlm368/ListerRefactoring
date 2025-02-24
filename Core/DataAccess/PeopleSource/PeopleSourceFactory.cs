using Core.DataAccess.PeopleSource.Abstractions;

namespace Core.DataAccess.PeopleSource
{
    public static class PeopleSourceFactory
    {
        public static PeopleSourceBase GetPeopleSource ( string? filePath )
        {
            PeopleSourceBase result = new PeopleCsvSource ();

            if ( string.IsNullOrWhiteSpace ( filePath ) )
            {
                return result;
            }

            bool fileIsCSV = ( ( filePath.Last () == 'v' ) || ( filePath.Last () == 'V' ) );

            if ( fileIsCSV )
            {
                result = new PeopleCsvSource ();
            }
            else
            {
                result = new PeopleXlsxSource ();
            }

            return result;
        }
    }
}