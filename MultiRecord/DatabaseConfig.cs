using System;

namespace MultiRecord
{
    public static class DatabaseConfig
    {
        // Production Database Configuration
        public static readonly string Host = "100.83.170.41";
        public static readonly int Port = 3306;
        public static readonly string Username = "orbitz_portal";
        public static readonly string Password = "Qwaveadmin12.";
        public static readonly string Database = "Orbitz";
        
        // Method to create MySqlManager with production config
        public static MySqlManager CreateMySqlManager()
        {
            return new MySqlManager(Host, Port, Username, Password, Database);
        }
        
        // Method to create MySqlManager with custom config (for testing or other purposes)
        public static MySqlManager CreateMySqlManager(string host, int port, string username, string password, string database)
        {
            return new MySqlManager(host, port, username, password, database);
        }
    }
}
