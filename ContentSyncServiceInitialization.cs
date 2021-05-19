using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace Optimizely.ImportExport
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class ContentSyncServiceInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services
                .AddSingleton<IContentSyncOptions, ContentSyncOptions>()
                .AddSingleton<IContentSyncService, ContentSyncService>();
        }

        public void Initialize(InitializationEngine context)
        {
            /* Not neccessary */
        }

        public void Uninitialize(InitializationEngine context)
        {
            /* Not neccessary */
        }
    }
}