namespace WibboEmulator.Database;

using MySql.Data.MySqlClient;
using WibboEmulator.Core;
using WibboEmulator.Database.Interfaces;

public sealed class DatabaseManager
{
    private readonly string _connectionStr;

    public DatabaseManager(DatabaseConfiguration databaseConfiguration)
    {
        var connectionStringBuilder = new MySqlConnectionStringBuilder
        {
            ConnectionTimeout = 300,
            Database = databaseConfiguration.Name,
            DefaultCommandTimeout = 300,
            Logging = false,
            MaximumPoolSize = databaseConfiguration.MaximumPoolSize,
            MinimumPoolSize = databaseConfiguration.MinimumPoolSize,
            Password = databaseConfiguration.Password,
            Pooling = true,
            Port = databaseConfiguration.Port,
            Server = databaseConfiguration.Hostname,
            UserID = databaseConfiguration.Username,
            AllowZeroDateTime = true,
            CharacterSet = "utf8mb4"
        };

        this._connectionStr = connectionStringBuilder.ToString();
    }

    public bool IsConnected()
    {
        try
        {
            using var con = new MySqlConnection(this._connectionStr);
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = "SELECT 1+1";
            _ = cmd.ExecuteNonQuery();
            cmd.Dispose();
            con.Close();
        }
        catch (MySqlException)
        {
            return false;
        }

        return true;
    }

    public IQueryAdapter GetQueryReactor()
    {
        try
        {
            IDatabaseClient dbConnection = new DatabaseConnection(this._connectionStr);

            dbConnection.Connect();

            return dbConnection.GetQueryReactor();
        }
        catch (MySqlException e)
        {
            ExceptionLogger.LogException($"Database connection error: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            ExceptionLogger.LogException($"Unexpected error: {e.Message}");
            return null;
        }
    }
}
