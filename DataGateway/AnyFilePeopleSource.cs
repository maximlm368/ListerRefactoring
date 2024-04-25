using ContentAssembler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGateway
{
    public class AnyFilePeopleSource : IPeopleDataSource
    {
        public string sourcePath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        public AnyFilePeopleSource() { }


        public List<Person> GetPersons(string? personsFilePath)
        {
            List<Person> result = new List<Person>();
            Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
            Encoding utf8 = Encoding.GetEncoding ("windows-1251");
            StreamReader reader = new StreamReader(personsFilePath, utf8, true);
            string allFeaturesOfOnePerson = "";
            string[] splittedFeaturesOfOnePerson;
            char[] seperators = { ';' };

            while ((allFeaturesOfOnePerson = reader.ReadLine()) != null)
            {
                splittedFeaturesOfOnePerson = allFeaturesOfOnePerson.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                
                string id = "";
                string firstName = "";
                string middleName = "";
                string lastName = "";
                string department = "";
                string position = "";

                for (int featureCounter = 0;   featureCounter < splittedFeaturesOfOnePerson.Length;   featureCounter++)
                {
                    switch (featureCounter)
                    {
                        case 0: id = splittedFeaturesOfOnePerson[featureCounter]; break;
                        case 1: lastName = splittedFeaturesOfOnePerson[featureCounter]; break;
                        case 2: firstName = splittedFeaturesOfOnePerson[featureCounter]; break;
                        case 3: middleName = splittedFeaturesOfOnePerson[featureCounter]; break;
                        case 4: department = splittedFeaturesOfOnePerson[featureCounter]; break;
                        case 5: position = splittedFeaturesOfOnePerson[featureCounter]; break;
                    }
                }

                Person person = new Person(id, firstName, middleName, lastName, department, position);
                result.Add(person);
            }

            return result;
        }
    }
}
