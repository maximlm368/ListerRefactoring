﻿using Lister.Core.DataAccess.Abstractions;
using Lister.Core.Models;
using System.Text;

namespace Lister.Desktop.CoreAbstractionsImplimentations.DataAccess;

public sealed class PeopleCsvSource : IPeopleSource
{
    private static PeopleCsvSource _instance;


    private PeopleCsvSource() { }


    public static PeopleCsvSource GetInstance()
    {
        if (_instance == null)
        {
            _instance = new PeopleCsvSource();
        }

        return _instance;
    }


    public List<Person>? Get(string? filePath, int gettingLimit)
    {
        List<Person> result = [];

        Encoding encoding = Encoding.GetEncoding( 1251 );

        if (filePath == null)
        {
            return null;
        }

        FileInfo fileInfo = new FileInfo( filePath );

        if (fileInfo.Exists)
        {
            using StreamReader reader = new StreamReader( filePath, encoding, true );
            string line = string.Empty;
            char separator = ';';

            int counter = 0;

            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split( separator, StringSplitOptions.TrimEntries );

                Person person = Person.Create( parts );

                if (person != null)
                {
                    result.Add( person );
                }

                counter++;

                if (counter > gettingLimit)
                {
                    return null;
                }
            }
        }
        else
        {
            return null;
        }

        return result;
    }
}