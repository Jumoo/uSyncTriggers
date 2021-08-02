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
    public class uSyncTriggerHandler
    {

        private IConsole _console;

        HttpClient _client;
        public uSyncTriggerHandler(string url, string username, string password, string hmacKey, IConsole console)
        {
            _console = console;


            if (string.IsNullOrWhiteSpace(hmacKey))
            {
                _console.Out.Write("[Basic] ");
                _client = GetClient(url, username, password);
            }
            else
            {
                _console.Out.Write("[HMAC] ");
                _client = GetHmacClient(url, hmacKey);
            }
        }

        public async Task<int> ImportAsync(TriggerOptions options)
            => await RunRemoteCommandAsync(uSyncTrigger.ImportUrl, options);

        public async Task<int> ExportAsync(TriggerOptions options)
            => await RunRemoteCommandAsync(uSyncTrigger.ExportUrl, options);

        private async Task<int> RunRemoteCommandAsync(string url, TriggerOptions options)
        {
            _console.Out.Write($"Contacting : {_client.BaseAddress}{url}\n");
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
                        // dislplay output (remove any string / array boundry)
                        _console.Out.Write($"{content.Trim('\"', '[', ']')}\n");
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
            => GetBaseClient(url, new BasicHandler(username, password));
        private HttpClient GetHmacClient(string url, string hmacKey)
            => GetBaseClient(url, new HmacHandler(hmacKey));

        private HttpClient GetBaseClient(string url, DelegatingHandler handler)
        {
            var baseUrl = url.EndsWith('/') ? url : $"{url}/";
            var client = HttpClientFactory.Create(handler);
            client.BaseAddress = new Uri(baseUrl);
            return client;
        }

    }
}
