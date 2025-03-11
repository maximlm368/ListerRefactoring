using Core.Models.Badge;


namespace Core.BadgesCreator.Abstractions;

/// <summary>
/// Define abstraction implementation of wich returns layout (in state without personal details) for template.
/// </summary>
public interface IBadgeLayoutProvider
{
    public string GetBadgeImageUri ( string templateName );

    public Layout GetBadgeLayout ( string templateName );
}