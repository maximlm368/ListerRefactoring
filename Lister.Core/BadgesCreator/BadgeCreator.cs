using Lister.Core.BadgesCreator.Abstractions;
using Lister.Core.PeopleAccess.Abstractions;
using Lister.Core.Entities;
using Lister.Core.Entities.Badge;

namespace Lister.Core.BadgesCreator;

/// <summary>
/// Single class creating Badge class instance for person and template.
/// </summary>
public sealed class BadgeCreator
{
    private static BadgeCreator? _instance;
    private static IBadgeLayoutProvider? _badgeLayoutProvider;
    private static IPeopleSourceFactory? _peopleSourceFactory;

    private Layout? _badgeLayout;
    private string? _backgroundPath;
    private string? _template;

    public List<Person>? People { get; private set; }

    private BadgeCreator ( IBadgeLayoutProvider badgeApprearenceProvider, IPeopleSourceFactory peopleSourceFactory )
    {
        _badgeLayoutProvider ??= badgeApprearenceProvider;
        _peopleSourceFactory ??= peopleSourceFactory;
    }

    public static BadgeCreator GetInstance ( IBadgeLayoutProvider badgeApprearenceProvider, IPeopleSourceFactory peopleSourceFactory )
    {
        _instance ??= new BadgeCreator ( badgeApprearenceProvider, peopleSourceFactory );

        return _instance;
    }

    public Badge? CreateBadgeByTemplate ( string? templateName, Person? person )
    {
        if ( string.IsNullOrWhiteSpace ( templateName ) || person == null )
        {
            return null;
        }

        if ( _template != templateName )
        {
            _badgeLayout = _badgeLayoutProvider?.GetBadgeLayout ( templateName );
            _backgroundPath = _badgeLayoutProvider?.GetBadgeImageUri ( templateName );
            _template = templateName;
        }

        return ( _backgroundPath != null && _badgeLayout != null )
            ? Badge.GetBadge ( person, _backgroundPath, _badgeLayout.Clone ( true ) )
            : null;
    }

    public bool TrySetPeopleFrom ( string filePath, int limit )
    {
        IPeopleSource? peopleSource = _peopleSourceFactory?.GetPeopleSource ( filePath );
        List<Person>? people = peopleSource?.Get ( filePath, limit );

        if ( people != null )
        {
            People = people;

            return true;
        }

        return false;
    }
}
