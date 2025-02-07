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
        public List <Person> GetPersons ( string ? filePath, int gettingLimit )
        {
            List <Person> result = [];

            using Stream stream = File.OpenRead (filePath);
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader (stream);
            int rowCount = reader.RowCount;

            if ( rowCount > gettingLimit ) 
            {
                return null;
            }

            var conf = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = ( reader ) => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = false,
                }
            };

            var dataSet = reader.AsDataSet (conf);
            var dataTable = dataSet.Tables [0];

            for ( var i = 1;   i < dataTable.Rows.Count;   i++ )
            {
                List<string> rowData = [];

                for ( var j = 0;   j < dataTable.Columns.Count;   j++ )
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


        public List <Person> GetPersons ( string ? filePath, int gettingLimit )
        {
            List <Person> result = [];

            Encoding encoding = Encoding.GetEncoding (1251);
            using StreamReader reader = new StreamReader (filePath, encoding, true);
            string line = string.Empty;
            char separator = ';';

            int counter = 0;

            while ( ( line = reader.ReadLine () ) != null )
            {
                string [] parts = line.Split (separator, StringSplitOptions.TrimEntries);

                Person person = Person.Create (parts);

                if ( person != null ) 
                {
                    result.Add (person);
                }

                counter++;

                if ( counter > gettingLimit ) 
                {
                    return null;
                }
            }

            return result;
        }


        //public bool CheckSize ( string? filePath, int limit ) 
        //{
        //    if ( string.IsNullOrWhiteSpace (filePath) )
        //    {
        //        return false;
        //    }

        //    Encoding encoding = Encoding.GetEncoding (1251);
        //    using StreamReader reader = new StreamReader (filePath, encoding, true);
        //    string line = string.Empty;
        //    char separator = ';';

        //    int counter = 0;

        //    while ( ( line = reader.ReadLine () ) != null )
        //    {
        //        counter++;

        //        if ( counter > limit )
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}
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
