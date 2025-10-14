using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;

namespace MultiRecord
{
    public class MySqlManager : IDisposable
    {
        private MySqlConnection _connection;
        private readonly string _connectionString;
        private bool _disposed = false;

        public MySqlManager(string host, int port, string username, string password, string database)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = host,
                Port = (uint)port,
                UserID = username,
                Password = password,
                Database = database,
                ConnectionTimeout = 10,
                AllowUserVariables = true,
                UseCompression = false,
                CharacterSet = "utf8mb4",
                SslMode = MySqlSslMode.Disabled
            };
            _connectionString = builder.ToString();
        }

        public async Task<bool> TestNetworkConnectivityAsync(string host, int timeoutMs = 5000)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(host, timeoutMs);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                // Extract host from connection string for ping test
                var builder = new MySqlConnectionStringBuilder(_connectionString);
                string host = builder.Server;
                
                // First test basic network connectivity
                System.Diagnostics.Debug.WriteLine($"Testing network ping to {host}");
                bool pingSuccess = await TestNetworkConnectivityAsync(host, 3000);
                
                if (!pingSuccess)
                {
                    throw new Exception($"Network ping to {host} failed. Server may be unreachable or blocking ICMP.");
                }
                
                System.Diagnostics.Debug.WriteLine($"Ping successful. Testing MySQL connection to {_connectionString}");
                
                using (var connection = new MySqlConnection(_connectionString))
                {
                    
                    await connection.OpenAsync();
                    
                    // Test a simple query to ensure full connectivity
                    using (var command = new MySqlCommand("SELECT 1", connection))
                    {
                        command.CommandTimeout = 5;
                        await command.ExecuteScalarAsync();
                    }
                    
                    return true;
                }
            }
            catch (MySqlException mysqlEx)
            {
                string errorMsg = $"MySQL Error {mysqlEx.Number}: {mysqlEx.Message}";
                
                // Provide specific error guidance
                switch (mysqlEx.Number)
                {
                    case 1042: // Can't get hostname
                        errorMsg += "\n\nSuggestion: Check if the server IP address is correct and reachable.";
                        break;
                    case 1045: // Access denied
                        errorMsg += "\n\nSuggestion: Check username and password.";
                        break;
                    case 1049: // Unknown database
                        errorMsg += "\n\nSuggestion: Check if the database name 'Orbitz' exists.";
                        break;
                    case 0: // Timeout or connection failed
                        errorMsg += "\n\nSuggestion: Check firewall settings and network connectivity.";
                        break;
                }
                
                System.Diagnostics.Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg);
            }
            catch (System.Net.Sockets.SocketException socketEx)
            {
                string errorMsg = $"Network Error: {socketEx.Message}\n\nSuggestion: Check if port {_connectionString.Split(';')[1].Split('=')[1]} is open and accessible.";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg);
            }
            catch (TimeoutException timeoutEx)
            {
                string errorMsg = $"Connection Timeout: {timeoutEx.Message}\n\nSuggestion: The server may be unreachable or overloaded.";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg);
            }
            catch (Exception ex)
            {
                string errorMsg = $"Connection Error: {ex.Message}\n\nType: {ex.GetType().Name}";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                throw new Exception(errorMsg);
            }
        }

        public async Task InitializeDatabaseAsync()
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Create software_models table
                var createModelsTable = @"
                    CREATE TABLE IF NOT EXISTS `software_models` (
                        `id` INT AUTO_INCREMENT PRIMARY KEY,
                        `model_name` VARCHAR(100) NOT NULL UNIQUE,
                        `description` TEXT,
                        `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                        INDEX `idx_model_name` (`model_name`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                ";

                // Create software_serial_numbers table
                var createSerialNumbersTable = @"
                    CREATE TABLE IF NOT EXISTS `software_serial_numbers` (
                        `id` INT AUTO_INCREMENT PRIMARY KEY,
                        `model_id` INT NOT NULL,
                        `serial_number` VARCHAR(100) NOT NULL,
                        `status` VARCHAR(20) DEFAULT 'active',
                        `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                        FOREIGN KEY (`model_id`) REFERENCES `software_models`(`id`) ON DELETE CASCADE,
                        UNIQUE KEY `unique_model_serial` (`model_id`, `serial_number`),
                        INDEX `idx_serial_number` (`serial_number`),
                        INDEX `idx_status` (`status`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                ";

                // Create software_measurements table
                var createMeasurementsTable = @"
                    CREATE TABLE IF NOT EXISTS `software_measurements` (
                        `id` INT AUTO_INCREMENT PRIMARY KEY,
                        `serial_id` INT NOT NULL,
                        `measurement_no` INT NOT NULL,
                        `function_name` VARCHAR(50) NOT NULL,
                        `measurement_value` DECIMAL(15,8) NOT NULL,
                        `upper_limit` DECIMAL(15,8) DEFAULT NULL,
                        `lower_limit` DECIMAL(15,8) DEFAULT NULL,
                        `tolerance_enabled` BOOLEAN DEFAULT FALSE,
                        `measured_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (`serial_id`) REFERENCES `software_serial_numbers`(`id`) ON DELETE CASCADE,
                        INDEX `idx_serial_id` (`serial_id`),
                        INDEX `idx_function_name` (`function_name`),
                        INDEX `idx_measurement_no` (`measurement_no`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                ";

                using (var command = new MySqlCommand(createModelsTable, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new MySqlCommand(createSerialNumbersTable, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new MySqlCommand(createMeasurementsTable, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                // Insert default models if they don't exist
                var insertDefaultModels = @"
                    INSERT IGNORE INTO `software_models` (`model_name`, `description`) VALUES
                    ('SDM3055-SC', 'Siglent Digital Multimeter SDM3055-SC'),
                    ('SDM3065X', 'Siglent Digital Multimeter SDM3065X'),
                    ('Generic DMM', 'Generic Digital Multimeter');
                ";

                using (var command = new MySqlCommand(insertDefaultModels, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Software Models Management
        public async Task<List<(int Id, string ModelName, string Description)>> GetSoftwareModelsAsync()
        {
            var models = new List<(int Id, string ModelName, string Description)>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = "SELECT id, model_name, description FROM software_models ORDER BY model_name";
                using (var command = new MySqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            models.Add((
                                Convert.ToInt32(reader["id"]),
                                reader["model_name"].ToString(),
                                reader["description"]?.ToString() ?? ""
                            ));
                        }
                    }
                }
            }
            
            return models;
        }

        // Software Serial Numbers Management
        public async Task<int> CreateSoftwareSerialNumberAsync(int modelId, string serialNumber)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO software_serial_numbers (model_id, serial_number, status) 
                    VALUES (@modelId, @serialNumber, 'active');
                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@modelId", modelId);
                    command.Parameters.AddWithValue("@serialNumber", serialNumber);
                    
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> CheckSoftwareSerialNumberExistsAsync(int modelId, string serialNumber)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT COUNT(*) FROM software_serial_numbers WHERE model_id = @modelId AND serial_number = @serialNumber";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@modelId", modelId);
                    command.Parameters.AddWithValue("@serialNumber", serialNumber);
                    
                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        public async Task<int?> GetSoftwareSerialNumberIdAsync(int modelId, string serialNumber)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT id FROM software_serial_numbers WHERE model_id = @modelId AND serial_number = @serialNumber AND status = 'active'";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@modelId", modelId);
                    command.Parameters.AddWithValue("@serialNumber", serialNumber);
                    
                    var result = await command.ExecuteScalarAsync();
                    return result != null ? (int?)Convert.ToInt32(result) : null;
                }
            }
        }

        public async Task<List<(int Id, string SerialNumber, DateTime CreatedAt)>> GetSoftwareSerialNumbersByModelAsync(int modelId)
        {
            var serialNumbers = new List<(int Id, string SerialNumber, DateTime CreatedAt)>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = "SELECT id, serial_number, created_at FROM software_serial_numbers WHERE model_id = @modelId ORDER BY created_at DESC";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@modelId", modelId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            serialNumbers.Add((
                                Convert.ToInt32(reader["id"]),
                                reader["serial_number"].ToString(),
                                Convert.ToDateTime(reader["created_at"])
                            ));
                        }
                    }
                }
            }
            
            return serialNumbers;
        }

        // Software Measurements Management
        public async Task<int> InsertSoftwareMeasurementAsync(int serialId, int measurementNo, string functionName, 
            decimal measurementValue, decimal? upperLimit = null, decimal? lowerLimit = null, bool toleranceEnabled = false)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO software_measurements 
                    (serial_id, measurement_no, function_name, measurement_value, upper_limit, lower_limit, tolerance_enabled, measured_at) 
                    VALUES (@serialId, @measurementNo, @functionName, @measurementValue, @upperLimit, @lowerLimit, @toleranceEnabled, NOW());
                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@serialId", serialId);
                    command.Parameters.AddWithValue("@measurementNo", measurementNo);
                    command.Parameters.AddWithValue("@functionName", functionName);
                    command.Parameters.AddWithValue("@measurementValue", measurementValue);
                    command.Parameters.AddWithValue("@upperLimit", upperLimit.HasValue ? (object)upperLimit.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@lowerLimit", lowerLimit.HasValue ? (object)lowerLimit.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@toleranceEnabled", toleranceEnabled);

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> UpdateSoftwareMeasurementToleranceAsync(int measurementId, decimal? upperLimit, decimal? lowerLimit, bool toleranceEnabled)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    UPDATE software_measurements 
                    SET upper_limit = @upperLimit, lower_limit = @lowerLimit, tolerance_enabled = @toleranceEnabled
                    WHERE id = @measurementId";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@measurementId", measurementId);
                    command.Parameters.AddWithValue("@upperLimit", upperLimit.HasValue ? (object)upperLimit.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@lowerLimit", lowerLimit.HasValue ? (object)lowerLimit.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@toleranceEnabled", toleranceEnabled);

                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<List<SoftwareMeasurement>> GetSoftwareMeasurementsBySerialIdAsync(int serialId)
        {
            var measurements = new List<SoftwareMeasurement>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = @"
                    SELECT id, measurement_no, function_name, measurement_value, upper_limit, lower_limit, tolerance_enabled, measured_at
                    FROM software_measurements 
                    WHERE serial_id = @serialId 
                    ORDER BY measurement_no";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@serialId", serialId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            measurements.Add(new SoftwareMeasurement
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                MeasurementNo = Convert.ToInt32(reader["measurement_no"]),
                                FunctionName = reader["function_name"].ToString(),
                                MeasurementValue = Convert.ToDecimal(reader["measurement_value"]),
                                UpperLimit = reader["upper_limit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["upper_limit"]),
                                LowerLimit = reader["lower_limit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["lower_limit"]),
                                ToleranceEnabled = Convert.ToBoolean(reader["tolerance_enabled"]),
                                MeasuredAt = Convert.ToDateTime(reader["measured_at"])
                            });
                        }
                    }
                }
            }
            
            return measurements;
        }


        public async Task<(bool Success, DataTable Data)> ExecuteQuery(string sql)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        using (var adapter = new MySqlDataAdapter(command))
                        {
                            var dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            return (true, dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Query Error: {ex.Message}");
                return (false, null);
            }
        }


        // Legacy methods for backward compatibility with existing system
        public async Task<int> CreateOrUpdateQWRecordAsync(string qwId, string section, string instrumentSerial, string operatorId)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new MySqlConnection(_connectionString);
                await _connection.OpenAsync();
            }
            
            string sql = @"
                INSERT INTO QW_Records (qw_id, section, instrument_serial, operator_id, created_at, updated_at)
                VALUES (@qwId, @section, @instrumentSerial, @operatorId, NOW(), NOW())
                ON DUPLICATE KEY UPDATE
                    section = VALUES(section),
                    instrument_serial = VALUES(instrument_serial),
                    operator_id = VALUES(operator_id),
                    updated_at = NOW()";
            
            using (var command = new MySqlCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@qwId", qwId);
                command.Parameters.AddWithValue("@section", section);
                command.Parameters.AddWithValue("@instrumentSerial", instrumentSerial);
                command.Parameters.AddWithValue("@operatorId", operatorId);
                
                await command.ExecuteNonQueryAsync();
                
                // Get the record ID
                string selectSql = "SELECT id FROM QW_Records WHERE qw_id = @qwId";
                using (var selectCommand = new MySqlCommand(selectSql, _connection))
                {
                    selectCommand.Parameters.AddWithValue("@qwId", qwId);
                    var result = await selectCommand.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }
        
        public async Task<int> InsertMeasurementAsync(int qwRecordId, int measurementIndex, string function, decimal measurementValue, string unit, DateTime timestamp, string toleranceStatus, ToleranceData toleranceData = null)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new MySqlConnection(_connectionString);
                await _connection.OpenAsync();
            }
            
            string sql = @"
                INSERT INTO Measurements 
                (qw_record_id, measurement_index, function_name, measurement_value, unit, timestamp, tolerance_status, upper_limit, lower_limit, tolerance_enabled)
                VALUES (@qwRecordId, @measurementIndex, @function, @measurementValue, @unit, @timestamp, @toleranceStatus, @upperLimit, @lowerLimit, @toleranceEnabled)";
            
            using (var command = new MySqlCommand(sql, _connection))
            {
                command.Parameters.AddWithValue("@qwRecordId", qwRecordId);
                command.Parameters.AddWithValue("@measurementIndex", measurementIndex);
                command.Parameters.AddWithValue("@function", function);
                command.Parameters.AddWithValue("@measurementValue", measurementValue);
                command.Parameters.AddWithValue("@unit", unit);
                command.Parameters.AddWithValue("@timestamp", timestamp);
                command.Parameters.AddWithValue("@toleranceStatus", toleranceStatus);
                command.Parameters.AddWithValue("@upperLimit", toleranceData?.UpperLimit);
                command.Parameters.AddWithValue("@lowerLimit", toleranceData?.LowerLimit);
                command.Parameters.AddWithValue("@toleranceEnabled", toleranceData?.Enabled ?? false);
                
                await command.ExecuteNonQueryAsync();
                return (int)command.LastInsertedId;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _connection?.Dispose();
                _disposed = true;
            }
        }
    }

    public class SoftwareMeasurement
    {
        public int Id { get; set; }
        public int MeasurementNo { get; set; }
        public string FunctionName { get; set; }
        public decimal MeasurementValue { get; set; }
        public decimal? UpperLimit { get; set; }
        public decimal? LowerLimit { get; set; }
        public bool ToleranceEnabled { get; set; }
        public DateTime MeasuredAt { get; set; }
    }
    
    // Legacy class for backward compatibility
    public class ToleranceData
    {
        public decimal? UpperLimit { get; set; }
        public decimal? LowerLimit { get; set; }
        public decimal? UpperPercent { get; set; }
        public decimal? LowerPercent { get; set; }
        public decimal? UpperAbs { get; set; }
        public decimal? LowerAbs { get; set; }
        public bool Enabled { get; set; }
    }
}