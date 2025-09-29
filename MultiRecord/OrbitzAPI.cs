using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MultiRecord
{
    public class OrbitzAPI
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string BASE_URL = "http://100.75.21.95:3001/api";
        
        static OrbitzAPI()
        {
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }
        
        private static void SetAuthToken()
        {
            if (!string.IsNullOrEmpty(AuthManager.CurrentToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthManager.CurrentToken);
            }
        }
        
        // Get Measurement Sessions
        public static async Task<List<MeasurementSession>> GetMeasurementSessionsAsync(string repairId, string pcbTestGroupId = null, string pcbTestRecordId = null)
        {
            try
            {
                SetAuthToken();
                
                var queryParams = new List<string> { $"repair_id={repairId}" };
                if (!string.IsNullOrEmpty(pcbTestGroupId))
                    queryParams.Add($"pcb_test_group_id={pcbTestGroupId}");
                if (!string.IsNullOrEmpty(pcbTestRecordId))
                    queryParams.Add($"pcb_test_record_id={pcbTestRecordId}");
                
                string queryString = string.Join("&", queryParams);
                var response = await httpClient.GetAsync($"{BASE_URL}/measurement-sessions?{queryString}");
                var responseText = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<JObject>(responseText);
                    if (result["success"]?.Value<bool>() == true)
                    {
                        var data = result["data"]?.ToObject<List<MeasurementSession>>();
                        return data ?? new List<MeasurementSession>();
                    }
                }
                
                throw new Exception($"Failed to get measurement sessions: {responseText}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting measurement sessions: {ex.Message}");
            }
        }
        
        // Get Specific Measurement Session
        public static async Task<MeasurementSession> GetMeasurementSessionAsync(string sessionId)
        {
            try
            {
                SetAuthToken();
                
                var response = await httpClient.GetAsync($"{BASE_URL}/measurement-sessions/{sessionId}");
                var responseText = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<JObject>(responseText);
                    if (result["success"]?.Value<bool>() == true)
                    {
                        var data = result["data"]?.ToObject<MeasurementSession>();
                        return data;
                    }
                }
                
                throw new Exception($"Failed to get measurement session: {responseText}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting measurement session: {ex.Message}");
            }
        }
        
        // Get Measurement Markers
        public static async Task<List<MeasurementMarker>> GetMeasurementMarkersAsync(string sessionId)
        {
            try
            {
                SetAuthToken();
                
                var response = await httpClient.GetAsync($"{BASE_URL}/measurement-sessions/{sessionId}/measurement-markers");
                var responseText = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<JObject>(responseText);
                    if (result["success"]?.Value<bool>() == true)
                    {
                        var markers = result["markers"]?.ToObject<List<MeasurementMarker>>();
                        return markers ?? new List<MeasurementMarker>();
                    }
                }
                
                throw new Exception($"Failed to get measurement markers: {responseText}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting measurement markers: {ex.Message}");
            }
        }
        
        // Update Measurement
        public static async Task<bool> UpdateMeasurementAsync(string repairId, string measurementId, MeasurementUpdateRequest request)
        {
            try
            {
                SetAuthToken();
                
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await httpClient.PutAsync($"{BASE_URL}/repairs/{repairId}/measurements/{measurementId}", content);
                var responseText = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<JObject>(responseText);
                    return result["success"]?.Value<bool>() == true;
                }
                
                throw new Exception($"Failed to update measurement: {responseText}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating measurement: {ex.Message}");
            }
        }
    }
    
    // Data Models
    public class MeasurementSession
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("repair_id")]
        public string RepairId { get; set; }
        
        [JsonProperty("pcb_test_group_id")]
        public string PcbTestGroupId { get; set; }
        
        [JsonProperty("pcb_test_record_id")]
        public string PcbTestRecordId { get; set; }
        
        [JsonProperty("session_name")]
        public string SessionName { get; set; }
        
        [JsonProperty("session_description")]
        public string SessionDescription { get; set; }
        
        [JsonProperty("session_number")]
        public int? SessionNumber { get; set; }
        
        [JsonProperty("measurement_type")]
        public string MeasurementType { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }
        
        [JsonProperty("test_configuration_version")]
        public string TestConfigurationVersion { get; set; }
        
        [JsonProperty("marker_snapshot_created_at")]
        public DateTime? MarkerSnapshotCreatedAt { get; set; }
        
        [JsonProperty("session_started_at")]
        public DateTime? SessionStartedAt { get; set; }
        
        [JsonProperty("session_completed_at")]
        public DateTime? SessionCompletedAt { get; set; }
        
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        [JsonProperty("created_by")]
        public int? CreatedBy { get; set; }
        
        [JsonProperty("updated_by")]
        public int? UpdatedBy { get; set; }
        
        [JsonProperty("test_group_name")]
        public string TestGroupName { get; set; }
        
        [JsonProperty("test_group_description")]
        public string TestGroupDescription { get; set; }
        
        [JsonProperty("created_by_name")]
        public string CreatedByName { get; set; }
        
        [JsonProperty("updated_by_name")]
        public string UpdatedByName { get; set; }
        
        [JsonProperty("total_measurements")]
        public int? TotalMeasurements { get; set; }
        
        [JsonProperty("tested_measurements")]
        public int? TestedMeasurements { get; set; }
        
        [JsonProperty("passed_measurements")]
        public int? PassedMeasurements { get; set; }
        
        [JsonProperty("failed_measurements")]
        public int? FailedMeasurements { get; set; }
        
        [JsonProperty("progress_percentage")]
        public int? ProgressPercentage { get; set; }
    }
    
    public class MeasurementMarker
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("parameters")]
        public Dictionary<string, object> Parameters { get; set; }
        
        [JsonProperty("x")]
        public decimal X { get; set; }
        
        [JsonProperty("y")]
        public decimal Y { get; set; }
        
        [JsonProperty("color")]
        public string Color { get; set; }
        
        [JsonProperty("test_group_id")]
        public string TestGroupId { get; set; }
        
        [JsonProperty("sort_order")]
        public int SortOrder { get; set; }
        
        [JsonProperty("tolerance_upper")]
        public decimal? ToleranceUpper { get; set; }
        
        [JsonProperty("tolerance_lower")]
        public decimal? ToleranceLower { get; set; }
        
        [JsonProperty("tolerance_upper_type")]
        public string ToleranceUpperType { get; set; }
        
        [JsonProperty("tolerance_lower_type")]
        public string ToleranceLowerType { get; set; }
        
        [JsonProperty("tolerance_enabled")]
        public bool ToleranceEnabled { get; set; }
        
        [JsonProperty("notes")]
        public string Notes { get; set; }
        
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        [JsonProperty("created_by_name")]
        public string CreatedByName { get; set; }
        
        [JsonProperty("measurement_id")]
        public string MeasurementId { get; set; }
        
        [JsonProperty("measured_value")]
        public decimal? MeasuredValue { get; set; }
        
        [JsonProperty("tolerance_status")]
        public string ToleranceStatus { get; set; }
        
        [JsonProperty("tolerance_upper_limit")]
        public decimal? ToleranceUpperLimit { get; set; }
        
        [JsonProperty("tolerance_lower_limit")]
        public decimal? ToleranceLowerLimit { get; set; }
        
        [JsonProperty("measurement_unit")]
        public string MeasurementUnit { get; set; }
        
        [JsonProperty("open_circuit")]
        public bool OpenCircuit { get; set; }
        
        [JsonProperty("measurement_number")]
        public int MeasurementNumber { get; set; }
        
        [JsonProperty("is_measured")]
        public bool IsMeasured { get; set; }
    }
    
    public class MeasurementUpdateRequest
    {
        [JsonProperty("marker_id")]
        public string MarkerId { get; set; }
        
        [JsonProperty("measured_value")]
        public decimal? MeasuredValue { get; set; }
        
        [JsonProperty("tolerance_status")]
        public string ToleranceStatus { get; set; }
        
        [JsonProperty("measurement_id")]
        public string MeasurementId { get; set; }
        
        [JsonProperty("marker_type")]
        public string MarkerType { get; set; }
        
        [JsonProperty("marker_parameters")]
        public Dictionary<string, object> MarkerParameters { get; set; }
        
        [JsonProperty("marker_position_x")]
        public decimal MarkerPositionX { get; set; }
        
        [JsonProperty("marker_position_y")]
        public decimal MarkerPositionY { get; set; }
        
        [JsonProperty("marker_display_name")]
        public string MarkerDisplayName { get; set; }
        
        [JsonProperty("marker_color")]
        public string MarkerColor { get; set; }
        
        [JsonProperty("tolerance_upper")]
        public decimal? ToleranceUpper { get; set; }
        
        [JsonProperty("tolerance_lower")]
        public decimal? ToleranceLower { get; set; }
        
        [JsonProperty("tolerance_upper_type")]
        public string ToleranceUpperType { get; set; }
        
        [JsonProperty("tolerance_lower_type")]
        public string ToleranceLowerType { get; set; }
        
        [JsonProperty("tolerance_enabled")]
        public bool ToleranceEnabled { get; set; }
        
        [JsonProperty("tolerance_upper_limit")]
        public decimal? ToleranceUpperLimit { get; set; }
        
        [JsonProperty("tolerance_lower_limit")]
        public decimal? ToleranceLowerLimit { get; set; }
        
        [JsonProperty("measurement_unit")]
        public string MeasurementUnit { get; set; }
        
        [JsonProperty("notes")]
        public string Notes { get; set; }
        
        [JsonProperty("open")]
        public bool? Open { get; set; }
    }
}