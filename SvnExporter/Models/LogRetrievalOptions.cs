namespace SvnExporter.Models;

public class LogRetrievalOptions
{
    public bool IncludeChangedPaths { get; set; } = true; // Default to include changed paths
    public bool IncludeRevisionProperties { get; set; } = true; // Default to include revision properties
    public EFileContentMode FileContentMode { get; set; } = EFileContentMode.Preview; // Default to preview
    public int FileContentPreviewLength { get; set; } = 100; // Default preview length
}