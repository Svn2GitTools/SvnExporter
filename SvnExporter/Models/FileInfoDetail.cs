using SharpSvn;

namespace SvnExporter.Lib.Models;

public class FileInfoDetail
{
    public long FileSize { get; set; }
    public SvnNodeKind FileType { get; set; }
    public bool IsBinary { get; set; }
    public string? Content { get; set; } // For text files
    public byte[]? BinaryContent { get; set; } // For binary files
}