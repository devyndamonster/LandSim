using Microsoft.Data.Sqlite;
using System.Data;

namespace LandSim.Database
{
    public class DatabaseConnection
    {

        public IDbConnection GetConnection()
        {
            return new SqliteConnection("Data Source=LandSim.db");
        }
        
    }
}
