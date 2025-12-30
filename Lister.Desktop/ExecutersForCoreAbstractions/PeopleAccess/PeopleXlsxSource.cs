using Lister.Core.Models;
using Lister.Core.PeopleAccess.Abstractions;
using System.Data;
using ExcelDataReader;

namespace Lister.Desktop.ExecutersForCoreAbstractions.PeopleAccess;

/// <summary>
/// Gets people from (xlsx.) file.
/// </summary>
public sealed class PeopleXlsxSource : IPeopleSource
{
    private static PeopleXlsxSource? _instance;

    private PeopleXlsxSource ()
    {

    }

    public static PeopleXlsxSource GetInstance ()
    {
        _instance ??= new PeopleXlsxSource ();

        return _instance;
    }

    public List<Person>? Get ( string? filePath, int gettingLimit )
    {
        List<Person> result = [];

        if ( filePath == null )
        {
            return null;
        }

        FileInfo fileInfo = new ( filePath );

        if ( fileInfo.Exists )
        {
            using Stream stream = File.OpenRead ( filePath );
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader ( stream );
            int rowCount = reader.RowCount;

            if ( rowCount > gettingLimit )
            {
                return null;
            }

            ExcelDataSetConfiguration conf = new ()
            {
                ConfigureDataTable = ( reader ) => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = false,
                }
            };

            DataSet dataSet = reader.AsDataSet ( conf );
            DataTable dataTable = dataSet.Tables [0];

            for ( var i = 1; i < dataTable.Rows.Count; i++ )
            {
                List<string> rowData = [];

                for ( var j = 0; j < dataTable.Columns.Count; j++ )
                {
                    var data = dataTable.Rows [i] [j];

                    string? strData = data.ToString ();

                    if ( strData != null )
                    {
                        rowData.Add ( strData );
                    }
                }

                Person? person = Person.Create ( [.. rowData] );

                if ( person != null )
                {
                    result.Add ( person );
                }
            }
        }
        else
        {
            return null;
        }

        return result;
    }


    public static List<string> GetRow ( string filePath )
    {
        List<string> resultRow = [];
        bool isXSLX = filePath.Last () == 'x';

        if ( isXSLX )
        {
            using Stream stream = File.OpenRead ( filePath );
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader ( stream );
            int rowCounts = reader.RowCount;

            ExcelDataSetConfiguration conf = new ()
            {
                ConfigureDataTable = ( reader ) => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = false,
                }
            };

            DataSet dataSet = reader.AsDataSet ( conf );
            DataTable dataTable = dataSet.Tables [0];

            for ( int i = 0; i < 1; i++ )
            {
                for ( int j = 0; j < dataTable.Columns.Count; j++ )
                {
                    object data = dataTable.Rows [i] [j];

                    string? strData = data.ToString ();

                    if ( strData != null ) 
                    {
                        resultRow.Add ( strData );
                    } 
                }
            }
        }

        return resultRow;
    }
}
