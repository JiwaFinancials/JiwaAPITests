using JiwaFinancials.Jiwa.JiwaServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ServiceStack.Diagnostics.Events;

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
            random = new Random();

            Client = new ServiceStack.JsonApiClient(Configuration.Hostname)
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
