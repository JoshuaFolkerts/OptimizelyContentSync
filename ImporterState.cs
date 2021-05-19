using EPiServer.Core;
using EPiServer.Enterprise;
using System.IO;

namespace Optimizely.ImportExport
{
    internal sealed class ImporterState
    {
        public ContentReference Destination { get; set; }

        public IDataImporter Importer { get; set; }

        public Stream Stream { get; set; }

        public ImportOptions Options { get; set; }
    }
}