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
        private static BadgesGetter _instance;

        private List <Person> _people;
        private Layout _badgeLayout;
        private string _backgroundPath;
        private string _template;


        public BadgesGetter ()
        {
            _people = new ();
        }


        public static BadgesGetter GetInstance ()
        {
            if ( _instance == null )
            {
                _instance = new BadgesGetter ();
            }

            return _instance;
        }


        public Badge ? CreateSingleBadgeByTemplate ( string templateName, Person person )
        {
            if ( string.IsNullOrWhiteSpace ( templateName ) )
            {
                return null;
            }

            if ( _template != templateName ) 
            {
                _badgeLayout = BadgeAppearence.GetBadgeLayout ( templateName );
                _backgroundPath = BadgeAppearence.GetBadgeImageUri ( templateName );
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