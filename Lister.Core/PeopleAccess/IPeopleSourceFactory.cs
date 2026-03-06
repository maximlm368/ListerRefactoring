namespace Lister.Core.PeopleAccess;

public interface IPeopleSourceFactory
{
    public IPeopleSource GetPeopleSource ( string? filePath );
}