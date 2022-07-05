using System;
using System.CommandLine;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace uSyncTrigger
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {

            var import = new Command("import", "Run an uSync import on the specified Umbraco instance")
            {
                new Argument<string>("url", "Umbraco url"),
                new Option<string>(new [] { "--username", "-u" }, "umbraco username"),
                new Option<string>(new [] { "--password", "-p" }, "umbraco password"),
                new Option<string?>(new [] { "--folder", "-f"}, "Folder to run import against"),
                new Option<string?>(new [] { "--group", "-g" }, "Handler group (e.g settings, content)"),
                new Option<string?>(new [] { "--set", "-s"}, "Handler set (e.g default)"),
                new Option<string?>(new [] { "--hmac", "-h"}, "HMAC authentication key"),
                new Option(new [] { "--force", "-x"}, "force import (items imported even if there is no change)"),
                new Option(new [] { "--verbose", "-v"}, "verbose output")
            }.WithHandler(nameof(HandleImport));

            import.AddValidator(validate);

            var export = new Command("export", "Run an export on the specified Umbraco instance")
            {
                new Argument<string>("url", "Umbraco url"),
                new Option<string>(new [] { "--username", "-u" }, "umbraco username"),
                new Option<string>(new [] { "--password", "-p" }, "umbraco password"),
                new Option<string?>(new [] { "--folder", "-f"}, "Folder to run export against"),
                new Option<string?>(new [] { "--group", "-g" }, "Handler group (e.g settings, content)"),
                new Option<string?>(new [] { "--set", "-s"}, "Handler set (e.g default)"),
                new Option<string?>(new [] { "--hmac", "-h"}, "HMAC authentication key"),
                new Option(new [] { "--verbose", "-v"}, "verbose output")
            }
            .WithHandler(nameof(HandleExport));

            var generate = new Command("seed", "Generate a HMAC key value for hmac auth")
                .WithHandler(nameof(HandleGenerate));

            export.AddValidator(validate);

            var cmd = new RootCommand
            {
                import, 
                export,
                generate
            };

            return await cmd.InvokeAsync(args);

        }

        static async Task<int> HandleImport(string url, string username, string password,
            string? folder, string? group, string? set, string? hmac, bool force, bool verbose, IConsole console)
        {
            var options = new TriggerOptions
            {
                Verbose = verbose,
                Folder = folder ?? string.Empty,
                Group = group ?? string.Empty,
                Set = set ?? string.Empty,
                HmacKey = hmac ?? string.Empty,
                Force = force
            };

            var handler = new uSyncTriggerHandler(url, username, password, options.HmacKey, console);
            return await handler.ImportAsync(options);
        }

        static async Task<int> HandleExport(string url, string username, string password,
            string? folder, string? group, string? set, string? hmac, bool force, bool verbose, IConsole console)
        {
            var options = new TriggerOptions
            {
                Verbose = verbose,
                Folder = folder ?? string.Empty,
                Group = group ?? string.Empty,
                Set = set ?? string.Empty,
                HmacKey = hmac ?? string.Empty,
                Force = force
            };

            var handler = new uSyncTriggerHandler(url, username, password, options.HmacKey, console);
            return await handler.ExportAsync(options);
        }


        static async Task<int> HandleGenerate(IConsole console)
        {
            console.Out.Write("[ uSync.Triggers ]\n\n");

            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                byte[] secretKeyByteArray = new byte[32]; //256 bit
                cryptoProvider.GetBytes(secretKeyByteArray);
                var key = Convert.ToBase64String(secretKeyByteArray);

                console.Out.Write("Seeding a secure HMAC key value:\n\n");
                console.Out.Write($"Generated Key = [{key}]\n\n");

                console.Out.Write("To use hmac auth add the following to an appsettings.json file:\n\n");
                console.Out.Write("\"uSync\" : {\n");
                console.Out.Write("  \"Triggers\": {\n");
                console.Out.Write("    \"Enabled\" : true\n");
                console.Out.Write($"    \"Key\" : \"{key}\"\n");
                console.Out.Write("  }\n");
                console.Out.Write("}\n\n");

                console.Out.Write("To run trigger command with hmac auth:\n\n");
                console.Out.Write($"\tdotnet run usynctrigger import <SITE UMBRACO URL> -h {key}\n\n");
            }

            return 0;
        }

        /// <summary>
        ///  validate the options
        /// </summary>
        /// <remarks>
        ///  check we have either username/password or hmac key
        ///  validate the url is a url we can parse. 
        /// </remarks>
        /// <param name="cmd"></param>
        /// <returns></returns>
        static string? validate(System.CommandLine.Parsing.CommandResult cmd)
        {
            if ((!cmd.Children.Contains("--username") || !cmd.Children.Contains("--password"))
                && !cmd.Children.Contains("--hmac"))
            {
                return "Option must include username and password or hmac key";
            }

            // check url is valid 

            var r = cmd.Children.GetByAlias("url");
            if (r != null && r.Tokens.Count == 1)
            {
                var url = r.Tokens[0].Value;
                if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                {
                    return $"Invalid url : [{url}]";
                }
            }
         
            return null;
        }

    }
}
