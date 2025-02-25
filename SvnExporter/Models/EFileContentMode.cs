namespace SvnExporter.Lib.Models;

public enum EFileContentMode
{
    None,       // Don't retrieve file content
    Preview,    // Retrieve a preview (first N bytes/chars)
    Full        // Retrieve the entire file content
}