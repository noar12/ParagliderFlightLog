using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ParagliderFlightLog.DataAccess
{
	public class SqliteDataAccess
	{
#pragma warning disable CA1822 // Mark members as static: don't want this to be static
		public bool DbExists(string connectionString)
		{
			string pattern = @"(?<=Data Source=).*?(?=;)";
			var rgx = new Regex(pattern);
			Match match = rgx.Match(connectionString);
			

			return File.Exists(match.Value);
		}
		public List<T> LoadData<T, U>(string sqlStatement, U parameters, string connectionString)
		{
            // T is the model of the data we want to load
            using IDbConnection connection = new SqliteConnection(connectionString);// this will always close the connection when exiting the scope
            List<T> rows = connection.Query<T>(sqlStatement, parameters).ToList();
            return rows;
        }
        public async Task<List<T>> LoadDataAsync<T, U>(string sqlStatement, U parameters, string connectionString)
        {
            // T is the model of the data we want to load
            using IDbConnection connection = new SqliteConnection(connectionString);// this will always close the connection when exiting the scope
            List<T> rows = (await connection.QueryAsync<T>(sqlStatement, parameters)).ToList();
            return rows;
        }
        public void SaveData<T>(string sqlStatement, T parameters, string connectionString)
		{
            using IDbConnection connection = new SqliteConnection(connectionString);
            connection.Execute(sqlStatement, parameters);
        }
#pragma warning restore CA1822 // Mark members as static
	}
}
