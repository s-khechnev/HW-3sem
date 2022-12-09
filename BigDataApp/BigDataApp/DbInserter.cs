using System.Data;
using System.Reflection;
using ConsoleTables;
using Npgsql;
using Z.BulkOperations;

namespace BigDataApp.Entities;

public static class DbInserter
{
    public static bool Insert<T>(ICollection<T> records)
    {
        bool isInserted = false;

        using (var connection = GetPostreSqlConnection())
        {
            /*connection.Open();
            string query = "INSERT INTO "Movies"(Id, Rating) VALUES (@value1, @value2)";
            NpgsqlCommand command = new NpgsqlCommand(query, connection);
            command.ExecuteNonQuery();*/
            /*command.ExecuteNonQuery();
            foreach (var record in records)
            {
                var movie = record as Movie;
                string query = "INSERT INTO Movies(Id, Rating) VALUES (@value1, @value2)";
                NpgsqlCommand command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@value1", movie.Id);
                command.Parameters.AddWithValue("@value2", movie.Rating);
                command.ExecuteNonQuery();

            }*/
        }

        return isInserted;
    }

    private static NpgsqlConnection GetPostreSqlConnection()
    {
        return new NpgsqlConnection(new NpgsqlConnectionStringBuilder()
        {
            Host = "localhost",
            Port = 5432,
            Database = "bigDataAppDB",
            Username = "test",
            Password = "12345"
        }.ToString());
    }

    public static DataTable GetTable<T>(ICollection<T> records)
    {
        DataTable dataTable = new();

        var type = typeof(T);

        foreach (var record in records)
        {
            dataTable.Rows.Add(dataTable.NewRow());
        }

        var allowableProp = new Type[] { typeof(string), typeof(int), typeof(float) };

        foreach (var propertyInfo in type.GetProperties().Where(propertyInfo => allowableProp.Contains(propertyInfo.PropertyType)))
        {
            DataColumn column = new(propertyInfo.Name);

            column.DataType = propertyInfo.PropertyType;

            dataTable.Columns.Add(column);

            int rowIndex = 0;

            foreach (var record in records)
            {
                DataRow row = dataTable.Rows[rowIndex];
                var item = propertyInfo.GetValue(record);
                row[propertyInfo.Name] = item;
            }
        }

        var table = new ConsoleTable("id", "Rating");
        
        for (int i = 0; i < 100; i++)
        {
            var t = dataTable.Rows[i];
            table.AddRow(t[0], t[1]);
        }
        
        table.Write();


        return dataTable;
    }


}