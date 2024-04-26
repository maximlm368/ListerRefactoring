using System;
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

        public List<FileInfo> GetBadgeModels ();

        public void GeneratePdf ();
    }



    public class UniformDocAssembler : IUniformDocumentAssembler
    {
        private IResultOfSessionSaver converter;
        private IBadgeAppearenceDataSource badgeAppearenceDataSource;
        private IPeopleDataSource peopleDataSource;
        private List<Person> people;


        public UniformDocAssembler(IResultOfSessionSaver converter
                                      , IBadgeAppearenceDataSource badgeAppearenceDataSource
                                                          , IPeopleDataSource peopleDataSource)
        {
            this.converter = converter;
            this.badgeAppearenceDataSource = badgeAppearenceDataSource;
            this.peopleDataSource = peopleDataSource;
            people = new List<Person>();
        }


        public List<Badge> CreateBadgesByModel ( string badgeModelName )
        {
            if ( badgeModelName == null ) 
            {
                throw new ArgumentNullException ("arguments must be not null");
            }

            List<Badge> badges = new List<Badge>();
            OrganizationalDataOfBadge badgeOrganizationalData = badgeAppearenceDataSource.GetBadgeData(badgeModelName);

            foreach ( var person in people ) 
            {    
                Badge item = new Badge (person, badgeModelName, badgeOrganizationalData);
                badges.Add (item);
            }

            return badges;
        }


        public Badge CreateSingleBadgeByModel(string badgeModelName, Person person) 
        {
            if (badgeModelName == null)
            {
                throw new ArgumentNullException("arguments must be not null");
            }

            if (person == null)
            {
                throw new ArgumentNullException("arguments must be not null");
            }

            OrganizationalDataOfBadge badgeOrganizationalData = badgeAppearenceDataSource.GetBadgeData(badgeModelName);

            Badge badge = new Badge ( person, badgeModelName, badgeOrganizationalData );

            return badge;
        }

        public Badge CreateSingleBadgeByModel(string badgeModelName)
        {
            throw new NotImplementedException();
        }


        public void GeneratePdf ()
        {
            converter.ConvertToExtention (null, null);
        }


        public List<FileInfo> GetBadgeModels()
        {
            return badgeAppearenceDataSource.GetBadgeModelsNames();
        }


        public List<Person> GetPersons(string ? personsFilePath) 
        {
            people = peopleDataSource.GetPersons(personsFilePath);
            return people;
        }


    }
}
