using Lister.Core.Entities;

namespace Lister.Core.Document.AbstractComponents;

public interface IPeopleSource
{
    public List<Person>? Get ( string? personsFilePath, int gettingLimit );
}
