using ContentAssembler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtentionsAndAuxiliary;
using System.Collections.Generic;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace DataGateway
{
    public class PeopleXlsxSource : IPeopleSource, IRowSource
    {
        private readonly int _xslxStartRow = 2;
        private readonly int _xslxWorkSheetNum = 1;


        public PeopleXlsxSource() { }


        //public List <Person> GetPersons ( string ? filePath )
        //{
        //    List<Person> result = [];

        //    if ( string.IsNullOrWhiteSpace (filePath) )
        //    {
        //        return result;
        //    }

        //    if ( ( filePath.Last () == 'v' ) || ( filePath.Last () == 'V' ) )
        //    {
        //        result = GetPersonsFromCSV (filePath);
        //    }
        //    else 
        //    {
        //        result = GetPersonsFromXLSX (filePath);
        //    }

        //    return result;
        //}


        public List <Person> GetPersons (string ? filePath)
        {
            List <Person> result = [];

            if ( string.IsNullOrWhiteSpace (filePath) )
            {
                return result;
            }

            Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
            //Encoding encoding = Encoding.GetEncoding (1251);

            using  XLWorkbook workbook = new XLWorkbook (filePath);
            IXLWorksheet worksheet = workbook.Worksheet (_xslxWorkSheetNum);

            int usefullColumnCount = worksheet.Columns ().Count ();
            int rowCount = worksheet.Rows ().Count ();

            for ( int index = _xslxStartRow;   index <= rowCount;   index++ ) 
            {
                IXLRow row = worksheet.Row (index);
                List <string> rowData = new ();

                for ( int counter = 1;   counter <= usefullColumnCount;   counter++ ) 
                {
                    IXLCell cell = row.Cell (counter);
                    string value = cell.Value.ToString ();
                    rowData.Add (value);
                }

                try
                {
                    Person person = Person.Create (rowData.ToArray());
                    result.Add (person);
                }
                catch ( ArgumentException ex ){}
            }

            IComparer <Person> comparer = new RusStringComparer <Person> ();
            result.Sort ( comparer );

            return result;
        }


        public List <string> GetRow ( string filePath, int rowNumZeroBased )
        {
            List<string> resultRow = new ();

            bool extentionIsXSLX = (filePath.Last () == 'x');

            if ( extentionIsXSLX )
            {
                Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
                //Encoding encoding = Encoding.GetEncoding (1251);

                using XLWorkbook workbook = new XLWorkbook (filePath);
                IXLWorksheet worksheet = workbook.Worksheet (_xslxWorkSheetNum);

                int usefullColumnCount = worksheet.Columns ().Count ();
                int rowCount = worksheet.Rows ().Count ();

                if ( rowCount < 1 ) 
                {
                    return resultRow;
                }

                IXLRow row = worksheet.Row (rowNumZeroBased + 1);

                for ( int counter = 1;   counter <= usefullColumnCount;   counter++ )
                {
                    IXLCell cell = row.Cell (counter);
                    string value = cell.Value.ToString ();
                    resultRow.Add (value);
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

            Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding (1251);
            using StreamReader reader = new StreamReader (filePath, encoding, true);
            string line = string.Empty;
            char separator = ';';

            while ( ( line = reader.ReadLine () ) != null )
            {
                string [] parts = line.Split (separator, StringSplitOptions.TrimEntries);

                try
                {
                    Person person = Person.Create (parts);
                    result.Add (person);
                }
                catch ( ArgumentException ex ) { }
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
