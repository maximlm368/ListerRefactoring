using Core.Models.Badge;
using Core.Models;
using Core.DataAccess;
using Core.DataAccess.PeopleSource;
using Core.DataAccess.PeopleSource.Abstractions;
using System;


namespace Core.BadgesProvider
{
    public class BadgesGetter
    {
        private List<Person> _people;
        private Layout _badgeLayout;


        public BadgesGetter ()
        {
            _people = new ();
        }


        public List<Badge> CreateBadgesByTemplate ( string templateName )
        {
            bool argumentIsNull = string.IsNullOrEmpty ( templateName );

            if ( argumentIsNull )
            {
                throw new ArgumentNullException ( "arguments must be not null" );
            }

            List<Badge> badges = new ();
            string backgroundPath = BadgeAppearence.GetBadgeImageUri ( templateName );
            _badgeLayout = BadgeAppearence.GetBadgeLayout ( templateName );

            foreach ( var person in _people )
            {
                Layout badgeLayout = _badgeLayout.Clone ();
                Badge item = Badge.GetBadge ( person, backgroundPath, badgeLayout );

                if ( item == null )
                {
                    continue;
                }

                badges.Add ( item );
            }

            return badges;
        }


        public Badge? CreateSingleBadgeByTemplate ( string templateName, Person person )
        {
            bool argumentIsNull = string.IsNullOrEmpty ( templateName ) || ( person == null );

            if ( argumentIsNull )
            {
                throw new ArgumentNullException ( "arguments must be not null" );
            }

            Layout badgeLayout = BadgeAppearence.GetBadgeLayout ( templateName );
            string backgroundPath = BadgeAppearence.GetBadgeImageUri ( templateName );

            return Badge.GetBadge ( person, backgroundPath, badgeLayout );
        }


        public List<Person> GetPersons ( string personsFilePath )
        {
            return _people;
        }


        public bool TrySetPeopleFrom ( string filePath, int limit )
        {
            PeopleSourceBase peopleSource = PeopleSourceFactory.GetPeopleSource ( filePath );

            List<Person> people = peopleSource.Get ( filePath, limit );

            if ( people != null )
            {
                _people = people;

                return true;
            }

            return ( people != null );
        }
    }
}