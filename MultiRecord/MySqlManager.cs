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
                SslMode = MySqlSslMode.None
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

                // Create QW_Records table
                var createRecordsTable = @"
                    CREATE TABLE IF NOT EXISTS `QW_Records` (
                        `id` INT AUTO_INCREMENT PRIMARY KEY,
                        `QWID` VARCHAR(50) NOT NULL UNIQUE,
                        `Section` VARCHAR(100) NOT NULL,
                        `InstrumentVendor` VARCHAR(50) NOT NULL DEFAULT 'Siglent',
                        `InstrumentModel` VARCHAR(50) NOT NULL DEFAULT 'SDM3055-SC',
                        `InstrumentSerial` VARCHAR(100) NOT NULL,
                        `OperatorID` INT DEFAULT 1,
                        `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        `UpdatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                        INDEX `idx_qwid` (`QWID`),
                        INDEX `idx_section` (`Section`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                ";

                // Create Measurements table
                var createMeasurementsTable = @"
                    CREATE TABLE IF NOT EXISTS `Measurements` (
                        `id` INT AUTO_INCREMENT PRIMARY KEY,
                        `QW_Record_ID` INT NOT NULL,
                        `MeasurementIndex` INT NOT NULL,
                        `Function` VARCHAR(50) NOT NULL,
                        `Measurement` DECIMAL(15,8) NOT NULL,
                        `Unit` VARCHAR(20) NOT NULL,
                        `Timestamp` DATETIME NOT NULL,
                        `Tolerance` VARCHAR(20) DEFAULT 'N/A',
                        `ToleranceMode` VARCHAR(20) DEFAULT 'percent',
                        `UpperPercent` DECIMAL(8,4) DEFAULT NULL,
                        `LowerPercent` DECIMAL(8,4) DEFAULT NULL,
                        `UpperAbs` DECIMAL(15,8) DEFAULT NULL,
                        `LowerAbs` DECIMAL(15,8) DEFAULT NULL,
                        `ToleranceEnabled` BOOLEAN DEFAULT FALSE,
                        `MarkerAnnotation_ID` INT DEFAULT NULL,
                        `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (`QW_Record_ID`) REFERENCES `QW_Records`(`id`) ON DELETE CASCADE,
                        INDEX `idx_qw_record` (`QW_Record_ID`),
                        INDEX `idx_function` (`Function`),
                        INDEX `idx_timestamp` (`Timestamp`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                ";

                // Create Annotation_Images table
                var createImagesTable = @"
                    CREATE TABLE IF NOT EXISTS `Annotation_Images` (
                        `id` INT AUTO_INCREMENT PRIMARY KEY,
                        `QW_Record_ID` INT NOT NULL,
                        `ImageData` LONGTEXT NOT NULL,
                        `MimeType` VARCHAR(50) DEFAULT 'image/jpeg',
                        `Width` INT DEFAULT NULL,
                        `Height` INT DEFAULT NULL,
                        `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (`QW_Record_ID`) REFERENCES `QW_Records`(`id`) ON DELETE CASCADE,
                        INDEX `idx_qw_record_img` (`QW_Record_ID`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                ";

                // Create Marker_Annotations table
                var createAnnotationsTable = @"
                    CREATE TABLE IF NOT EXISTS `Marker_Annotations` (
                        `id` INT AUTO_INCREMENT PRIMARY KEY,
                        `QW_Record_ID` INT NOT NULL,
                        `Measurement_ID` INT NOT NULL,
                        `PositionX` DECIMAL(10,2) NOT NULL,
                        `PositionY` DECIMAL(10,2) NOT NULL,
                        `Label` VARCHAR(255) NOT NULL,
                        `Shape` VARCHAR(20) DEFAULT 'point',
                        `Color` VARCHAR(20) DEFAULT NULL,
                        `CreatedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY (`QW_Record_ID`) REFERENCES `QW_Records`(`id`) ON DELETE CASCADE,
                        FOREIGN KEY (`Measurement_ID`) REFERENCES `Measurements`(`id`) ON DELETE CASCADE,
                        INDEX `idx_qw_record_marker` (`QW_Record_ID`),
                        INDEX `idx_measurement` (`Measurement_ID`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
                ";

                using (var command = new MySqlCommand(createRecordsTable, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new MySqlCommand(createMeasurementsTable, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new MySqlCommand(createImagesTable, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                using (var command = new MySqlCommand(createAnnotationsTable, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<int> CreateOrUpdateQWRecordAsync(string qwid, string section, string instrumentSerial, int operatorId = 1)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check if record exists
                var checkSql = "SELECT id FROM QW_Records WHERE QWID = @qwid";
                using (var checkCommand = new MySqlCommand(checkSql, connection))
                {
                    checkCommand.Parameters.AddWithValue("@qwid", qwid);
                    var existingId = await checkCommand.ExecuteScalarAsync();
                    
                    if (existingId != null)
                    {
                        // Update existing record
                        var updateSql = @"
                            UPDATE QW_Records 
                            SET Section = @section, InstrumentSerial = @serial, OperatorID = @operatorId, UpdatedAt = CURRENT_TIMESTAMP
                            WHERE QWID = @qwid";
                        
                        using (var updateCommand = new MySqlCommand(updateSql, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@qwid", qwid);
                            updateCommand.Parameters.AddWithValue("@section", section);
                            updateCommand.Parameters.AddWithValue("@serial", instrumentSerial);
                            updateCommand.Parameters.AddWithValue("@operatorId", operatorId);
                            await updateCommand.ExecuteNonQueryAsync();
                        }
                        
                        return Convert.ToInt32(existingId);
                    }
                    else
                    {
                        // Create new record
                        var insertSql = @"
                            INSERT INTO QW_Records (QWID, Section, InstrumentSerial, OperatorID) 
                            VALUES (@qwid, @section, @serial, @operatorId);
                            SELECT LAST_INSERT_ID();";
                        
                        using (var insertCommand = new MySqlCommand(insertSql, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@qwid", qwid);
                            insertCommand.Parameters.AddWithValue("@section", section);
                            insertCommand.Parameters.AddWithValue("@serial", instrumentSerial);
                            insertCommand.Parameters.AddWithValue("@operatorId", operatorId);
                            var result = await insertCommand.ExecuteScalarAsync();
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
        }

        public async Task<int> InsertMeasurementAsync(int qwRecordId, int measurementIndex, string function, 
            decimal measurement, string unit, DateTime timestamp, string tolerance, 
            ToleranceData toleranceData = null, int? markerAnnotationId = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO Measurements 
                    (QW_Record_ID, MeasurementIndex, Function, Measurement, Unit, Timestamp, Tolerance, 
                     ToleranceMode, UpperPercent, LowerPercent, UpperAbs, LowerAbs, ToleranceEnabled, MarkerAnnotation_ID) 
                    VALUES (@qwRecordId, @measurementIndex, @function, @measurement, @unit, @timestamp, @tolerance,
                            @toleranceMode, @upperPercent, @lowerPercent, @upperAbs, @lowerAbs, @toleranceEnabled, @markerAnnotationId);
                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@qwRecordId", qwRecordId);
                    command.Parameters.AddWithValue("@measurementIndex", measurementIndex);
                    command.Parameters.AddWithValue("@function", function);
                    command.Parameters.AddWithValue("@measurement", measurement);
                    command.Parameters.AddWithValue("@unit", unit);
                    command.Parameters.AddWithValue("@timestamp", timestamp);
                    command.Parameters.AddWithValue("@tolerance", tolerance);

                    if (toleranceData != null)
                    {
                        command.Parameters.AddWithValue("@toleranceMode", toleranceData.Mode ?? "percent");
                        command.Parameters.AddWithValue("@upperPercent", toleranceData.UpperPercent);
                        command.Parameters.AddWithValue("@lowerPercent", toleranceData.LowerPercent);
                        command.Parameters.AddWithValue("@upperAbs", toleranceData.UpperAbs);
                        command.Parameters.AddWithValue("@lowerAbs", toleranceData.LowerAbs);
                        command.Parameters.AddWithValue("@toleranceEnabled", toleranceData.Enabled);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@toleranceMode", DBNull.Value);
                        command.Parameters.AddWithValue("@upperPercent", DBNull.Value);
                        command.Parameters.AddWithValue("@lowerPercent", DBNull.Value);
                        command.Parameters.AddWithValue("@upperAbs", DBNull.Value);
                        command.Parameters.AddWithValue("@lowerAbs", DBNull.Value);
                        command.Parameters.AddWithValue("@toleranceEnabled", false);
                    }

                    command.Parameters.AddWithValue("@markerAnnotationId", markerAnnotationId.HasValue ? (object)markerAnnotationId.Value : DBNull.Value);

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<int> InsertMarkerAnnotationAsync(int qwRecordId, int measurementId, 
            decimal positionX, decimal positionY, string label, string shape = "point", string color = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO Marker_Annotations (QW_Record_ID, Measurement_ID, PositionX, PositionY, Label, Shape, Color) 
                    VALUES (@qwRecordId, @measurementId, @positionX, @positionY, @label, @shape, @color);
                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@qwRecordId", qwRecordId);
                    command.Parameters.AddWithValue("@measurementId", measurementId);
                    command.Parameters.AddWithValue("@positionX", positionX);
                    command.Parameters.AddWithValue("@positionY", positionY);
                    command.Parameters.AddWithValue("@label", label);
                    command.Parameters.AddWithValue("@shape", shape ?? "point");
                    command.Parameters.AddWithValue("@color", (object)color ?? DBNull.Value);

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<int> InsertAnnotationImageAsync(int qwRecordId, string imageDataBase64, 
            string mimeType = "image/jpeg", int? width = null, int? height = null)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    INSERT INTO Annotation_Images (QW_Record_ID, ImageData, MimeType, Width, Height) 
                    VALUES (@qwRecordId, @imageData, @mimeType, @width, @height);
                    SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@qwRecordId", qwRecordId);
                    command.Parameters.AddWithValue("@imageData", imageDataBase64);
                    command.Parameters.AddWithValue("@mimeType", mimeType);
                    command.Parameters.AddWithValue("@width", width.HasValue ? (object)width.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@height", height.HasValue ? (object)height.Value : DBNull.Value);

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<JObject> ExportQWRecordAsJsonAsync(string qwid)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Get QW Record
                var qwRecordSql = @"
                    SELECT QWID, Section, InstrumentVendor, InstrumentModel, InstrumentSerial 
                    FROM QW_Records WHERE QWID = @qwid";

                JObject result = new JObject();

                using (var command = new MySqlCommand(qwRecordSql, connection))
                {
                    command.Parameters.AddWithValue("@qwid", qwid);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result["QWID"] = reader["QWID"].ToString();
                            result["Section"] = reader["Section"].ToString();
                            
                            result["Instrument"] = new JObject
                            {
                                ["Vendor"] = reader["InstrumentVendor"].ToString(),
                                ["Model"] = reader["InstrumentModel"].ToString(),
                                ["Serial"] = reader["InstrumentSerial"].ToString()
                            };
                        }
                    }
                }

                // Get Measurements and Annotations
                // This is a complex query that we'll implement based on your specific needs
                // For now, returning basic structure
                result["Measurement"] = new JArray();
                result["MarkerAnnotation"] = new JArray();

                return result;
            }
        }

        public async Task<List<PCBMeasurement>> GetPCBMeasurementsBySessionAsync(string sessionId)
        {
            var measurements = new List<PCBMeasurement>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    SELECT 
                        id, measurement_session_id, marker_id, measurement_number,
                        measured_value, tolerance_status, tolerance_upper, tolerance_lower,
                        tolerance_upper_type, tolerance_lower_type, tolerance_enabled,
                        marker_type, marker_parameters, marker_position_x, marker_position_y,
                        marker_display_name, marker_color, notes, measurement_unit,
                        tolerance_upper_limit, tolerance_lower_limit, open
                    FROM pcb_measurements 
                    WHERE measurement_session_id = @sessionId 
                    ORDER BY measurement_number";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@sessionId", sessionId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var measurement = new PCBMeasurement
                            {
                                Id = reader["id"].ToString(),
                                MeasurementSessionId = reader["measurement_session_id"].ToString(),
                                MarkerId = reader["marker_id"].ToString(),
                                MeasurementNumber = Convert.ToInt32(reader["measurement_number"]),
                                MeasuredValue = reader["measured_value"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["measured_value"]),
                                ToleranceStatus = reader["tolerance_status"].ToString(),
                                ToleranceUpper = Convert.ToDecimal(reader["tolerance_upper"]),
                                ToleranceLower = Convert.ToDecimal(reader["tolerance_lower"]),
                                ToleranceUpperType = reader["tolerance_upper_type"].ToString(),
                                ToleranceLowerType = reader["tolerance_lower_type"].ToString(),
                                ToleranceEnabled = Convert.ToBoolean(reader["tolerance_enabled"]),
                                MarkerType = reader["marker_type"].ToString(),
                                MarkerParameters = reader["marker_parameters"].ToString(),
                                MarkerPositionX = Convert.ToDecimal(reader["marker_position_x"]),
                                MarkerPositionY = Convert.ToDecimal(reader["marker_position_y"]),
                                MarkerDisplayName = reader["marker_display_name"].ToString(),
                                MarkerColor = reader["marker_color"].ToString(),
                                Notes = reader["notes"]?.ToString(),
                                MeasurementUnit = reader["measurement_unit"]?.ToString(),
                                ToleranceUpperLimit = reader["tolerance_upper_limit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["tolerance_upper_limit"]),
                                ToleranceLowerLimit = reader["tolerance_lower_limit"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["tolerance_lower_limit"]),
                                Open = reader["open"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(reader["open"])
                            };
                            
                            measurements.Add(measurement);
                        }
                    }
                }
            }
            
            return measurements;
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

    public class PCBMeasurement
    {
        public string Id { get; set; }
        public string MeasurementSessionId { get; set; }
        public string MarkerId { get; set; }
        public int MeasurementNumber { get; set; }
        public decimal? MeasuredValue { get; set; }
        public string ToleranceStatus { get; set; }
        public decimal ToleranceUpper { get; set; }
        public decimal ToleranceLower { get; set; }
        public string ToleranceUpperType { get; set; }
        public string ToleranceLowerType { get; set; }
        public bool ToleranceEnabled { get; set; }
        public string MarkerType { get; set; }
        public string MarkerParameters { get; set; }
        public decimal MarkerPositionX { get; set; }
        public decimal MarkerPositionY { get; set; }
        public string MarkerDisplayName { get; set; }
        public string MarkerColor { get; set; }
        public string Notes { get; set; }
        public string MeasurementUnit { get; set; }
        public decimal? ToleranceUpperLimit { get; set; }
        public decimal? ToleranceLowerLimit { get; set; }
        public bool? Open { get; set; }
    }

    public class ToleranceData
    {
        public string Mode { get; set; } = "percent";
        public decimal? UpperPercent { get; set; }
        public decimal? LowerPercent { get; set; }
        public decimal? UpperAbs { get; set; }
        public decimal? LowerAbs { get; set; }
        public bool Enabled { get; set; } = true;
    }
}