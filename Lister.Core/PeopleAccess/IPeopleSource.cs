using Lister.Core.Entities;

namespace Lister.Core.PeopleAccess;

public interface IPeopleSource
{
    public List<Person>? Get ( string? personsFilePath, int gettingLimit );
}