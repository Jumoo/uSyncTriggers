using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

using uSync.BackOffice;
using uSync.Triggers.Auth;
using uSync.Triggers.Config;

namespace uSync.Triggers
{
    [ComposeAfter(typeof(uSyncBackOfficeComposer))]

    public class uSyncTriggersComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.AdduSyncTriggers();
        }
    }

    public static class uSyncTriggerBuilderExtensions
    {
        public static IUmbracoBuilder AdduSyncTriggers(this IUmbracoBuilder builder)
        {
            builder.AdduSync();

            builder.Services.AddOptions<uSyncTriggerConfig>()
                .Bind(builder.Config.GetSection("uSync:Triggers"));

            builder.Services.AddAuthentication(o =>
                o.AddScheme(uSyncTriggers.AuthScheme, 
                    a => a.HandlerType = typeof(uSyncTriggerAuthenticationHandler)));

            return builder;
        }

    }
}
