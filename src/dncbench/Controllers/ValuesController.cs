using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

using System.IO;
using System.Text;

namespace dncbench.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        //IConfiguration iconfig;
        //static string[] _values = {"Apple", "Banana", "Cat", "Dog", "Elephant", "Flower", "Goose", "Hat", "Ice", "Jelly"};
        string[] _values = null;
        string blob_acct, blob_cont, blob_file;
        string kv_acct, kv_secret;

        public ValuesController(IConfiguration _config)
        {
            //iconfig = _config;
            _values = _config["Test:Values"].Split(";");
            blob_acct = _config["Test:Blob:account"];
            blob_cont = _config["Test:Blob:container"];
            blob_file = _config["Test:Blob:file"];
            kv_acct = _config["Test:KeyVault:account"];
            kv_secret = _config["Test:KeyVault:secret"];
        }

        [HttpGet("/health")]
        public ActionResult<string> HealthCheck()
        {
            return "Ok";
        }

        [HttpGet("/blob")]
        public async Task<ActionResult<string>> GetBlob()
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://storage.azure.com/");//, tenantId);
            TokenCredential tokenCredential = new TokenCredential(accessToken);
            StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);

            try
            {
                CloudBlobClient cloudBlobClient = new CloudBlobClient(new StorageUri(new Uri($"https://{blob_acct}.blob.core.windows.net")), storageCredentials);

                CloudBlobContainer container = cloudBlobClient.GetContainerReference($"{blob_cont}");
                CloudBlockBlob blob = container.GetBlockBlobReference($"{blob_file}");

                MemoryStream blobtxt = new MemoryStream();
                await blob.DownloadToStreamAsync(blobtxt);

                string text = "none";
                using (StreamReader reader = new StreamReader(blobtxt))
                {
                    blobtxt.Position = 0;
                    text = reader.ReadToEnd();
                }

                return text;
            }
            catch (Exception ex)
            {
                return ($"Something went wrong: {ex.Message}");

            }
        }

        // GET api/values
        [HttpGet("/keyvault")]
        public async Task<ActionResult<string>> GetKeyVault()
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
            KeyVaultClient keyVaultClient =
                new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            try
            {
                var secret = await keyVaultClient
                    .GetSecretAsync($"https://{kv_acct}.vault.azure.net/secrets/{kv_secret}")
                    .ConfigureAwait(false);

                return secret.Value;
            }
            catch (Exception ex)
            {
                return ($"Something went wrong: {ex.Message}");
            }
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Random rand = new Random();
            int r = rand.Next(10);

            return new string[] { "value1", "value2", _values[r] };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return _values[id];
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
