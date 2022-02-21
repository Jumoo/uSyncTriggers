using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

using uSync.Triggers.Config;

namespace uSync.Triggers.Auth
{
    internal class uSyncTriggerAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "hmacauth";
        public string Scheme => DefaultScheme;
        public TimeSpan AllowedDateDrift => TimeSpan.FromSeconds(120);
        public Func<string, string[]> GetRolesForId { get; set; }
    }
}
