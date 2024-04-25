using SkiaSharp;
using System;
using System.IO;

namespace ContentAssembler
{
    public class Person
    {
        public string id { get; private set; }
        public string firstName { get; private set; }
        public string middleName { get; private set; }
        public string lastName { get; private set; }
        public string department { get; private set; }
        public string position { get; private set; }


        public Person(string id, string firstName, string middleName, string lastName,  string department, string position)
        {
            this.id = id;
            this.firstName = firstName;
            this.middleName = middleName;
            this.lastName = lastName;
            this.department = department;
            this.position = position;

           
        }



        public override string ToString()
        {
            return firstName + " " + middleName + " " + lastName;
        }

    }



}