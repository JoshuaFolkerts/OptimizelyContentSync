using EPiServer.Core.Transfer;
using EPiServer.Enterprise;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimizely.ImportExport
{
    internal sealed class ExportState
    {
        public string FileLocation { get; set; }

        public IDataExporter Exporter { get; set; }

        public ExportOptions Options { get; set; }

        public Stream Stream { get; set; }

        public IList<ExportSource> SourceRoots { get; set; }

        public Dictionary<string, object> Settings { get; set; }

        public Guid Parent { get; set; }
    }
}