using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Security;

using uSync.Triggers.Config;

namespace uSync.Triggers.Auth
{
    internal class uSyncTriggerAuthenticationHandler : AuthenticationHandler<uSyncTriggerAuthenticationOptions>
    {
        private const string c_authorizationHeader = "Authorization";

        private readonly ILogger<uSyncTriggerAuthenticationHandler> _logger;
        private readonly IOptionsMonitor<uSyncTriggerConfig> _triggerConfig;

        private readonly IUserService _userService;
        private readonly IBackOfficeSignInManager _backOfficeSignInManager;
        private readonly IUmbracoMapper _umbracoMapper;

        public uSyncTriggerAuthenticationHandler(
            IOptionsMonitor<uSyncTriggerConfig> triggerConfig, 
            IOptionsMonitor<uSyncTriggerAuthenticationOptions> options,
            ILoggerFactory logger,
            IUserService userService,
            IBackOfficeSignInManager backOfficeSignInManager,
            IUmbracoMapper umbracoMapper,
            UrlEncoder urlEncoder,
            ISystemClock clock)
            : base(options, logger, urlEncoder, clock)
        {
            _logger = logger.CreateLogger<uSyncTriggerAuthenticationHandler>();
            _triggerConfig = triggerConfig;            
            
            _userService = userService; 
            _backOfficeSignInManager = backOfficeSignInManager;
            _umbracoMapper = umbracoMapper;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (IsValidRequest())
            {
                var ticket = await GetUmbracoAuthTicket();
                return AuthenticateResult.Success(ticket);
            }

            return AuthenticateResult.Fail(new AccessViolationException("Invalid Auth"));
        }

        private bool IsValidRequest()
        {
            var hmackey = _triggerConfig.CurrentValue.Key;
            if (string.IsNullOrWhiteSpace(hmackey)) return false;

            var headerContent = Request.Headers[c_authorizationHeader].SingleOrDefault();
            if (headerContent == null) return false;

            var authHeader = headerContent.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (authHeader.Length != 2) return false;

            var scheme = authHeader[0];
            if (!scheme.Equals(uSyncTriggerAuthenticationOptions.DefaultScheme)) return false;

            var parameters = GetHmacAuthHeader(authHeader[1]);
            if (parameters == null) return false;

            // stop multiple requests with same nonce value
            if (MemoryCache.Default.Contains(parameters.Nonce)) return false;

            var timestampTime = DateTimeOffset.FromUnixTimeSeconds(parameters.Timestamp);
            if ( (DateTime.UtcNow - timestampTime).TotalSeconds > Options.AllowedDateDrift.TotalSeconds) return false;

            // if the sig is ok, 
            if (CheckSigniture(hmackey, parameters, Request)) return true;

            return false;
        }

        private HmacParameters GetHmacAuthHeader(string authHeader)
        {
            var array = authHeader.Split(":");
            if (array.Length != 3) return null;

            return new HmacParameters
            {
                Signature = array[0],
                Nonce = array[1],
                Timestamp = Convert.ToInt64(array[2])
            };
        }

        private bool CheckSigniture(string key, HmacParameters parameters, HttpRequest request)
        {
            var token = $"{request.Method}" +
                $"{parameters.Timestamp}" +
                $"{parameters.Nonce}" +
                $"{request.ContentLength ?? 0}";

            var secretBytes = Convert.FromBase64String(key);
            var tokenBytes = Encoding.UTF8.GetBytes(token);

            using (HMACSHA256 hmac = new HMACSHA256(secretBytes))
            {
                var hashed = hmac.ComputeHash(tokenBytes);
                string stringToken = Convert.ToBase64String(hashed);

                var match = parameters.Signature.Equals(stringToken, StringComparison.Ordinal);
                if (match)
                    MemoryCache.Default.Add(parameters.Nonce, parameters.Timestamp, DateTime.Now.AddMinutes(180));

                return match;
            }
        }

        private async Task<AuthenticationTicket> GetUmbracoAuthTicket()
        {
            var user = _userService.GetUserById(Constants.Security.SuperUserId);
            var authUser = _umbracoMapper.Map<IUser, BackOfficeIdentityUser>(user);

            var principle = await _backOfficeSignInManager.CreateUserPrincipalAsync(authUser);
            var ticket = new AuthenticationTicket(principle, Scheme.Name);
            return ticket;

        }
    }
}
