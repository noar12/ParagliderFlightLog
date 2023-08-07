using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
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
		public bool DbExists(string connectionString)
		{
			string pattern = @"(?<=Data Source=).*?(?=;)";
			Regex rgx = new Regex(pattern);
			Match match = rgx.Match(connectionString);
			

			return File.Exists(match.Value);
		}
		public List<T> LoadData<T, U>(string sqlStatement, U parameters, string connectionString)
		{
			// T is the model of the data we want to load
			using (IDbConnection connection = new SQLiteConnection(connectionString))// this will always close the connection when exiting the scope
			{
				List<T> rows = connection.Query<T>(sqlStatement, parameters).ToList();
				return rows;
			}
		}
		public void SaveData<T>(string sqlStatement, T parameters, string connectionString)
		{
			using (IDbConnection connection = new SQLiteConnection(connectionString))
			{
				connection.Execute(sqlStatement, parameters);
			}
		}
	}
}
