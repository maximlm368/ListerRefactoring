using Core.Models;

namespace Core.DataAccess.PeopleSource.Abstractions;

public abstract class PeopleSourceBase
{
    public abstract List<Person>? Get ( string? personsFilePath, int gettingLimit );
}