using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwaAPITests
{
    public static class Configuration
    {
        public static string Hostname = "http://localhost";
        public static bool UseAPIKeyAuth = true;
        // APIKey is only used if UseAPIKeyAuth is true
        public static string APIKey = "dOmYbQy_Oivw94cWd3wB7dszVf0ru6JGcI81qKJ04FA";
        // Credentials below are only used if UseAPIKeyAuth is false
        public static string Credentials_Username = "Admin";
        public static string Credentials_Password = "password";
    }
}
