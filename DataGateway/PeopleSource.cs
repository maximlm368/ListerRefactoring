using ContentAssembler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using ExtentionsAndAuxiliary;
using System.Collections.Generic;
using ExcelDataReader;
using ExcelDataReader.Core;
//using DocumentFormat.OpenXml.Drawing.Charts;
using System.Data;

namespace DataGateway
{
    public class PeopleXlsxSource : IPeopleSource, IRowSource
    {
        public List <Person> GetPersons ( string? filePath )
        {
            List<Person> result = [];

            if ( string.IsNullOrWhiteSpace (filePath) )
            {
                return result;
            }

            using Stream stream = File.OpenRead (filePath);
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader (stream);
            int rowCounts = reader.RowCount;

            var conf = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = ( reader ) => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = false,
                }
            };

            var dataSet = reader.AsDataSet (conf);
            var dataTable = dataSet.Tables [0];

            for ( var i = 0; i < dataTable.Rows.Count; i++ )
            {
                List<string> rowData = [];

                for ( var j = 0; j < dataTable.Columns.Count; j++ )
                {
                    var data = dataTable.Rows [i] [j];
                    rowData.Add (data.ToString ());
                }

                Person person = Person.Create (rowData.ToArray ());

                if ( person != null )
                {
                    result.Add (person);
                }
            }

            IComparer <Person> comparer = new RusStringComparer<Person> ();
            result.Sort (comparer);

            return result;
        }


        public List<string> GetRow ( string filePath, int rowNumZeroBased )
        {
            List<string> resultRow = [];

            bool extentionIsXSLX = ( filePath.Last () == 'x' );

            if ( extentionIsXSLX )
            {
                using Stream stream = File.OpenRead ( filePath );
                using IExcelDataReader reader = ExcelReaderFactory.CreateReader (stream);
                int rowCounts = reader.RowCount;

                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = (reader) => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = false,
                    }
                };

                DataSet dataSet = reader.AsDataSet (conf);
                System.Data.DataTable dataTable = dataSet.Tables [0];

                for ( int i = 0;   i < 1;   i++ )
                {
                    for ( int j = 0;   j < dataTable.Columns.Count;   j++ )
                    {
                        object data = dataTable.Rows [i] [j];
                        resultRow.Add (data.ToString ());
                    }
                }
            }

            return resultRow;
        }
    }



    public class PeopleCsvSource : IPeopleSource
    {
        public PeopleCsvSource () { }


        public List <Person> GetPersons ( string ? filePath )
        {
            List <Person> result = [];

            if ( string.IsNullOrWhiteSpace (filePath) )
            {
                return result;
            }

            Encoding encoding = Encoding.GetEncoding (1251);
            using StreamReader reader = new StreamReader (filePath, encoding, true);
            string line = string.Empty;
            char separator = ';';

            while ( ( line = reader.ReadLine () ) != null )
            {
                string [] parts = line.Split (separator, StringSplitOptions.TrimEntries);

                Person person = Person.Create (parts);

                if ( person != null ) 
                {
                    result.Add (person);
                }
            }

            IComparer<Person> comparer = new RusStringComparer<Person> ();
            result.Sort (comparer);

            return result;
        }
    }



    public class PeopleSourceFactory : IPeopleSourceFactory
    {
        public PeopleSourceFactory () { }


        public IPeopleSource GetPeopleSource ( string ? filePath )
        {
            IPeopleSource result = new PeopleCsvSource ();

            if ( string.IsNullOrWhiteSpace (filePath) )
            {
                return result;
            }

            bool fileIsCSV = (( filePath.Last () == 'v' )   ||   ( filePath.Last () == 'V' ));

            if ( fileIsCSV )
            {
                result = new PeopleCsvSource ();
            }
            else
            {
                result = new PeopleXlsxSource ();
            }

            return result;
        }
    }



    public interface IRowSource 
    {
        public List<string> GetRow ( string filePath, int rowNumZeroBased );
    }
}
