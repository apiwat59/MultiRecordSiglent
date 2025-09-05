using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                ConnectionTimeout = 30,
                AllowUserVariables = true,
                UseCompression = true,
                CharacterSet = "utf8mb4"
            };
            _connectionString = builder.ToString();
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
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

        public void Dispose()
        {
            if (!_disposed)
            {
                _connection?.Dispose();
                _disposed = true;
            }
        }
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