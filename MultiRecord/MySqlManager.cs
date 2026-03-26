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
                        `measurement_name` VARCHAR(100) DEFAULT NULL,
                        `measurement_value` DECIMAL(15,6) NOT NULL,
                        `upper_limit` DECIMAL(15,6) DEFAULT NULL,
                        `lower_limit` DECIMAL(15,6) DEFAULT NULL,
                        `tolerance_enabled` TINYINT(1) DEFAULT 0,
                        `tolerance_percentage` DECIMAL(5,2) DEFAULT NULL,
                        `tolerance_type` ENUM('percent','absolute') DEFAULT 'percent',
                        `note` TEXT DEFAULT NULL,
                        `system_info` TEXT DEFAULT NULL,
                        `is_pass` TINYINT(1) DEFAULT NULL,
                        `measured_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
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

                // Note: Database schema is already up-to-date with all required columns

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
                
                // Query from spaze database, models table
                var sql = "SELECT id, name, description FROM spaze.models ORDER BY name";
                using (var command = new MySqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            models.Add((
                                Convert.ToInt32(reader["id"]),
                                reader["name"].ToString(),
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

        // Get all serial numbers regardless of model_id (for cases where model_id comes from external system like Spaze)
        public async Task<List<(int Id, string SerialNumber, DateTime CreatedAt)>> GetAllSoftwareSerialNumbersAsync()
        {
            var serialNumbers = new List<(int Id, string SerialNumber, DateTime CreatedAt)>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = "SELECT id, serial_number, created_at FROM software_serial_numbers ORDER BY created_at DESC";
                using (var command = new MySqlCommand(sql, connection))
                {
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

        // Get serial number ID by serial number only (ignore model_id)
        public async Task<int?> GetSoftwareSerialNumberIdBySerialAsync(string serialNumber)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = "SELECT id FROM software_serial_numbers WHERE serial_number = @serialNumber AND status = 'active'";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@serialNumber", serialNumber);
                    
                    var result = await command.ExecuteScalarAsync();
                    return result != null ? (int?)Convert.ToInt32(result) : null;
                }
            }
        }

        // Software Measurements Management
        public async Task<int> InsertSoftwareMeasurementAsync(int serialId, int measurementNo, string functionName, 
            decimal measurementValue, decimal? upperLimit = null, decimal? lowerLimit = null, bool toleranceEnabled = false, 
            string systemInfo = null, string measurementName = null, string toleranceType = "percent", 
            decimal? tolerancePercentage = null, string note = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO software_measurements 
                    (serial_id, measurement_no, measurement_name, function_name, measurement_value, upper_limit, lower_limit, 
                     tolerance_enabled, tolerance_type, tolerance_percentage, note, system_info, measured_at) 
                    VALUES (@serialId, @measurementNo, @measurementName, @functionName, @measurementValue, @upperLimit, @lowerLimit, 
                            @toleranceEnabled, @toleranceType, @tolerancePercentage, @note, @systemInfo, NOW());
                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@serialId", serialId);
                    command.Parameters.AddWithValue("@measurementNo", measurementNo);
                    command.Parameters.AddWithValue("@measurementName", !string.IsNullOrEmpty(measurementName) ? (object)measurementName : DBNull.Value);
                    command.Parameters.AddWithValue("@functionName", functionName);
                    command.Parameters.AddWithValue("@measurementValue", measurementValue);
                    command.Parameters.AddWithValue("@upperLimit", upperLimit.HasValue ? (object)upperLimit.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@lowerLimit", lowerLimit.HasValue ? (object)lowerLimit.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@toleranceEnabled", toleranceEnabled);
                    command.Parameters.AddWithValue("@toleranceType", toleranceType ?? "percent");
                    command.Parameters.AddWithValue("@tolerancePercentage", tolerancePercentage.HasValue ? (object)tolerancePercentage.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@note", !string.IsNullOrEmpty(note) ? (object)note : DBNull.Value);
                    command.Parameters.AddWithValue("@systemInfo", !string.IsNullOrEmpty(systemInfo) ? (object)systemInfo : DBNull.Value);

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<bool> UpdateSoftwareMeasurementToleranceAsync(int measurementId, decimal? upperLimit, decimal? lowerLimit, 
            bool toleranceEnabled, string toleranceType = null, decimal? tolerancePercentage = null, string note = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    UPDATE software_measurements 
                    SET upper_limit = @upperLimit, lower_limit = @lowerLimit, tolerance_enabled = @toleranceEnabled,
                        tolerance_type = COALESCE(@toleranceType, tolerance_type),
                        tolerance_percentage = @tolerancePercentage,
                        note = COALESCE(@note, note)
                    WHERE id = @measurementId";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@measurementId", measurementId);
                    command.Parameters.AddWithValue("@upperLimit", upperLimit.HasValue ? (object)upperLimit.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@lowerLimit", lowerLimit.HasValue ? (object)lowerLimit.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@toleranceEnabled", toleranceEnabled);
                    command.Parameters.AddWithValue("@toleranceType", !string.IsNullOrEmpty(toleranceType) ? (object)toleranceType : DBNull.Value);
                    command.Parameters.AddWithValue("@tolerancePercentage", tolerancePercentage.HasValue ? (object)tolerancePercentage.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@note", !string.IsNullOrEmpty(note) ? (object)note : DBNull.Value);

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
                    SELECT id, measurement_no, measurement_name, function_name, measurement_value, upper_limit, lower_limit, 
                           tolerance_enabled, tolerance_type, tolerance_percentage, note, system_info, is_pass, measured_at
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
                                MeasurementName = reader["measurement_name"]?.ToString(),
                                FunctionName = reader["function_name"].ToString(),
                                MeasurementValue = Convert.ToDecimal(reader["measurement_value"]),
                                UpperLimit = reader["upper_limit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["upper_limit"]),
                                LowerLimit = reader["lower_limit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["lower_limit"]),
                                ToleranceEnabled = Convert.ToBoolean(reader["tolerance_enabled"]),
                                ToleranceType = reader["tolerance_type"]?.ToString() ?? "percent",
                                TolerancePercentage = reader["tolerance_percentage"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["tolerance_percentage"]),
                                Note = reader["note"]?.ToString(),
                                SystemInfo = reader["system_info"]?.ToString(),
                                IsPass = reader["is_pass"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(reader["is_pass"]),
                                MeasuredAt = reader["measured_at"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["measured_at"])
                            });
                        }
                    }
                }
            }
            
            return measurements;
        }

        // ==================== Repair Sessions Management ====================
        
        /// <summary>
        /// Get the next session number for a given serial_id and qw_id
        /// </summary>
        public async Task<int> GetNextRepairSessionNumberAsync(int serialId, string qwId = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = "SELECT COALESCE(MAX(session_number), 0) + 1 FROM software_repair_sessions WHERE serial_id = @serialId";
                
                // ถ้ามี qwId ให้กรองด้วย
                if (!string.IsNullOrEmpty(qwId))
                {
                    sql += " AND qw_id = @qwId";
                }
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@serialId", serialId);
                    if (!string.IsNullOrEmpty(qwId))
                    {
                        command.Parameters.AddWithValue("@qwId", qwId);
                    }
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// Create a new repair session
        /// </summary>
        public async Task<int> CreateRepairSessionAsync(string qwId, int serialId, string serialNumber, int sessionNumber, string sessionNote, string repairType = "before_repair", string createdBy = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = @"
                    INSERT INTO software_repair_sessions 
                    (qw_id, serial_id, session_number, session_note, repair_type, status, created_by, created_at)
                    VALUES (@qwId, @serialId, @sessionNumber, @sessionNote, @repairType, 'in_progress', @createdBy, NOW());
                    SELECT LAST_INSERT_ID();";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@qwId", qwId);
                    command.Parameters.AddWithValue("@serialId", serialId);
                    command.Parameters.AddWithValue("@sessionNumber", sessionNumber);
                    command.Parameters.AddWithValue("@sessionNote", !string.IsNullOrEmpty(sessionNote) ? (object)sessionNote : DBNull.Value);
                    command.Parameters.AddWithValue("@repairType", repairType);
                    command.Parameters.AddWithValue("@createdBy", !string.IsNullOrEmpty(createdBy) ? (object)createdBy : DBNull.Value);
                    
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// Clone measurements from software_measurements to software_repair_measurements
        /// </summary>
        public async Task<int> CloneMeasurementsToRepairSessionAsync(int repairSessionId, int serialId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = @"
                    INSERT INTO software_repair_measurements 
                    (repair_session_id, measurement_no, measurement_name, function_name, measurement_value, 
                     upper_limit, lower_limit, tolerance_enabled, tolerance_type, tolerance_percentage, note, system_info, measured_at)
                    SELECT 
                        @repairSessionId,
                        measurement_no, measurement_name, function_name, measurement_value,
                        upper_limit, lower_limit, tolerance_enabled, tolerance_type, tolerance_percentage, note, system_info, NOW()
                    FROM software_measurements
                    WHERE serial_id = @serialId
                    ORDER BY measurement_no";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@repairSessionId", repairSessionId);
                    command.Parameters.AddWithValue("@serialId", serialId);
                    
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected;
                }
            }
        }

        /// <summary>
        /// Get all repair sessions for a specific serial_id and qw_id
        /// </summary>
        public async Task<List<RepairSession>> GetRepairSessionsBySerialIdAsync(int serialId, string qwId = null)
        {
            var sessions = new List<RepairSession>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = @"
                    SELECT id, qw_id, serial_id, session_number, session_note, repair_type, status, created_by, created_at, updated_at, completed_at
                    FROM software_repair_sessions
                    WHERE serial_id = @serialId";
                
                // ถ้ามี qwId ให้กรองด้วย
                if (!string.IsNullOrEmpty(qwId))
                {
                    sql += " AND qw_id = @qwId";
                }
                
                sql += " ORDER BY session_number DESC";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@serialId", serialId);
                    if (!string.IsNullOrEmpty(qwId))
                    {
                        command.Parameters.AddWithValue("@qwId", qwId);
                    }
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            sessions.Add(new RepairSession
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                QwId = reader["qw_id"].ToString(),
                                SerialId = Convert.ToInt32(reader["serial_id"]),
                                SessionNumber = Convert.ToInt32(reader["session_number"]),
                                SessionNote = reader["session_note"]?.ToString(),
                                RepairType = reader["repair_type"]?.ToString() ?? "before_repair",
                                Status = reader["status"].ToString(),
                                CreatedBy = reader["created_by"]?.ToString(),
                                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                                UpdatedAt = Convert.ToDateTime(reader["updated_at"]),
                                CompletedAt = reader["completed_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["completed_at"])
                            });
                        }
                    }
                }
            }
            
            return sessions;
        }

        /// <summary>
        /// Get repair measurements for a specific repair session
        /// </summary>
        public async Task<List<RepairMeasurement>> GetRepairMeasurementsBySessionIdAsync(int repairSessionId)
        {
            var measurements = new List<RepairMeasurement>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = @"
                    SELECT id, repair_session_id, measurement_no, measurement_name, function_name, measurement_value, actual_measured_value,
                           upper_limit, lower_limit, tolerance_enabled, tolerance_type, tolerance_percentage, note, system_info, is_pass, measured_at
                    FROM software_repair_measurements
                    WHERE repair_session_id = @repairSessionId
                    ORDER BY measurement_no";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@repairSessionId", repairSessionId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            measurements.Add(new RepairMeasurement
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                RepairSessionId = Convert.ToInt32(reader["repair_session_id"]),
                                MeasurementNo = Convert.ToInt32(reader["measurement_no"]),
                                MeasurementName = reader["measurement_name"]?.ToString(),
                                FunctionName = reader["function_name"].ToString(),
                                MeasurementValue = Convert.ToDecimal(reader["measurement_value"]),
                                ActualMeasuredValue = reader["actual_measured_value"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["actual_measured_value"]),
                                UpperLimit = reader["upper_limit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["upper_limit"]),
                                LowerLimit = reader["lower_limit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["lower_limit"]),
                                ToleranceEnabled = Convert.ToBoolean(reader["tolerance_enabled"]),
                                ToleranceType = reader["tolerance_type"]?.ToString() ?? "percent",
                                TolerancePercentage = reader["tolerance_percentage"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["tolerance_percentage"]),
                                Note = reader["note"]?.ToString(),
                                SystemInfo = reader["system_info"]?.ToString(),
                                IsPass = reader["is_pass"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(reader["is_pass"]),
                                MeasuredAt = reader["measured_at"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["measured_at"])
                            });
                        }
                    }
                }
            }
            
            return measurements;
        }

        /// <summary>
        /// Update a repair measurement value (actual measured value)
        /// </summary>
        public async Task<bool> UpdateRepairMeasurementAsync(int measurementId, decimal actualMeasuredValue, bool? isPass = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = @"
                    UPDATE software_repair_measurements
                    SET actual_measured_value = @actualMeasuredValue,
                        is_pass = @isPass,
                        measured_at = NOW()
                    WHERE id = @measurementId";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@measurementId", measurementId);
                    command.Parameters.AddWithValue("@actualMeasuredValue", actualMeasuredValue);
                    command.Parameters.AddWithValue("@isPass", isPass.HasValue ? (object)isPass.Value : DBNull.Value);
                    
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        /// <summary>
        /// Clear a single repair measurement value (undo)
        /// </summary>
        public async Task<bool> ClearRepairMeasurementValueAsync(int measurementId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    UPDATE software_repair_measurements
                    SET actual_measured_value = NULL,
                        is_pass = NULL,
                        measured_at = NULL
                    WHERE id = @measurementId";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@measurementId", measurementId);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        /// <summary>
        /// Clear all repair measurement values for a session (ล้างข้อมูลทั้งหมด)
        /// </summary>
        public async Task<bool> ClearRepairSessionMeasurementsAsync(int sessionId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    UPDATE software_repair_measurements
                    SET actual_measured_value = NULL,
                        is_pass = NULL,
                        measured_at = NULL
                    WHERE repair_session_id = @sessionId";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@sessionId", sessionId);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        /// <summary>
        /// Update repair session status
        /// </summary>
        public async Task<bool> UpdateRepairSessionStatusAsync(int sessionId, string status)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var sql = @"
                    UPDATE software_repair_sessions
                    SET status = @status,
                        completed_at = CASE WHEN @status = 'completed' THEN NOW() ELSE completed_at END
                    WHERE id = @sessionId";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@sessionId", sessionId);
                    command.Parameters.AddWithValue("@status", status);
                    
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        /// <summary>
        /// Update repair session type (before_repair or after_repair) with optional note
        /// </summary>
        public async Task<bool> UpdateRepairSessionTypeAsync(int sessionId, string repairType, string note = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Build SQL based on whether note is provided
                var sql = string.IsNullOrEmpty(note) 
                    ? @"UPDATE software_repair_sessions
                        SET repair_type = @repairType
                        WHERE id = @sessionId"
                    : @"UPDATE software_repair_sessions
                        SET repair_type = @repairType,
                            session_note = CONCAT(IFNULL(session_note, ''), 
                                                  CASE WHEN IFNULL(session_note, '') = '' THEN '' ELSE '\n' END,
                                                  '[เปลี่ยนประเภท] ', @note)
                        WHERE id = @sessionId";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@sessionId", sessionId);
                    command.Parameters.AddWithValue("@repairType", repairType);
                    if (!string.IsNullOrEmpty(note))
                    {
                        command.Parameters.AddWithValue("@note", note);
                    }
                    
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        /// <summary>
        /// Delete a repair session and all its measurements
        /// </summary>
        public async Task<bool> DeleteRepairSessionAsync(int sessionId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // CASCADE DELETE will automatically delete related measurements
                var sql = "DELETE FROM software_repair_sessions WHERE id = @sessionId";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@sessionId", sessionId);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }


        public async Task<(bool Success, DataTable Data, string ErrorMessage)> ExecuteQuery(string sql)
        {
            try
            {
                Console.WriteLine($"[MySqlManager.ExecuteQuery] Executing SQL: {sql}");
                Console.WriteLine($"[MySqlManager.ExecuteQuery] Connection String: {_connectionString}");
                
                using (var connection = new MySqlConnection(_connectionString))
                {
                    Console.WriteLine($"[MySqlManager.ExecuteQuery] Opening connection...");
                    await connection.OpenAsync();
                    Console.WriteLine($"[MySqlManager.ExecuteQuery] Connection opened successfully");
                    
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        using (var adapter = new MySqlDataAdapter(command))
                        {
                            var dataTable = new DataTable();
                            Console.WriteLine($"[MySqlManager.ExecuteQuery] Filling DataTable...");
                            adapter.Fill(dataTable);
                            Console.WriteLine($"[MySqlManager.ExecuteQuery] Query executed successfully, rows: {dataTable.Rows.Count}");
                            return (true, dataTable, "Success");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"SQL Query Error: {ex.Message}";
                Console.WriteLine($"[MySqlManager.ExecuteQuery] Exception: {errorMsg}");
                Console.WriteLine($"[MySqlManager.ExecuteQuery] Stack Trace: {ex.StackTrace}");
                System.Diagnostics.Debug.WriteLine(errorMsg);
                return (false, null, errorMsg);
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

        // Method to scroll DataGridView to latest record
        public void ScrollRDToLatest(Main mainForm)
        {
            try
            {
                if (mainForm?.dataGridViewRD?.Rows?.Count > 0)
                {
                    var dataGridView = mainForm.dataGridViewRD;
                    int lastIndex = dataGridView.Rows.Count - 1;
                    
                    // Clear current selection
                    dataGridView.ClearSelection();
                    
                    // Select the last row
                    dataGridView.Rows[lastIndex].Selected = true;
                    
                    // Scroll to the last row
                    dataGridView.FirstDisplayedScrollingRowIndex = lastIndex;
                    
                    // Set current cell to the last row
                    if (dataGridView.Columns.Count > 0)
                    {
                        dataGridView.CurrentCell = dataGridView.Rows[lastIndex].Cells[0];
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw to avoid breaking the main flow
                System.Diagnostics.Debug.WriteLine($"Error scrolling RD DataGridView: {ex.Message}");
            }
        }
    }

    public class SoftwareMeasurement
    {
        public int Id { get; set; }
        public int MeasurementNo { get; set; }
        public string MeasurementName { get; set; }
        public string FunctionName { get; set; }
        public decimal MeasurementValue { get; set; }
        public decimal? UpperLimit { get; set; }
        public decimal? LowerLimit { get; set; }
        public bool ToleranceEnabled { get; set; }
        public string ToleranceType { get; set; } = "percent";
        public decimal? TolerancePercentage { get; set; }
        public string Note { get; set; }
        public string SystemInfo { get; set; }
        public bool? IsPass { get; set; }
        public DateTime MeasuredAt { get; set; }
    }

    public class RepairSession
    {
        public int Id { get; set; }
        public string QwId { get; set; }
        public int SerialId { get; set; }
        public int SessionNumber { get; set; }
        public string SessionNote { get; set; }
        public string RepairType { get; set; } = "before_repair"; // before_repair or after_repair
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        // Property สำหรับแสดงใน ComboBox
        public string DisplayText => $"รอบที่ {SessionNumber} ({(RepairType == "before_repair" ? "ก่อนซ่อม" : "หลังซ่อม")})";
    }

    public class RepairMeasurement
    {
        public int Id { get; set; }
        public int RepairSessionId { get; set; }
        public int MeasurementNo { get; set; }
        public string MeasurementName { get; set; }
        public string FunctionName { get; set; }
        public decimal MeasurementValue { get; set; } // ค่าอ้างอิงจาก template
        public decimal? ActualMeasuredValue { get; set; } // ค่าที่วัดได้จริงในการซ่อม
        public decimal? UpperLimit { get; set; }
        public decimal? LowerLimit { get; set; }
        public bool ToleranceEnabled { get; set; }
        public string ToleranceType { get; set; } = "percent";
        public decimal? TolerancePercentage { get; set; }
        public string Note { get; set; }
        public string SystemInfo { get; set; }
        public bool? IsPass { get; set; }
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