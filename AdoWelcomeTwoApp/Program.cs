using AdoWelcomeTwoApp;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Diagnostics;

string connectionString = "Data Source=Y5-0\\MSSQLSERVER01;Database=BookShop;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

using (SqlConnection connection = new(connectionString))
{
    await connection.OpenAsync();
    SqlCommand command = connection.CreateCommand();

    //await Examples.ParametersInput(command);

    SqlTransaction transaction = connection.BeginTransaction();
    command.Transaction = transaction;
    
    try
    {
        command.CommandText = @"INSERT INTO Author
                                (first_name, last_name, country)
                                VALUES
                                ( 'Антон', 'Чехов', 'Россия')";
        await command.ExecuteNonQueryAsync();

        command.CommandText = @"INSERT INTO Book
                                (author_id, title, year, price)
                                VALUES
                                ( 5, 'Отверженные', 1975, 730)";
        await command.ExecuteNonQueryAsync();


        await transaction.CommitAsync();
        Console.WriteLine("Transaction correct finished");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        await transaction.RollbackAsync();
    }
}


