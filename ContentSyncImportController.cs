using EPiServer.Enterprise;
using EPiServer.ServiceApi.Configuration;
using EPiServer.ServiceApi.Extensions;
using EPiServer.ServiceApi.Util;
using EPiServer.ServiceLocation;
using EPiServer.Web.Internal;
using EPiServer.Security;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace Optimizely.ImportExport
{
    public class ContentSyncImportController : ApiController
    {
        private static readonly ApiCallLogger _logger = new ApiCallLogger(typeof(ContentSyncImportController));

        private readonly PermanentLinkMapper _permanentLinkMapper;

        private readonly ServiceAccessor<IDataImporter> _dataImporterAccessor;

        private const string InvalidMediaTypeMessage = "Wrong Media Type";

        public ContentSyncImportController(PermanentLinkMapper permanentLinkMapper, ServiceAccessor<IDataImporter> dataImporterAccessor)
        {
            _permanentLinkMapper = permanentLinkMapper;
            _dataImporterAccessor = dataImporterAccessor;
        }

        [Route("episerverapi/import/cms/content/{id:guid}", Name = "opti_UpdateContent")]
        [HttpPost]
        [ResponseType(typeof(Guid))]
        [AuthorizePermission(Permissions.GroupName, Permissions.Write)]
        public virtual async Task<IHttpActionResult> PostCmsImport(Guid id)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                _logger.Error(InvalidMediaTypeMessage, Request.CreateResponseException(InvalidMediaTypeMessage, HttpStatusCode.UnsupportedMediaType));
                throw Request.CreateResponseException(InvalidMediaTypeMessage, HttpStatusCode.UnsupportedMediaType);
            }

            var file = await Request.GetUploadedFile(UploadPaths.IntegrationDataPath);
            var destinationRoot = _permanentLinkMapper.Find(id);
            if (destinationRoot == null)
            {
                return Ok(id);
            }

            var importerOptions = ImportOptions.DefaultOptions;
            importerOptions.KeepIdentity = true;
            importerOptions.ValidateDestination = true;
            importerOptions.EnsureContentNameUniqueness = true;
            importerOptions.IsTest = false;
            var importer = _dataImporterAccessor();
            PrincipalInfo.RecreatePrincipalForThreading();
            var state = new ImporterState
            {
                Destination = destinationRoot.ContentReference,
                Importer = importer,
                Options = importerOptions,
                Stream = new FileStream(file.LocalFileName, FileMode.Open)
            };

            var message = await ImportFileThread(state);
            if (string.IsNullOrEmpty(message))
            {
                return Ok(id);
            }
            else
            {
                throw Request.CreateResponseException(message, HttpStatusCode.InternalServerError);
            }
        }

        private Task<string> ImportFileThread(ImporterState state)
        {
            return Task.Run(() =>
            {
                try
                {
                    state.Importer.Import(state.Stream, state.Destination, state.Options);
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.Error("Can't import data because, ", ex);
                    return ex.StackTrace;
                }
            });
        }
    }
}