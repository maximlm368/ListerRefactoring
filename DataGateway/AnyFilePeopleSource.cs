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


        public List<Person> GetPersons(string ? filePath)
        {
            List<Person> result = [];
            Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding (1251);
            using StreamReader reader = new StreamReader(filePath, encoding, true);
            string line = string.Empty;
            string[] parts;
            char seperator = ';';

            while ((line = reader.ReadLine()) != null)
            {
                parts = line.Split(seperator, StringSplitOptions.TrimEntries);
                Person person = Person.Create (parts);
                result.Add(person);
            }

            return result;
        }
    }
}
