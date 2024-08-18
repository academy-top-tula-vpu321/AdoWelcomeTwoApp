using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoWelcomeTwoApp
{
    static class Examples
    {
        async static public Task ParametersInput(SqlCommand command)
        {
            Author author = new() { FirstName = "Агата", LastName = "Кристи", Country = "Англия" };

            //author.LastName = @"Толстой','Россия');DELETE FROM Author WHERE id = 1";



            string commandText = @$"INSERT INTO Author
                                (first_name, last_name, country)
                                VALUES
                                ( @first_name, @last_name, @country)";

            SqlParameter firstNameParam = new("@first_name", System.Data.SqlDbType.NVarChar, 50);
            SqlParameter lastNameParam = new("@last_name", System.Data.SqlDbType.NVarChar, 50);
            SqlParameter countryParam = new("@country", System.Data.SqlDbType.NVarChar, 50);

            firstNameParam.Value = author.FirstName;
            lastNameParam.Value = author.LastName;
            countryParam.Value = author.Country;

            //command.Parameters.Add(firstNameParam);
            //command.Parameters.Add(lastNameParam);
            //command.Parameters.Add(countryParam);
            command.Parameters.AddRange(new[] { firstNameParam, lastNameParam, countryParam });

            Console.WriteLine(commandText);

            command.CommandText = commandText;

            await command.ExecuteNonQueryAsync();
        }

        async static public Task ParametersOutput(SqlCommand command)
        {
            Author author = new() { FirstName = "Чарльз", LastName = "Диккенз", Country = "Англия" };
            string commandText;

            commandText = @"INSERT INTO Author
                        (first_name, last_name, country)
                        VALUES
                        (@first_name, @last_name, @country);
                    SET @id = SCOPE_IDENTITY();";
            command.CommandText = commandText;

            SqlParameter firstNameParam = new("@first_name", System.Data.SqlDbType.NVarChar, 50);
            SqlParameter lastNameParam = new("@last_name", System.Data.SqlDbType.NVarChar, 50);
            SqlParameter countryParam = new("@country", System.Data.SqlDbType.NVarChar, 50);

            firstNameParam.Value = author.FirstName;
            lastNameParam.Value = author.LastName;
            countryParam.Value = author.Country;

            command.Parameters.AddRange(new[] { firstNameParam, lastNameParam, countryParam });

            SqlParameter idParam = new SqlParameter
            {
                ParameterName = "@id",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };
            command.Parameters.Add(idParam);

            await command.ExecuteNonQueryAsync();

            Console.WriteLine($"Id of new author: {idParam.Value}");
        }

        async static public Task StoredProcedures(SqlCommand command)
        {
            string commandText = @"CREATE PROCEDURE AuthorInsert
                                @first_name NVARCHAR(50),
                                @last_name NVARCHAR(50),
                                @country NVARCHAR(50)
                            AS

                                INSERT INTO Author
                                    (first_name, last_name, country)
                                    VALUES
                                    (@first_name, @last_name, @country)
                                SELECT SCOPE_IDENTITY()

                            GO";
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();

            commandText = @"CREATE PROCEDURE AuthorSelect
                        AS

                            SELECT * FROM Author

                    GO";
            command.CommandText = commandText;
            await command.ExecuteNonQueryAsync();

            commandText = @"CREATE PROCEDURE BookPriceRange
                                @minPrice DECIMAL(8, 2) OUT,
                                @maxPrice DECIMAL(8, 2) OUT
                            AS
                                SELECT @minPrice = MIN(price),
                                        @maxPrice = MAX(price)
                                    FROM Book";
            command.CommandText = commandText;
            command.CommandType = System.Data.CommandType.Text;

            await command.ExecuteNonQueryAsync();

            //await AuthorInsert("Марк", "Твен", "Америка", command);
            await AuthorSelect(command);
            await BookPriceRange(command);

            async Task AuthorInsert(string firstName, string lastName, string country, SqlCommand command)
            {
                string insertProc = "AuthorInsert";

                command.CommandText = insertProc;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                SqlParameter firstNameParam = new("@first_name", System.Data.SqlDbType.NVarChar, 50);
                SqlParameter lastNameParam = new("@last_name", System.Data.SqlDbType.NVarChar, 50);
                SqlParameter countryParam = new("@country", System.Data.SqlDbType.NVarChar, 50);

                firstNameParam.Value = firstName;
                lastNameParam.Value = lastName;
                countryParam.Value = country;

                command.Parameters.AddRange(new[] { firstNameParam, lastNameParam, countryParam });

                var id = await command.ExecuteScalarAsync();
                Console.WriteLine($"Id of new author: {id}");
            }

            async Task AuthorSelect(SqlCommand command)
            {
                string selectProc = "AuthorSelect";

                command.CommandText = selectProc;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            Console.Write($"{reader.GetName(i)}\t\t");
                        Console.WriteLine($"\n{new string('-', 70)}");

                        while (await reader.ReadAsync())
                        {
                            string row = "";
                            for (int i = 0; i < reader.FieldCount; i++)
                                row += reader.GetValue(i) + "\t\t";
                            Console.WriteLine(row);
                        }
                    }
                }
            }

            async Task BookPriceRange(SqlCommand command)
            {
                string bookPriceRange = "BookPriceRange";
                command.CommandText = bookPriceRange;
                command.CommandType = System.Data.CommandType.StoredProcedure;

                SqlParameter minParam = new()
                {
                    ParameterName = "@minPrice",
                    SqlDbType = System.Data.SqlDbType.Decimal,
                    Direction = System.Data.ParameterDirection.Output
                };

                SqlParameter maxParam = new()
                {
                    ParameterName = "@maxPrice",
                    SqlDbType = System.Data.SqlDbType.Decimal,
                    Direction = System.Data.ParameterDirection.Output
                };

                command.Parameters.AddRange(new[] { minParam, maxParam });

                await command.ExecuteNonQueryAsync();
                decimal minPrice = (decimal)minParam.Value;
                decimal maxPrice = (decimal)maxParam.Value;

                Console.WriteLine($"Min price = {minPrice}, Max price = {maxPrice}");
            }
        }
    }
}
