namespace Lister.Core.DataAccess.Abstractions;

public interface IPeopleSourceFactory
{
    public IPeopleSource GetPeopleSource ( string? filePath );
}