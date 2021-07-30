using System.CommandLine;
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
                new Option<string>(new [] { "--username", "-u" }, "umbraco username") { IsRequired = true },
                new Option<string>(new [] { "--password", "-p" }, "umbraco password") { IsRequired = true },
                new Option<string?>(new [] { "--folder", "-f"}, "Folder to run import against"),
                new Option<string?>(new [] { "--group", "-g" }, "Handler group (e.g settings, content)"),
                new Option<string?>(new [] { "--set", "-s"}, "Handler set (e.g default)"),
                new Option(new [] { "--force", "-x"}, "force import (items imported even if there is no change)"),
                new Option(new [] { "--verbose", "-v"}, "verbose output")
            }.WithHandler(nameof(HandleImport));

            var export = new Command("export", "Run an export on the specified Umbraco instance")
            {
                new Argument<string>("url", "Umbraco url"),               
                new Option<string>(new [] { "--username", "-u" }, "umbraco username") { IsRequired = true },
                new Option<string>(new [] { "--password", "-p" }, "umbraco password") { IsRequired = true },
                new Option<string?>(new [] { "--folder", "-f"}, "Folder to run export against"),
                new Option<string?>(new [] { "--group", "-g" }, "Handler group (e.g settings, content)"),
                new Option<string?>(new [] { "--set", "-s"}, "Handler set (e.g default)"),
                new Option(new [] { "--verbose", "-v"}, "verbose output")            
            }.WithHandler(nameof(HandleExport));


            var cmd = new RootCommand
            {
                import, 
                export
            };

            return await cmd.InvokeAsync(args);

        }

        static async Task HandleImport(string url, string username, string password, 
            string? folder, string? group, string? set, bool force, bool verbose, IConsole console)
        {

            var options = new TriggerOptions
            {
                Verbose = verbose,
                Folder = folder ?? string.Empty,
                Group = group ?? string.Empty,
                Set = set ?? string.Empty,
                Force = force   
            };

            using (var handler = new uSyncTriggerHandler(url, username, password, console))
            {
                await handler.ImportAsync(options);
            }
        }

        static async Task HandleExport(string url, string username, string password,
            string? folder, string? group, string? set, bool force, bool verbose, IConsole console)
        {

            var options = new TriggerOptions
            {
                Verbose = verbose,
                Folder = folder ?? string.Empty,
                Group = group ?? string.Empty,
                Set = set ?? string.Empty,
                Force = force
            };

            using (var handler = new uSyncTriggerHandler(url, username, password, console))
            {
                await handler.ExportAsync(options);
            }
        }
    }
}
