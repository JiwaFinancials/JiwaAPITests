using JiwaFinancials.Jiwa.JiwaServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using static ServiceStack.Diagnostics.Events;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using static ServiceStack.Diagnostics;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Text;

namespace JiwaAPITests
{
    // This is a base class our tests inherit from, to reduce some boiler plate code.    
    [TestFixture]    
    public class JiwaAPITest
    {
        private Random random;

        public ServiceStack.JsonApiClient Client;
        public System.Net.HttpStatusCode LastHttpStatusCode;        

        [SetUp]
        public async Task Setup()
        {
            if (! System.IO.File.Exists("config.json"))
            {
                // Create a config file if not present
                ConfigDTO newConfig = new ConfigDTO();
                newConfig.Hostname = "http://localhost";
                newConfig.UseAPIKeyAuth = true;
                newConfig.APIKey = "dOmYbQy_Oivw94cWd3wB7dszVf0ru6JGcI81qKJ04FA";
                newConfig.Credentials_Username = "Admin";
                newConfig.Credentials_Password = "password";

                System.IO.File.WriteAllText("config.json", newConfig.ToJson().IndentJson());
            }

            ConfigDTO config = System.IO.File.ReadAllText("config.json").FromJson<ConfigDTO>();
            Configuration.Hostname = config.Hostname;
            Configuration.UseAPIKeyAuth = config.UseAPIKeyAuth;
            Configuration.APIKey = config.APIKey;
            Configuration.Credentials_Username = config.Credentials_Username;
            Configuration.Credentials_Password = config.Credentials_Password;

            if (string.IsNullOrWhiteSpace(Configuration.Hostname))
            {
                throw new Exception("Hostname in App.config is missing or not set");
            }            

            if (Configuration.UseAPIKeyAuth && string.IsNullOrWhiteSpace(Configuration.APIKey))
            {
                throw new Exception("APIKey in App.config is missing or not set, and must be provided when UseAPIKeyAuth = true");
            }

            if (!Configuration.UseAPIKeyAuth && string.IsNullOrWhiteSpace(Configuration.Credentials_Username))
            {
                throw new Exception("Credentials_Username in App.config is missing or not set, and must be provided when UseAPIKeyAuth = false");
            }

            if (!Configuration.UseAPIKeyAuth && string.IsNullOrWhiteSpace(Configuration.Credentials_Password))
            {
                throw new Exception("Credentials_Password in App.config is missing or not set, and must be provided when UseAPIKeyAuth = false");
            }

            random = new Random();

            Client = new ServiceStack.JsonApiClient(config.Hostname)
            {
                ResponseFilter = res => LastHttpStatusCode = res.StatusCode,                
            };
            
            ServiceStack.ClientConfig.SkipEmptyArrays = true;            

            if (Configuration.UseAPIKeyAuth)
            {
                Auth(Configuration.APIKey);
            }
            else
            {
                await Auth(Configuration.Credentials_Username, Configuration.Credentials_Password);
            }
        }

        public void Auth(string APIKey)
        {            
            Client.BearerToken = Configuration.APIKey;
        }

        public async Task Auth(string username, string password)
        {                     
            ServiceStack.AuthenticateResponse authRes = await Client.PostAsync(new LoginPOSTRequest() { UserName = Configuration.Credentials_Username, Password = Configuration.Credentials_Password });            
        }        

        [TearDown]
        public void TearDown()
        {
            Client.Dispose();
        }

        public string RandomString(int length)
        {            
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(characters, length).Select(x => x[random.Next(x.Length)]).ToArray());        
        }

            }
        }
