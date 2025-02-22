using SharpSvn;

namespace SvnExporter.Models;

public class SvnChangeInfo
{
    public SvnChangeAction Action { get; set; }
    public string Path { get; set; }
    public string RepositoryPath { get; set; }
    public SvnNodeKind NodeKind { get; set; }
    public bool ContentModified { get; set; }
    public bool PropertiesModified { get; set; }
    public string? CopyFromPath { get; set; }
    public long? CopyFromRevision { get; set; }
    public FileInfoDetail? FileInfo { get; set; } // Null if not a file or deleted
}