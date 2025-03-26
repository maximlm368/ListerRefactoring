using Lister.Core.BadgesCreator.Abstractions;
using Lister.Core.PeopleAccess.Abstractions;
using Lister.Core.Models;
using Lister.Core.Models.Badge;


namespace Lister.Core.BadgesCreator;

/// <summary>
/// Single class creating Badge class instance for person and template.
/// </summary>
public sealed class BadgeCreator
{
    private static BadgeCreator _instance;
    private static IBadgeLayoutProvider _badgeLayoutProvider;
    private static IPeopleSourceFactory _peopleSourceFactory;

    private List <Person> _people;
    private Layout _badgeLayout;
    private string _backgroundPath;
    private string _template;


    private BadgeCreator ( IBadgeLayoutProvider badgeApprearenceProvider, IPeopleSourceFactory peopleSourceFactory )
    {
        _people = new ();

        if ( _badgeLayoutProvider == null ) 
        {
            _badgeLayoutProvider = badgeApprearenceProvider;
        }

        if ( _peopleSourceFactory == null ) 
        {
            _peopleSourceFactory = peopleSourceFactory;
        }
    }


    public static BadgeCreator GetInstance ( IBadgeLayoutProvider badgeApprearenceProvider
                                           , IPeopleSourceFactory peopleSourceFactory )
    {
        if ( _instance == null )
        {
            _instance = new BadgeCreator ( badgeApprearenceProvider, peopleSourceFactory );
        }

        return _instance;
    }


    public Badge? CreateSingleBadgeByTemplate ( string templateName, Person person )
    {
        if ( string.IsNullOrWhiteSpace ( templateName ) )
        {
            return null;
        }

        if ( _template != templateName ) 
        {
            _badgeLayout = _badgeLayoutProvider.GetBadgeLayout ( templateName );
            _backgroundPath = _badgeLayoutProvider.GetBadgeImageUri ( templateName );
            _template = templateName;
        }

        return Badge.GetBadge ( person, _backgroundPath, _badgeLayout.Clone(true) );
    }


    public List<Person> GetPersons ( string personsFilePath )
    {
        return _people;
    }


    public List<Person> GetCurrentPeople ()
    {
        return _people;
    }


    public bool TrySetPeopleFrom ( string filePath, int limit )
    {
        IPeopleSource peopleSource = _peopleSourceFactory.GetPeopleSource ( filePath );
        List<Person> people = peopleSource.Get ( filePath, limit );

        if ( people != null )
        {
            _people = people;

            return true;
        }

        return ( people != null );
    }
}