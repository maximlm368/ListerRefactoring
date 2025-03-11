using Core.Models;
using System.Data;
using ExcelDataReader;
using Core.DataAccess.Abstractions;

namespace View.CoreAbstractionsImplimentations.DataAccess;

public sealed class PeopleXlsxSource : IPeopleSource
{
    private static PeopleXlsxSource _instance;


    private PeopleXlsxSource() { }


    public static PeopleXlsxSource GetInstance()
    {
        if (_instance == null)
        {
            _instance = new PeopleXlsxSource();
        }

        return _instance;
    }


    public List<Person>? Get(string? filePath, int gettingLimit)
    {
        List<Person> result = [];

        if (filePath == null)
        {
            return null;
        }

        FileInfo fileInfo = new FileInfo( filePath );

        if (fileInfo.Exists)
        {
            using Stream stream = File.OpenRead( filePath );
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader( stream );
            int rowCount = reader.RowCount;

            if (rowCount > gettingLimit)
            {
                return null;
            }

            ExcelDataSetConfiguration conf = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = (reader) => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = false,
                }
            };

            DataSet dataSet = reader.AsDataSet( conf );
            DataTable dataTable = dataSet.Tables[0];

            for (var i = 1; i < dataTable.Rows.Count; i++)
            {
                List<string> rowData = [];

                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    var data = dataTable.Rows[i][j];
                    rowData.Add( data.ToString() );
                }

                Person person = Person.Create( rowData.ToArray() );

                if (person != null)
                {
                    result.Add( person );
                }
            }
        }
        else
        {
            return null;
        }

        return result;
    }


    public List<string> GetRow(string filePath, int rowNumZeroBased)
    {
        List<string> resultRow = [];

        bool extentionIsXSLX = filePath.Last() == 'x';

        if (extentionIsXSLX)
        {
            using Stream stream = File.OpenRead( filePath );
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader( stream );
            int rowCounts = reader.RowCount;

            ExcelDataSetConfiguration conf = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = (reader) => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = false,
                }
            };

            DataSet dataSet = reader.AsDataSet( conf );
            DataTable dataTable = dataSet.Tables[0];

            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    object data = dataTable.Rows[i][j];
                    resultRow.Add( data.ToString() );
                }
            }
        }

        return resultRow;
    }
}