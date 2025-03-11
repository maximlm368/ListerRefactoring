using Core.Models;

namespace Core.DataAccess.Abstractions;

public interface IPeopleSource
{
    public List<Person> ? Get(string ? personsFilePath, int gettingLimit);
}