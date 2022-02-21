using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uSync.Triggers.Config
{
    public class uSyncTriggerConfig
    {
        public bool Enabled { get; set; } = false;
        public string Key { get; set; } = string.Empty;

        public bool FolderLimits { get; set; } = true;

    }
}
