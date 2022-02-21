using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using System;
using System.IO;
using System.Linq;

using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Controllers;

using uSync.BackOffice;
using uSync.BackOffice.Configuration;
using uSync.BackOffice.Services;
using uSync.BackOffice.SyncHandlers;
using uSync.Triggers.Config;

namespace uSync.Triggers.Controllers
{
    [PluginController("uSync")]
    [Authorize(AuthenticationSchemes = uSyncTriggers.AuthScheme)]
    public class TriggerController : UmbracoApiController
    {
        private readonly uSyncService _uSyncService;
        private readonly uSyncConfigService _uSyncConfigService;
        private readonly SyncFileService _syncFileService;
        private readonly IOptionsMonitor<uSyncTriggerConfig> _uSyncTriggerConfig;

        public TriggerController(
            IOptionsMonitor<uSyncTriggerConfig> triggerConfig,
            uSyncService uSyncService, 
            uSyncConfigService uSyncConfigService, 
            SyncFileService syncFileService)
        {
            _uSyncService = uSyncService;
            _uSyncConfigService = uSyncConfigService;
            _syncFileService = syncFileService;
            _uSyncTriggerConfig = triggerConfig;
        }

        [HttpGet]
        public object Import(string group = "", string set = "", string folder = "", bool force = false, bool verbose = false)
        {
            var options = GetOptions(group, set, folder, force, verbose);
            return Import(options);
        }

        [HttpPost]
        public object Import(TriggerOptions options)
        {
            EnsureEnabled();
            EnsureOptions(options);

            var handlerOptions = new SyncHandlerOptions
            {
                Group = options.Group,
                Set = options.Set
            };

            var results = _uSyncService.Import(options.Folder, options.Force, handlerOptions);

            if (options.Verbose)
                return results.Where(x => x.Change != Core.ChangeType.NoChange);
            else
                return $"{results.CountChanges()} Changes in {results.Count()} items";
        }

        [HttpGet]
        public object Export(string group = "", string set = "", string folder = "", bool force = false, bool verbose = false)
        {
            var options = GetOptions(group, set, folder, force, verbose);
            return Export(options);
        }

        [HttpPost]
        public string Export(TriggerOptions options)
        {
            EnsureEnabled();
            EnsureOptions(options);

            var handlerOptions = new SyncHandlerOptions
            {
                Group = options.Group,
                Set = options.Set
            };

            var result = _uSyncService.Export(options.Folder, handlerOptions);

            return $"{result.Count()} exported items";
        }


        ////
        ////
        ////

        private TriggerOptions GetOptions(string group, string set, string folder ,bool force, bool verbose)
        {
            var options = new TriggerOptions
            {
                Group = group,
                Set = set,
                Folder = folder,
                Force = force,
                Verbose = verbose
            };
            
            EnsureOptions(options);

            return options;
        }

        private void EnsureOptions(TriggerOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Group))
                options.Group = _uSyncConfigService.Settings.UIEnabledGroups;

            if (string.IsNullOrWhiteSpace(options.Set))
                options.Set = _uSyncConfigService.Settings.DefaultSet;

            if (string.IsNullOrWhiteSpace(options.Folder))
                options.Folder = _uSyncConfigService.Settings.RootFolder;

            if (options.Group.Equals("all", StringComparison.InvariantCultureIgnoreCase))
                options.Group = "";

            var folderLimits = _uSyncTriggerConfig.CurrentValue.FolderLimits;

            if (folderLimits)
            {
                var absPath = _syncFileService.GetAbsPath(options.Folder);
                var rootPath = Path.GetFullPath(Path.GetDirectoryName(
                    _uSyncConfigService.Settings.RootFolder.TrimEnd(Path.DirectorySeparatorChar)));

                if (!absPath.StartsWith(rootPath, StringComparison.InvariantCultureIgnoreCase))
                    throw new AccessViolationException($"Cannot access {absPath} out of uSync folder limits {rootPath}");
            }

            // else - there are no limits, you can access anywhere on the disk. 
        }

        private void EnsureEnabled()
        {
            if (!_uSyncTriggerConfig.CurrentValue.Enabled)
                throw new NotSupportedException("Triggers is disabled");
        }

    }

    public class TriggerOptions
    {
        public string Group { get; set; }
        public string Set { get; set; }
        public string Folder { get; set; }

        public bool Force { get; set; } 
        public bool Verbose { get; set; }
    }
}
