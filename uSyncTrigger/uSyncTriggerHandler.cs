using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace uSyncTrigger
{
    public class uSyncTriggerHandler : IDisposable
    {
        private const string _triggerBase = "usync/trigger";
        private static string _importUrl = _triggerBase + "/import" ;
        private static string _exportUrl = _triggerBase + "/export" ;

        private IConsole _console;

        HttpClient _client;
        public uSyncTriggerHandler(string url, string username, string password, IConsole console)
        {
            _console = console;

            _client = GetClient(url, username, password);
        }

        public async Task ImportAsync(TriggerOptions options)
        {
            _console.Out.Write($"Contacting Site : {_client.BaseAddress}{_importUrl}\n");

            var response = await _client.PostAsJsonAsync(_importUrl, options);

            _console.Out.Write($"{response.StatusCode}\n");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _console.Out.Write($"{content}\n");
            }
        }

        public async Task ExportAsync(TriggerOptions options)
        {
            _console.Out.Write($"Contacting Site : {_client.BaseAddress}{_exportUrl}\n");

            var response = await _client.PostAsJsonAsync(_exportUrl, options);

            _console.Out.Write($"{response.StatusCode}\n");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _console.Out.Write($"{content}\n");
            }
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
