using Microsoft.Data.Sqlite;
using System.Data;
using Dapper;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace ParagliderFlightLog.DataAccess
{
    /// <summary>
    /// Wrap the usage of Dapper to access a SQLite database
    /// </summary>
    public class SqliteDataAccess
    {
        private readonly ILogger<SqliteDataAccess> _logger;
#pragma warning disable CA1822 // Mark members as static: don't want this to be static
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        public SqliteDataAccess(ILogger<SqliteDataAccess> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Return true if a db file corresponding to the <param name="connectionString"></param> exists
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static bool DbExists(string connectionString)
        {
            return File.Exists(GetDbPathFromConnectionString(connectionString));
        }

        /// <summary>
        /// return the path of the connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static string GetDbPathFromConnectionString(string connectionString)
        {
            string pattern = @"(?<=Data Source=).*?(?=;)";
            var rgx = new Regex(pattern);
            Match match = rgx.Match(connectionString);
            return match.Value;
        }

        /// <summary>
        /// Load data from a SQLite database
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionString"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public List<T> LoadData<T, U>(string sqlStatement, U parameters, string connectionString)
        {
            // T is the model of the data we want to load
            using IDbConnection
                connection =
                    new SqliteConnection(
                        connectionString); // this will always close the connection when exiting the scope
            List<T> rows = connection.Query<T>(sqlStatement, parameters).ToList();
            return rows;
        }

        /// <summary>
        /// Load data from a SQLite database asynchronously
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionString"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        public async Task<List<T>> LoadDataAsync<T, U>(string sqlStatement, U parameters, string connectionString)
        {
            try
            {
                // T is the model of the data we want to load
                using IDbConnection
                    connection =
                        new SqliteConnection(
                            connectionString); // this will always close the connection when exiting the scope
                List<T> rows = (await connection.QueryAsync<T>(sqlStatement, parameters)).ToList();
                return rows;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "The statement: {sqlStatement}, {parameter}", sqlStatement, parameters);
                return [];
            }
        }

        /// <summary>
        /// Save data in a SQLite database
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionString"></param>
        /// <typeparam name="T"></typeparam>
        public void SaveData<T>(string sqlStatement, T parameters, string connectionString)
        {
            try
            {
                using IDbConnection connection = new SqliteConnection(connectionString);
                connection.Execute(sqlStatement, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The statement: {sqlStatement}, {parameter}", sqlStatement, parameters);
            }
        }

        /// <summary>
        /// Save data in a SQLite database asynchronously
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionString"></param>
        /// <typeparam name="T"></typeparam>
        public async Task SaveDataAsync<T>(string sqlStatement, T parameters, string connectionString)
        {
            try
            {
                using IDbConnection connection = new SqliteConnection(connectionString);
                await connection.ExecuteAsync(sqlStatement, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The statement: {sqlStatement}, {parameter}", sqlStatement, parameters);
            }
        }
        /// <summary>
        /// Save data in a SQLite database within a transaction
        /// </summary>
        /// <param name="sqlStatement"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionString"></param>
        /// <typeparam name="T"></typeparam>
        public void SaveDataTransaction<T>(string sqlStatement, T parameters, string connectionString)
        {
            try
            {
                using IDbConnection connection = new SqliteConnection(connectionString);
                connection.Open();
                using var transaction = connection.BeginTransaction();

                connection.Execute(sqlStatement, parameters, transaction: transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The statement: {sqlStatement}, {parameters}", sqlStatement, parameters);
            }
        }
#pragma warning restore CA1822 // Mark members as static
    }
}