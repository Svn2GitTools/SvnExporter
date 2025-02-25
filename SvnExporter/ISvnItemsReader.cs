using SvnExporter.Lib.Models;

namespace SvnExporter.Lib;

public interface ISvnItemsReader
{
    /// <summary>
    /// Gets the log entries.
    /// </summary>
    /// <param name="svnRepoUrl">The SVN repo URL.</param>
    /// <param name="options">The options.</param>
    /// <param name="startRevision">The start revision.</param>
    /// <param name="endRevision">The end revision.</param>
    /// <param name="batchSize">Size of the batch.</param>
    /// <returns>IEnumerable&lt;LogEntry&gt;.</returns>
    IEnumerable<SvnRevision> GetLogEntries(
        string svnRepoUrl,
        LogRetrievalOptions options,
        long? startRevision = null,
        SharpSvn.SvnRevision? endRevision = null,
        int batchSize = 1000);
}