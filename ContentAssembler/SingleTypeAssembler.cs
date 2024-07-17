﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Unicode;


namespace ContentAssembler
{
    public interface IUniformDocumentAssembler
    {
        /// <summary>
        /// Assembles document consists pages that consist one kind badges
        /// </summary>
        /// <param name="personsFilePath">Path to file contains list of persons</param>
        /// <param name="badgeModelName">Kind of badge</param>
        /// <returns></returns>
        public List<Badge> CreateBadgesByModel (string badgeModelName);

        public Badge CreateSingleBadgeByModel (string badgeModelName, Person person);

        public List<Person> GetPersons (string? personsFilePath);

        public List <TemplateName> GetBadgeModels ( );

        public void GeneratePdf ( );
    }



    public class UniformDocAssembler : IUniformDocumentAssembler
    {
        private IResultOfSessionSaver _converter;
        private IBadgeAppearenceProvider _badgeAppearenceProvider;
        private IPeopleDataSource _peopleDataSource;
        private List<Person> _people;
        private BadgeLayout _badgeLayout;


        public UniformDocAssembler (IBadgeAppearenceProvider badgeAppearenceDataSource, IPeopleDataSource peopleDataSource)
        {
            this._badgeAppearenceProvider = badgeAppearenceDataSource;
            this._peopleDataSource = peopleDataSource;
            _people = new List<Person>();
        }


        public List <Badge> CreateBadgesByModel ( string templateName )
        {
            bool argumentIsNull = string.IsNullOrEmpty (templateName);

            if ( argumentIsNull ) 
            {
                throw new ArgumentNullException ( "arguments must be not null" );
            }

            List<Badge> badges = new ();
            string backgroundPath = _badgeAppearenceProvider.GetBadgeBackgroundPath ( templateName );
            _badgeLayout = _badgeAppearenceProvider.GetBadgeLayout (templateName);

            foreach ( var person   in   _people ) 
            {
                BadgeLayout badgeLayout = _badgeLayout.Clone ();
                Badge item = new Badge (person, backgroundPath, badgeLayout);
                badges.Add (item);
            }

            return badges;
        }


        public Badge CreateSingleBadgeByModel(string templateName, Person person) 
        {
            bool argumentIsNull = string.IsNullOrEmpty(templateName)   ||   (person == null);

            if (argumentIsNull)
            {
                throw new ArgumentNullException("arguments must be not null");
            }

            BadgeLayout badgeLayout = _badgeAppearenceProvider.GetBadgeLayout(templateName);
            string backgroundPath = _badgeAppearenceProvider.GetBadgeBackgroundPath ( templateName );
            Badge badge = new Badge ( person, backgroundPath, badgeLayout );

            return badge;
        }

        public Badgeee CreateSingleBadgeByModel(string badgeModelName)
        {
            throw new NotImplementedException();
        }


        public void GeneratePdf ()
        {
            //converter.ConvertToExtention (null, null);
        }


        public List <TemplateName> GetBadgeModels ( )
        {
            return _badgeAppearenceProvider.GetBadgeTemplateNames ( );
        }


        public List <Person> GetPersons (string ? personsFilePath) 
        {
            _people = _peopleDataSource.GetPersons(personsFilePath);
            return _people;
        }


    }
}
