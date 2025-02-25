using SvnExporter.Lib.Models;

namespace SvnExporter.Lib;

public interface ISvnItemsExporter
{
    void Export(IEnumerable<SvnRevision> logEntries);
}