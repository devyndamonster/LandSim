using Microsoft.Data.Sqlite;
using System.Data;

namespace LandSim.Areas.Map
{
    public class DatabaseConnection
    {

        public IDbConnection GetConnection()
        {
            return new SqliteConnection("Data Source=LandSim.db");
        }
        
    }
}
