namespace Core.DataAccess.Abstractions;

public interface IPeopleSourceFactory
{
    public IPeopleSource GetPeopleSource ( string ? filePath );
}