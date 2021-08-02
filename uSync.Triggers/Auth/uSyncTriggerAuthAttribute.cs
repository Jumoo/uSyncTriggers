using Microsoft.AspNet.SignalR;

using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Runtime.Remoting.Contexts;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Web.Security;

using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;

namespace uSync.Triggers.Auth
{
    public class uSyncTriggerAuthAttribute : Attribute, IAuthenticationFilter
    {
        private const string BasicScheme = "Basic";
        private const string HmacScheme = "hmacauth";

        public uSyncTriggerAuthAttribute() { }

        public bool AllowMultiple => false;

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;
            var authorization = request.Headers.Authorization;
            if (authorization == null) return;

            var configuredScheme = GetConfiguredScheme();

            if (authorization.Scheme != configuredScheme)
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid Scheme", context.Request);
                return;
            }

            switch (authorization.Scheme)
            {
                case BasicScheme:
                    await AuthenticateBasicAsync(context, cancellationToken);
                    break;
                case HmacScheme:
                    await AuthenticateHmacAsync(context, cancellationToken);
                    return;
            }

            return;
        }

        #region Basic Auth 
        private async Task AuthenticateBasicAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;
            var authorization = request.Headers.Authorization;

            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                return;
            }

            var (username, password) = ExtractUserNameAndPassword(authorization.Parameter);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
                return;
            }

            var user = ValidateUmbracoUser(username, password);

            if (user != null)
            {

                var umbracoIdentity = new UmbracoBackOfficeIdentity(
                    user.Id,
                    user.Username,
                    user.Name,
                    user.StartContentIds,
                    user.StartMediaIds,
                    "en-us",
                    Guid.NewGuid().ToString(),
                    user.SecurityStamp,
                    user.AllowedSections,
                    user.Groups.Select(x => x.Alias)
                    );

                context.Principal = new ClaimsPrincipal(umbracoIdentity);
            }

        }

        private (string username, string password) ExtractUserNameAndPassword(string authorizationParameter)
        {
            byte[] credentialBytes;

            try
            {
                credentialBytes = Convert.FromBase64String(authorizationParameter);
            }
            catch (FormatException)
            {
                return (null, null);
            }

            // The currently approved HTTP 1.1 specification says characters here are ISO-8859-1.
            // However, the current draft updated specification for HTTP 1.1 indicates this encoding is infrequently
            // used in practice and defines behavior only for ASCII.
            Encoding encoding = Encoding.ASCII;
            // Make a writable copy of the encoding to enable setting a decoder fallback.
            encoding = (Encoding)encoding.Clone();
            // Fail on invalid bytes rather than silently replacing and continuing.
            encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
            string decodedCredentials;

            try
            {
                decodedCredentials = encoding.GetString(credentialBytes);
            }
            catch (DecoderFallbackException)
            {
                return (null, null);
            }

            if (String.IsNullOrEmpty(decodedCredentials))
            {
                return (null, null);
            }

            int colonIndex = decodedCredentials.IndexOf(':');

            if (colonIndex == -1)
            {
                return (null, null);
            }

            string userName = decodedCredentials.Substring(0, colonIndex);
            string password = decodedCredentials.Substring(colonIndex + 1);
            return (userName, password);
        }

        private IUser ValidateUmbracoUser(string username, string password)
        {
            try
            {
                // this is the Models builder way. 
                // but i am not sure it increments failed attempts,
                // so it would be suseptible to brute force ? 

                var provider = Membership.Providers[Constants.Security.UserMembershipProviderName];
                if (provider == null || !provider.ValidateUser(username, password))
                    return null;


                var user = Current.Services.UserService.GetByUsername(username);
                if (!user.IsApproved || user.IsLockedOut)
                    return null;

                return user;
            }
            catch
            {
                return null;
            }
        }
        #endregion


        private async Task AuthenticateHmacAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {

            var hmacKey = ConfigurationManager.AppSettings["uSync.TriggerHmacKey"];
            if (string.IsNullOrWhiteSpace(hmacKey)) 
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", context.Request);
                return;
            }

            // hmac is on. 

            var request = context.Request;
            var authorization = request.Headers.Authorization;

            var hmacParameters = GetHmacAuthHeader(authorization.Parameter);
            if (hmacParameters == null)
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
                return;
            }

            if (MemoryCache.Default.Contains(hmacParameters.Nonce))
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
                return;
            }

            var timestampTime = DateTimeOffset.FromUnixTimeSeconds(hmacParameters.Timestamp);
            if ( (DateTime.UtcNow - timestampTime).TotalSeconds > 180)
            {
                context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
                return;
            }


            if (CheckSignature(hmacParameters, hmacKey, request))
            {

                var user = Current.Services.UserService.GetUserById(-1);

                var umbracoIdentity = new UmbracoBackOfficeIdentity(
                                 user.Id,
                                 user.Username,
                                 user.Name,
                                 user.StartContentIds,
                                 user.StartMediaIds,
                                 "en-us",
                                 Guid.NewGuid().ToString(),
                                 user.SecurityStamp,
                                 user.AllowedSections,
                                 user.Groups.Select(x => x.Alias)
                                 );

                context.Principal = new ClaimsPrincipal(umbracoIdentity);
            }
        }

        private bool CheckSignature(HmacParamaters paramaters, string key, HttpRequestMessage request)
        {
            string token = $"{request.Method}" +
                $"{paramaters.Timestamp}" +
                $"{paramaters.Nonce}" +
                $"{request.Content.Headers.ContentLength ?? 0}";

            var secretBytes = Convert.FromBase64String(key);
            var tokenBytes = Encoding.UTF8.GetBytes(token);

            using(HMACSHA256 hmac = new HMACSHA256(secretBytes))
            {
                var hashed = hmac.ComputeHash(tokenBytes);
                string stringToken = Convert.ToBase64String(hashed);

                var match = paramaters.Signature.Equals(stringToken, StringComparison.Ordinal);

                if (match)
                {
                    MemoryCache.Default.Add(paramaters.Nonce, paramaters.Timestamp, DateTimeOffset.Now.AddMinutes(180));
                }

                return match;
            }
        }


        private HmacParamaters GetHmacAuthHeader(string authHeader)
        {
            var array = authHeader.Split(':');
            if (array.Length == 3)
            {
                return new HmacParamaters()
                {
                    Signature = array[0],
                    Nonce = array[1],
                    Timestamp = Convert.ToInt64(array[2])
                };
            }

            return null;
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            var authScheme = GetConfiguredScheme();
            var challenge = new AuthenticationHeaderValue(authScheme);
            context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
            return Task.FromResult(0);
        }


        private string GetConfiguredScheme()
        {
            var scheme = ConfigurationManager.AppSettings["uSync.TriggerScheme"];

            if (!string.IsNullOrWhiteSpace(scheme) && scheme.InvariantEquals("hmac"))
            {
                return HmacScheme;
            }

            return BasicScheme;
        }
    }
}
