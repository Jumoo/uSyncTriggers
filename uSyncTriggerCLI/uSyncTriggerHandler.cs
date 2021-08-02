using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.CommandLine;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace uSyncTrigger
{
    public class uSyncTriggerHandler : IDisposable
    {

        private IConsole _console;

        HttpClient _client;
        public uSyncTriggerHandler(string url, string username, string password, IConsole console)
        {
            _console = console;

            _client = GetClient(url, username, password);
        }

        public async Task<int> ImportAsync(TriggerOptions options)
            => await RunRemoteCommandAsync(uSyncTrigger.ImportUrl, options);

        public async Task<int> ExportAsync(TriggerOptions options)
            => await RunRemoteCommandAsync(uSyncTrigger.ExportUrl, options);

        private async Task<int> RunRemoteCommandAsync(string url, TriggerOptions options)
        {
            _console.Out.Write($"Contacting : {_client.BaseAddress}{uSyncTrigger.ImportUrl}\n");
            try
            {
                var sw = Stopwatch.StartNew();

                var response = await _client.PostAsJsonAsync(url, options);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (options.Verbose)
                    {
                        ShowVerbose(content);
                    }
                    else
                    {
                        _console.Out.Write($"{content}\n");
                    }
                }
                else
                {
                    _console.Out.Write($"Error      : {response.StatusCode}\n\t{content}\n");
                }

                sw.Stop();
                _console.Out.Write($"Completed  : [{response.StatusCode}] {sw.Elapsed.TotalSeconds:N2} Seconds\n");
                return response.IsSuccessStatusCode ? 0 : 1;
            }
            catch (Exception ex)
            {
                _console.Out.Write($"Exception  : {ex.Message}");
                return 1; 
            }
        }

        private void ShowVerbose(string content)
        {
            var results = JsonConvert.DeserializeObject<JArray>(content);
            _console.Out.Write($"Returned   : {results.Count} changes \n");


            if (results.Count > 0)
            {
                _console.Out.Write($"{"Change",-7} - {"",-5} - {"itemType",-14} - {"Name"} - {"Message"}\n");
                _console.Out.Write($"{new string('-', 50)} \n");

                foreach (var result in results)
                {
                    var itemType = CleanItemType(result.Value<string>("ItemType"));

                    _console.Out.Write($"{result["Change"],-7} - {result["Success"],-5} - {itemType,-14} - {result["Name"]} - {result["Message"]}\n");
                }

                _console.Out.Write("\n");
            }
        }

        private string CleanItemType(string itemType)
        {
            if (itemType.IndexOf(',') > 0)
            {
                var cleanType = itemType.Substring(0, itemType.IndexOf(','));
                return cleanType.Substring(cleanType.LastIndexOf('.') + 1);
            }
            return itemType;
        }


        private HttpClient GetClient(string url, string username, string password)
        {
            var credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));

            var baseUrl = url.EndsWith('/') ? url : $"{url}/";

            var client = new HttpClient()
            {
                BaseAddress = new Uri(baseUrl),
            };

            client.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue("Basic", credentials);

            return client;

        }

        public void Dispose()
        {
            if (_client != null) _client.Dispose(); 
        }
    }
}
