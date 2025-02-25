using System.Collections.ObjectModel;
using System.Diagnostics;

using SharpSvn;

using SvnExporter.Lib.Models;

using SvnRevision = SvnExporter.Lib.Models.SvnRevision;

namespace SvnExporter.Lib;

public class SvnItemsReader : ISvnItemsReader
{
    public IEnumerable<SvnRevision> GetLogEntries(
    string svnRepoUrl,
    LogRetrievalOptions options,
    long? startRevision = null,
    SharpSvn.SvnRevision? endRevision = null,
    int batchSize = 1000)
    {
        using (SvnClient client = new SvnClient())
        {
            var fileUri = new Uri(svnRepoUrl);
            Collection<Uri> targetUris = new Collection<Uri> { fileUri };

            // Get the effective end revision
            var totalRevisions = GetTotalRevisions(fileUri);
            var effectiveEndRevision = endRevision?.Revision ?? totalRevisions;

            Console.WriteLine($"Total revisions: {totalRevisions}");
            Console.WriteLine($"Processing revisions from {startRevision ?? 1} to {effectiveEndRevision}");

            int processedCount = 0;
            long currentStartRevision = startRevision ?? 1;
            Stopwatch totalStopwatch = Stopwatch.StartNew();
            Stopwatch batchStopwatch = new Stopwatch();
            Queue<double> recentSpeeds = new Queue<double>();
            const int maxSamples = 5; // Track last 5 batches for smoothing

            while (currentStartRevision <= effectiveEndRevision)
            {
                long batchEnd = Math.Min(
                    Math.Min(currentStartRevision + batchSize - 1, totalRevisions),
                    effectiveEndRevision
                );

                batchStopwatch.Restart();
                SvnLogArgs logArgs = new SvnLogArgs
                {
                    Start = currentStartRevision,
                    End = batchEnd,
                    RetrieveAllProperties = false
                };

                Collection<SvnLogEventArgs> logEntriesRaw = new Collection<SvnLogEventArgs>();
                bool success = client.GetLog(targetUris, logArgs, out logEntriesRaw);

                if (success && logEntriesRaw != null)
                {
                    foreach (var logEventArgs in logEntriesRaw)
                    {
                        yield return ConvertToLogEntry(client, svnRepoUrl, logEventArgs, options);
                        processedCount++;
                    }

                    batchStopwatch.Stop();
                    // Compute recent speed (revisions per second for the last batch)
                    double batchSeconds = batchStopwatch.Elapsed.TotalSeconds;
                    double batchSpeed = batchSeconds > 0 ? batchSize / batchSeconds : 0;

                    // Maintain a moving average of processing speed
                    if (batchSpeed > 0)
                    {
                        if (recentSpeeds.Count >= maxSamples)
                        {
                            recentSpeeds.Dequeue();
                        }
                        recentSpeeds.Enqueue(batchSpeed);
                    }

                    double averageSpeed = recentSpeeds.Count > 0 ? recentSpeeds.Average() : batchSpeed;
                    double remainingRevisions = effectiveEndRevision - processedCount;
                    double estimatedRemainingSeconds = averageSpeed > 0 ? remainingRevisions / averageSpeed : 0;
                    TimeSpan estimatedRemainingTime = TimeSpan.FromSeconds(estimatedRemainingSeconds);

                    Console.Write(
                        $"\rProcessed {processedCount}/{(effectiveEndRevision - (startRevision ?? 1) + 1)}, " +
                        $"{(double)processedCount * 100 / (effectiveEndRevision - (startRevision ?? 1) + 1):F}% revisions. " +
                        $"Elapsed: {totalStopwatch.Elapsed:hh\\:mm\\:ss}, " +
                        $"Remaining: {estimatedRemainingTime:hh\\:mm\\:ss}");
                }
                else
                {
                    Console.WriteLine(
                        $"Error: Could not retrieve logs for revisions {currentStartRevision}-{batchEnd}. Skipping...");
                }

                // Move to the next batch
                currentStartRevision = batchEnd + 1;
            }

            totalStopwatch.Stop();
            Console.WriteLine(
                $"\nFinished processing {processedCount} revisions in {totalStopwatch.Elapsed:hh\\:mm\\:ss}.");
        }
    }

