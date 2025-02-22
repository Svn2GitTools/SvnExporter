using System.Collections.ObjectModel;

namespace SvnExporter.Models;

public class SvnRevision
{
    public string Author { get; set; }

    public Collection<SvnChangeInfo> ChangeInfo { get; set; }

    public DateTime Date { get; set; }

    public string CommitMessage { get; set; }

    public Dictionary<string, string> Properties { get; set; } // Revision properties

    public long Revision { get; set; }
}