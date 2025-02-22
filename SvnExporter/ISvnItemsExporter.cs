using SvnExporter.Models;

namespace SvnExporter;

public interface ISvnItemsExporter
{
    void Export(IEnumerable<SvnRevision> logEntries);
}