    /// <summary>
    /// Gets the total number of revisions in the specified SVN repository.
    /// </summary>
    /// <param name="repositoryUri">The URI of the SVN repository.</param>
    /// <returns>The total number of revisions, or -1 if an error occurs.</returns>
    public static long GetTotalRevisions(Uri repositoryUri)
    {
        try
        {
            using (SvnClient client = new SvnClient())
            {
                // Define the arguments for retrieving the log
                SvnLogArgs logArgs = new SvnLogArgs
                                         {
                                             RetrieveAllProperties =
                                                 false, // Only retrieve essential properties
                                             Limit = 1 // Limit the number of log entries to 1
                                         };

                // Retrieve the latest log entry
                if (client.GetLog(repositoryUri, logArgs, out Collection<SvnLogEventArgs> logEntries))
                {
                    if (logEntries.Count > 0)
                    {
                        // The latest revision number is the total number of revisions
                        return logEntries[0].Revision;
                    }

                    Console.WriteLine("No revisions found.");
                    return 0;
                }

                Console.WriteLine("Failed to retrieve log information.");
                return -1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return -1;
        }
    }

    private SvnRevision ConvertToLogEntry(
        SvnClient client,
        string svnRepoUrl,
        SvnLogEventArgs entry,
        LogRetrievalOptions options)
    {
        var logEntry = new SvnRevision
                           {
                               Revision = entry.Revision,
                               Author = entry.Author,
                               Date = entry.Time,
                               CommitMessage = entry.LogMessage,
                               ChangeInfo =
                                   options.IncludeChangedPaths
                                       ? new Collection<SvnChangeInfo>()
                                       : null, // Conditional initialization
                               Properties =
                                   options.IncludeRevisionProperties
                                       ? new Dictionary<string, string>()
                                       : null // Conditional initialization
                           };

        if (options.IncludeChangedPaths)
        {
            foreach (var changedPath in entry.ChangedPaths)
            {
                logEntry.ChangeInfo.Add(
                    GetChangedPathInfo(
                        client,
                        svnRepoUrl,
                        entry,
                        changedPath,
                        options)); // Pass options
            }
        }

        if (options.IncludeRevisionProperties)
        {
            SvnPropertyCollection properties;
            if (client.GetRevisionPropertyList(new Uri(svnRepoUrl), entry.Revision, out properties))
            {
                var excludeProperties = new HashSet<string> { "svn:log", "svn:author", "svn:date" };
                foreach (var property in properties)
                {
                    if (!excludeProperties.Contains(property.Key))
                    {
                        logEntry.Properties.Add(property.Key, property.StringValue);
                    }
                }
            }
        }

        return logEntry;
    }

    private SvnChangeInfo GetChangedPathInfo(
        SvnClient client,
        string svnRepoUrl,
        SvnLogEventArgs entry,
        SvnChangeItem changedPath,
        LogRetrievalOptions options)
    {
        var changedPathInfo = new SvnChangeInfo
                                  {
                                      Action = changedPath.Action,
                                      Path = changedPath.Path,
                                      RepositoryPath = changedPath.RepositoryPath.ToString(),
                                      NodeKind = changedPath.NodeKind,
                                      ContentModified = changedPath.ContentModified ?? false,
                                      PropertiesModified = changedPath.PropertiesModified ?? false,
                                      CopyFromPath = changedPath.CopyFromPath,
                                      CopyFromRevision = changedPath.CopyFromRevision,
                                      FileInfo = null // Will be populated conditionally
                                  };

        if (changedPath.Action != SvnChangeAction.Delete
            && changedPath.NodeKind == SvnNodeKind.File)
        {
            changedPathInfo.FileInfo = GetFileInfo(
                client,
                svnRepoUrl,
                entry,
                changedPath,
                options); // Pass options
        }

        return changedPathInfo;
    }

    private FileInfoDetail GetFileInfo(
        SvnClient client,
        string svnRepoUrl,
        SvnLogEventArgs entry,
        SvnChangeItem changedPath,
        LogRetrievalOptions options)
    {
        if (options.FileContentMode == EFileContentMode.None)
        {
            return null; // Don't retrieve file info or content
        }

        var fileInfoDetail = new FileInfoDetail();
        SvnUriTarget fileTarget = new(
            new Uri(svnRepoUrl.TrimEnd('/') + changedPath.Path),
            entry.Revision);
        SvnInfoEventArgs info;
        if (client.GetInfo(fileTarget, out info))
        {
            fileInfoDetail.FileSize = info.RepositorySize;
            fileInfoDetail.FileType = info.NodeKind;
        }

        SvnPropertyValue propValue;
        bool isBinary = false;
        if (client.GetProperty(fileTarget, "svn:mime-type", out propValue))
        {
            string mimeType = propValue?.StringValue;
            if (!string.IsNullOrEmpty(mimeType) &&
                (mimeType.StartsWith("application")
                 || mimeType.StartsWith("image")))
            {
                isBinary = true;
            }
        }

        fileInfoDetail.IsBinary = isBinary;

        if (options.FileContentMode != EFileContentMode.None) // Retrieve content only if requested
        {
            using (MemoryStream stream = new MemoryStream())
            {
                if (client.Write(fileTarget, stream))
                {
                    stream.Position = 0;
                    if (isBinary)
                    {
                        if (options.FileContentMode == EFileContentMode.Preview)
                        {
                            fileInfoDetail.BinaryContent = new byte[Math.Min(
                                options.FileContentPreviewLength,
                                stream.Length)];
                            stream.Read(
                                fileInfoDetail.BinaryContent,
                                0,
                                fileInfoDetail.BinaryContent.Length);
                        }
                        else if (options.FileContentMode == EFileContentMode.Full)
                        {
                            fileInfoDetail.BinaryContent = stream.ToArray(); // Read all bytes
                        }
                    }
                    else
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string content = reader.ReadToEnd();
                            if (options.FileContentMode == EFileContentMode.Preview)
                            {
                                fileInfoDetail.Content =
                                    content.Length > options.FileContentPreviewLength
                                        ? content.Substring(0, options.FileContentPreviewLength)
                                          + "..."
                                        : content;
                            }
                            else if (options.FileContentMode == EFileContentMode.Full)
                            {
                                fileInfoDetail.Content = content; // Read all text content
                            }
                        }
                    }
                }
            }
        }

        return fileInfoDetail;
    }
}