using EPiServer;
using EPiServer.Core;
using EPiServer.Core.Transfer;
using EPiServer.Enterprise;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Optimizely.ImportExport
{
    public interface IContentSyncService
    {
        Task ExportItem(IContent content, IDataExporter exporter, IContentLoader contentLoader);
    }

    public class ContentSyncService : IContentSyncService
    {
        private ServiceAccessor<IDataExporter> _dataExporterAccessor;

        private IContentLoader _contentLoader;

        private IContentSyncOptions _contentSyncOptions;

        private ILogger _logger = LogManager.GetLogger(typeof(ContentSyncService));

        public ContentSyncService(IContentLoader contentLoader, ServiceAccessor<IDataExporter> dataExporterAccessor, IContentSyncOptions contentSyncOptions)
        {
            this._contentLoader = contentLoader;
            this._dataExporterAccessor = dataExporterAccessor;
            this._contentSyncOptions = contentSyncOptions;
        }

        public async Task ExportItem(IContent content, IDataExporter exporter, IContentLoader contentLoader)
        {
            var exportedFileLocation = Path.GetTempFileName();
            var stream = new FileStream(exportedFileLocation, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            var settings = new Dictionary<string, object>
            {
                [this._contentSyncOptions.SettingPageLink] = content.ContentLink,
                [this._contentSyncOptions.SettingRecursively] = false,
                [this._contentSyncOptions.SettingPageFiles] = true,
                [this._contentSyncOptions.SettingIncludeContentTypeDependencies] = true
            };

            var sourceRoots = new List<ExportSource>
            {
                new ExportSource(content.ContentLink, ExportSource.NonRecursive)
            };

            var options = ExportOptions.DefaultOptions;
            options.ExcludeFiles = false;
            options.IncludeReferencedContentTypes = true;

            contentLoader.TryGet(content.ParentLink, out IContent parent);

            var state = new ExportState
            {
                Stream = stream,
                Exporter = exporter,
                FileLocation = exportedFileLocation,
                Options = options,
                SourceRoots = sourceRoots,
                Settings = settings,
                Parent = parent?.ContentGuid ?? Guid.Empty
            };

            if (state.Parent == Guid.Empty)
            {
                return;
            }

            try
            {
                exporter.Export(state.Stream, state.SourceRoots, state.Options);
                exporter.Dispose();
                await SendContent(state.FileLocation, state.Parent);
            }
            catch (Exception ex)
            {
                exporter.Abort();
                exporter.Status.Log.Error("Can't export package because: {0}", ex, ex.Message);
            }
        }

        private async Task SendContent(string file, Guid parentId)
        {
            var token = await GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return;
            }
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(this._contentSyncOptions.StagingUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var content = new MultipartFormDataContent();
                var filestream = new FileStream(file, FileMode.Open);
                content.Add(new StreamContent(filestream), "file", "Import.episerverdata");
                var response = await client.PostAsync($"/episerverapi/import/cms/content/{parentId}", content);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.Error(response.Content.ReadAsStringAsync().Result);
                }
            }
        }

        private async Task<string> GetToken()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(this._contentSyncOptions.StagingUrl);
                var fields = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", this._contentSyncOptions.Username },
                    { "password", this._contentSyncOptions.Password }
                };
                var response = await client.PostAsync("/episerverapi/token", new FormUrlEncodedContent(fields));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    var token = JObject.Parse(content).GetValue("access_token");
                    return token.ToString();
                }
            }
            return null;
        }
    }
}