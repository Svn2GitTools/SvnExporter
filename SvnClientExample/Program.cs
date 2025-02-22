using SvnExporter;
using SvnExporter.Models;

namespace SvnClientExample;

class Program
{
    static void Main(string[] args)
    {
        string? svnRepoUrl = null;
        if (args.Length > 0)
        {
            svnRepoUrl = args[0];
        }

        if (string.IsNullOrEmpty(svnRepoUrl))
        {
            Console.WriteLine("Please provide a valid SVN repository URL as an argument.");
            return;
        }
        ISvnItemsReader itemsReader = new SvnItemsReader();
        ISvnItemsExporter svnItemsExporter = new ConsoleSvnItemsExporter();

        try
        {
            // 1. Console Display Options (Preview Content)
            LogRetrievalOptions consoleOptions = new LogRetrievalOptions()
                                                     {
                                                         FileContentMode = EFileContentMode.Preview,
                                                         FileContentPreviewLength = 100,
                                                         IncludeChangedPaths = true,
                                                         IncludeRevisionProperties = true
                                                     };
            Console.WriteLine("--- Console Display Output (Preview Content) ---");
            IEnumerable<SvnRevision> logEntriesForConsole = itemsReader.GetLogEntries(svnRepoUrl, consoleOptions);
            svnItemsExporter.Export(logEntriesForConsole);


            // 2. Git Export Options (Full Content)
            LogRetrievalOptions gitExportOptions = new LogRetrievalOptions()
                                                       {
                                                           FileContentMode = EFileContentMode.Full,
                                                           IncludeChangedPaths = true,
                                                           IncludeRevisionProperties = true
                                                       };
            // (You might have a different exporter for Git commands or Git format)
            // Console.WriteLine("\n--- Git Export Output (Full Content - Example Console Output) ---"); // Example - replace with GitExporter
            // IEnumerable<LogEntry> logEntriesForGit = logRetriever.GetLogEntries(svnRepoUrl, gitExportOptions);
            // logExporter.Export(logEntriesForGit); // Example - use GitExporter instead


            // 3. Author List Options (No File Content, Minimal Data)
            LogRetrievalOptions authorListOptions = new LogRetrievalOptions()
                                                        {
                                                            FileContentMode = EFileContentMode.None,
                                                            IncludeChangedPaths = false, // We might not need changed paths for just authors
                                                            IncludeRevisionProperties = false // Probably don't need revision properties for author list
                                                        };
            Console.WriteLine("\n--- Author List Output (Minimal Data) ---");
            AuthorsListExporter authorsListExporter = new AuthorsListExporter(); // Example AuthorListExporter
            IEnumerable<SvnRevision> logEntriesForAuthors = itemsReader.GetLogEntries(svnRepoUrl, authorListOptions);
            authorsListExporter.Export(logEntriesForAuthors);


        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}