using Lister.Core.Models;

namespace Lister.Core.DataAccess.Abstractions;

public interface IPeopleSource
{
    public List<Person>? Get ( string? personsFilePath, int gettingLimit );
}