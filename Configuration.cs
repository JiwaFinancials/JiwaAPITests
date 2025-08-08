using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests
{
    public static class Configuration
    {
        public static string Hostname;
        public static bool UseAPIKeyAuth;
        // APIKey is only used if UseAPIKeyAuth is true
        public static string APIKey;
        // Credentials below are only used if UseAPIKeyAuth is false
        public static string Credentials_Username;
        public static string Credentials_Password;
    }

    public class ConfigDTO
    {
        public string? Hostname { get; set; }
        public bool UseAPIKeyAuth { get; set; }
        // APIKey is only used if UseAPIKeyAuth is true
        public string? APIKey { get; set; }
        // Credentials below are only used if UseAPIKeyAuth is false
        public string? Credentials_Username { get; set; }
        public string? Credentials_Password { get; set; }
    }
}
