using SvnExporter.Lib.Models;

namespace SvnExporter.Lib;

public class ConsoleSvnItemsExporter : ISvnItemsExporter
{
    public void Export(IEnumerable<SvnRevision> logEntries)
    {
        foreach (var entry in logEntries)
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine($"Revision: {entry.Revision}");
            Console.WriteLine($"Author: {entry.Author}");
            Console.WriteLine($"Date: {entry.Date}");
            Console.WriteLine($"Message: {entry.CommitMessage}");
            Console.WriteLine("Changed Paths:");

            if (entry.ChangeInfo != null)
            {
                foreach (var changedPath in entry.ChangeInfo)
                {
                    Console.WriteLine($"  Action: {changedPath.Action}");
                    Console.WriteLine($"  Path: {changedPath.Path}");
                    Console.WriteLine($"  Repository Path: {changedPath.RepositoryPath}");
                    Console.WriteLine($"  Node Kind: {changedPath.NodeKind}");
                    Console.WriteLine($"  Content Modified: {changedPath.ContentModified}");
                    Console.WriteLine($"  Properties Modified: {changedPath.PropertiesModified}");
                    Console.WriteLine($"  Copy From Path: {changedPath.CopyFromPath}");
                    Console.WriteLine($"  Copy From Revision: {changedPath.CopyFromRevision}");

                    if (changedPath.FileInfo != null) // FileInfo is populated only for relevant file changes
                    {
                        Console.WriteLine($"  File Size: {changedPath.FileInfo.FileSize} bytes");
                        Console.WriteLine($"  File Type: {changedPath.FileInfo.FileType}");

                        if (changedPath.FileInfo.IsBinary)
                        {
                            Console.WriteLine(
                                "  Binary file content (first 100 bytes in hex):");
                            Console.WriteLine(
                                "  " + BitConverter.ToString(changedPath.FileInfo.BinaryContent).Replace("-", " "));
                        }
                        else
                        {
                            Console.WriteLine(
                                "  Text file content (first 100 characters):");
                            Console.WriteLine(
                                "  " + changedPath.FileInfo.Content);
                        }
                    }
                    Console.WriteLine();
                }
            }

            if (entry.Properties != null && entry.Properties.Count > 0)
            {
                Console.WriteLine("Properties:");
                foreach (var property in entry.Properties)
                {
                    Console.WriteLine($"  {property.Key}: {property.Value}");
                }
            }

            Console.WriteLine();
        }
    }
}