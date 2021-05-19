using EPiServer.Core;
using EPiServer.Core.Transfer;
using EPiServer.Enterprise;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer;

namespace Optimizely.ImportExport
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class ContentSyncInitialization : IInitializableModule
    {
        private ServiceAccessor<IDataExporter> _dataExporterAccessor;

        private IContentLoader _contentLoader;

        private IContentSyncOptions _contentSyncOptions;

        private ContentSyncService _contentSyncService;

        public void Initialize(InitializationEngine context)
        {
            var services = context.Locate.Advanced;

            _contentSyncOptions = services.GetInstance<IContentSyncOptions>();
            _contentSyncService = services.GetInstance<ContentSyncService>();
            _dataExporterAccessor = services.GetInstance<ServiceAccessor<IDataExporter>>();
            _contentLoader = services.GetInstance<IContentLoader>();

            if (!_contentSyncOptions.ContentStagingEnabled)
                return;

            var events = services.GetInstance<IContentEvents>();
            events.PublishedContent += Events_PublishedContent;
        }

        public void Uninitialize(InitializationEngine context)
        {
            var events = context.Locate.Advanced.GetInstance<IContentEvents>();
            events.PublishedContent -= Events_PublishedContent;
        }

        private void Events_PublishedContent(object sender, ContentEventArgs e)
        {
            if (e.Content is PageData || e.Content is BlockData || e.Content is MediaData)
                Task.Run(() => this._contentSyncService.ExportItem(e.Content, _dataExporterAccessor(), _contentLoader));
        }
    }
}