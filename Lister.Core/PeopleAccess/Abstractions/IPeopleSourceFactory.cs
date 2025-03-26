namespace Lister.Core.PeopleAccess.Abstractions;

public interface IPeopleSourceFactory
{
    public IPeopleSource GetPeopleSource ( string? filePath );
}