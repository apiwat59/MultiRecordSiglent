using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MultiRecord
{
    public class AuthManager
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string BASE_URL = "http://100.87.111.69:3001/api/auth";
        
        public static string CurrentToken { get; private set; }
        public static OrbitzUser CurrentUser { get; private set; }
        
        static AuthManager()
        {
            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }
        
        public static async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var loginData = new
                {
                    email = email,
                    password = password
                };
                
                var json = JsonConvert.SerializeObject(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await httpClient.PostAsync($"{BASE_URL}/login", content);
                var responseText = await response.Content.ReadAsStringAsync();
                
                var result = JsonConvert.DeserializeObject<JObject>(responseText);
                
                if (response.IsSuccessStatusCode && result["success"]?.Value<bool>() == true)
                {
                    CurrentToken = result["token"]?.Value<string>();
                    var userObj = result["user"];
                    
                    if (userObj != null)
                    {
                        CurrentUser = new OrbitzUser
                        {
                            Id = userObj["id"]?.Value<int>() ?? 0,
                            Email = userObj["email"]?.Value<string>(),
                            Username = userObj["username"]?.Value<string>(),
                            FullName = userObj["full_name"]?.Value<string>(),
                            RoleId = userObj["role_id"]?.Value<int>() ?? 0,
                            Role = userObj["role"]?.Value<string>(),
                            RoleDisplayName = userObj["role_display_name"]?.Value<string>(),
                            IsActive = userObj["is_active"]?.Value<bool>() ?? false,
                            LastLoginAt = userObj["last_login_at"]?.Value<DateTime?>()
                        };
                    }
                    
                    return new AuthResult
                    {
                        Success = true,
                        Message = result["message"]?.Value<string>() ?? "Login successful",
                        User = CurrentUser,
                        Token = CurrentToken
                    };
                }
                else
                {
                    return new AuthResult
                    {
                        Success = false,
                        Error = result["error"]?.Value<string>() ?? "Login failed",
                        ErrorCode = result["code"]?.Value<string>()
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                return new AuthResult
                {
                    Success = false,
                    Error = $"Network error: {ex.Message}",
                    ErrorCode = "NETWORK_ERROR"
                };
            }
            catch (TaskCanceledException)
            {
                return new AuthResult
                {
                    Success = false,
                    Error = "Request timeout. Please check your connection.",
                    ErrorCode = "TIMEOUT"
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    Error = $"Unexpected error: {ex.Message}",
                    ErrorCode = "UNKNOWN_ERROR"
                };
            }
        }
        
        public static async Task<bool> VerifyTokenAsync()
        {
            if (string.IsNullOrEmpty(CurrentToken))
                return false;
                
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{BASE_URL}/verify");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentToken);
                
                var response = await httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                var result = JsonConvert.DeserializeObject<JObject>(responseText);
                
                return response.IsSuccessStatusCode && result["success"]?.Value<bool>() == true && result["valid"]?.Value<bool>() == true;
            }
            catch
            {
                return false;
            }
        }
        
        public static async Task<AuthResult> GetCurrentUserAsync()
        {
            if (string.IsNullOrEmpty(CurrentToken))
            {
                return new AuthResult
                {
                    Success = false,
                    Error = "No authentication token available",
                    ErrorCode = "NO_TOKEN"
                };
            }
                
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{BASE_URL}/me");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentToken);
                
                var response = await httpClient.SendAsync(request);
                var responseText = await response.Content.ReadAsStringAsync();
                
                var result = JsonConvert.DeserializeObject<JObject>(responseText);
                
                if (response.IsSuccessStatusCode && result["success"]?.Value<bool>() == true)
                {
                    var userObj = result["user"];
                    
                    if (userObj != null)
                    {
                        CurrentUser = new OrbitzUser
                        {
                            Id = userObj["id"]?.Value<int>() ?? 0,
                            Email = userObj["email"]?.Value<string>(),
                            Username = userObj["username"]?.Value<string>(),
                            FullName = userObj["full_name"]?.Value<string>(),
                            RoleId = userObj["role_id"]?.Value<int>() ?? 0,
                            Role = userObj["role"]?.Value<string>(),
                            RoleDisplayName = userObj["role_display_name"]?.Value<string>(),
                            IsActive = userObj["is_active"]?.Value<bool>() ?? false,
                            Phone = userObj["phone"]?.Value<string>(),
                            Address = userObj["address"]?.Value<string>(),
                            City = userObj["city"]?.Value<string>(),
                            Country = userObj["country"]?.Value<string>(),
                            Timezone = userObj["timezone"]?.Value<string>(),
                            Language = userObj["language"]?.Value<string>(),
                            Theme = userObj["theme"]?.Value<string>(),
                            LastLoginAt = userObj["last_login_at"]?.Value<DateTime?>(),
                            LoginCount = userObj["login_count"]?.Value<int>() ?? 0,
                            CreatedAt = userObj["created_at"]?.Value<DateTime?>(),
                            UpdatedAt = userObj["updated_at"]?.Value<DateTime?>()
                        };
                    }
                    
                    return new AuthResult
                    {
                        Success = true,
                        User = CurrentUser
                    };
                }
                else
                {
                    return new AuthResult
                    {
                        Success = false,
                        Error = result["error"]?.Value<string>() ?? "Failed to get user info",
                        ErrorCode = result["code"]?.Value<string>()
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    Error = $"Error getting user info: {ex.Message}",
                    ErrorCode = "USER_INFO_ERROR"
                };
            }
        }
        
        public static async Task<bool> LogoutAsync()
        {
            if (string.IsNullOrEmpty(CurrentToken))
                return true; // Already logged out
                
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{BASE_URL}/logout");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentToken);
                
                var response = await httpClient.SendAsync(request);
                
                // Clear local session regardless of server response
                CurrentToken = null;
                CurrentUser = null;
                
                return response.IsSuccessStatusCode;
            }
            catch
            {
                // Clear local session even if logout request fails
                CurrentToken = null;
                CurrentUser = null;
                return false;
            }
        }
        
        public static bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(CurrentToken) && CurrentUser != null;
        }
        
        public static void ClearSession()
        {
            CurrentToken = null;
            CurrentUser = null;
        }
        
        /// <summary>
        /// Role IDs constants (ตรงกับค่าจริงในฐานข้อมูล)
        /// </summary>
        public const int ROLE_ADMIN = 1;
        public const int ROLE_MANAGER = 3;
        public const int ROLE_RD = 7;
        public const int ROLE_STAFF = 9;
        
        /// <summary>
        /// ตรวจสอบว่า role ปัจจุบันมีสิทธิ์เข้าถึงทุก tabs หรือไม่
        /// </summary>
        public static bool HasFullAccess()
        {
            if (CurrentUser == null)
                return false;
            
            return CurrentUser.RoleId == ROLE_ADMIN || CurrentUser.RoleId == ROLE_MANAGER;
        }
        
        /// <summary>
        /// ตรวจสอบว่า role ปัจจุบันสามารถเข้าถึง R&D tab ได้หรือไม่
        /// </summary>
        public static bool CanAccessRDTab()
        {
            if (CurrentUser == null)
                return false;
            
            return CurrentUser.RoleId == ROLE_ADMIN || 
                   CurrentUser.RoleId == ROLE_MANAGER || 
                   CurrentUser.RoleId == ROLE_RD;
        }
        
        /// <summary>
        /// ตรวจสอบว่า role ปัจจุบันสามารถเข้าถึง Repair tab ได้หรือไม่
        /// </summary>
        public static bool CanAccessRepairTab()
        {
            if (CurrentUser == null)
                return false;
            
            return CurrentUser.RoleId == ROLE_ADMIN || 
                   CurrentUser.RoleId == ROLE_MANAGER || 
                   CurrentUser.RoleId == ROLE_RD || 
                   CurrentUser.RoleId == ROLE_STAFF;
        }
        
        /// <summary>
        /// ตรวจสอบว่า role ปัจจุบันสามารถเข้าถึง Settings tab ได้หรือไม่
        /// </summary>
        public static bool CanAccessSettingsTab()
        {
            if (CurrentUser == null)
                return false;
            
            return CurrentUser.RoleId == ROLE_ADMIN || CurrentUser.RoleId == ROLE_MANAGER;
        }
    }
    
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public string ErrorCode { get; set; }
        public OrbitzUser User { get; set; }
        public string Token { get; set; }
    }
    
    public class OrbitzUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public int RoleId { get; set; }
        public string Role { get; set; }
        public string RoleDisplayName { get; set; }
        public bool IsActive { get; set; }
        public string ProfilePicture { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Timezone { get; set; }
        public string Language { get; set; }
        public string Theme { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int LoginCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}