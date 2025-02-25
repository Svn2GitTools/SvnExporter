using SvnExporter.Lib.Models;

namespace SvnExporter.Lib;

public class AuthorsListExporter : ISvnItemsExporter
{
    private HashSet<string>? _authors;

    public void Export(IEnumerable<SvnRevision> logEntries)
    {
        _authors = new HashSet<string>();
        foreach (var entry in logEntries)
        {
            _authors.Add(entry.Author);
        }

        Console.WriteLine("Authors:");
        foreach (var author in _authors)
        {
            Console.WriteLine($"  {author}");
        }
    }

    public void WriteToFile(string fileName, string eMailDomain)
    {
        if (_authors == null)
        {
            Console.WriteLine("No authors found.  Export must be called first.");
            return; // Or throw an exception if you prefer
        }

        try
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                foreach (var author in _authors)
                {
                    writer.WriteLine($"{author} {author}@{eMailDomain}");
                }
            }
            Console.WriteLine($"Authors written to {Path.GetFullPath(fileName)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to file: {ex.Message}");
            // Consider logging the exception details for debugging.
        }
    }
}
