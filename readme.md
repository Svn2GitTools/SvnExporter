# SvnExporter
[![NuGet Package](https://img.shields.io/nuget/v/SvnImporter.Lib.svg?style=flat-square)](https://www.nuget.org/packages/SvnImporter.Lib)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)  <!-- Replace with your license badge if different -->
[![GitHub Organization](https://img.shields.io/badge/Organization-Svn2GitTools-blueviolet)](https://github.com/Svn2GitTools) <!-- Replace with your org link if different -->

**SvnExporter** is a .NET library and tool designed to extract and export data from Subversion (SVN) repositories. It provides a flexible way to retrieve SVN log entries, including commit details, changed paths, file content (optional), and revision properties.  This tool is useful for analyzing SVN history, generating reports, or as a building block for more complex SVN to Git migration processes.

## Key Features

* **Flexible Log Retrieval:**
    * Retrieves SVN log entries in batches for efficient processing of large repositories.
    * Allows specifying start and end revisions for targeted data extraction.
    * Tracks progress and estimates remaining time during log retrieval.
* **Detailed Revision Information:**
    * Extracts author, commit date, and commit message for each revision.
    * Optionally includes lists of changed paths within each revision.
    * Optionally retrieves revision properties (beyond standard SVN metadata).
* **File Content Handling (Optional):**
    * Provides options to include file content in the exported data:
        * **None:**  No file content is retrieved (fastest, minimal data).
        * **Preview:** Retrieves a preview (first N characters/bytes) of file content.
        * **Full:** Retrieves the entire file content.
    * Detects binary files based on `svn:mime-type` property.
    * Handles both text and binary file content appropriately.
* **Extensible Exporting:**
    * Provides an `ISvnItemsExporter` interface for defining custom export formats.
    * Includes example exporters:
        * **Console Exporter:**  Displays SVN data in the console (with optional content preview).
        * **Authors List Exporter:** Generates a list of authors from the SVN log.
     >Note: Not included into this project, see SvnRepo2Git project.
     * **Svn2Git Exporter (Integration):** Designed to be used as a component in SVN to Git conversion tool.
* **Options for Data Inclusion:**
    * Fine-grained control over what data to retrieve using `LogRetrievalOptions`:
        * `FileContentMode`: Control the level of file content retrieval (None, Preview, Full).
        * `IncludeChangedPaths`: Include lists of changed files/directories in each revision.
        * `IncludeRevisionProperties`: Include custom revision properties.
* **Uses SharpSvn:**  Built on the robust and well-regarded [SharpSvn](https://github.com/AmpScm/SharpSvn) library for interacting with SVN repositories.

## Getting Started


### Prerequisites

* **.NET Runtime:**  Ensure you have a compatible .NET runtime installed (e.g., .NET 9 or later).
* **SharpSvn:** SvnExporter relies on the SharpSvn library.  This is typically managed through NuGet package manager when building the project.
* **SVN Client (Optional):** While SvnExporter uses SharpSvn, having a standard SVN client installed might be helpful for troubleshooting or interacting with SVN repositories outside of this tool.
### Installation  

You can install `SvnExporter` using NuGet:  

```bash
dotnet add package SvnExporter.Lib
```  

> **Note:** The package ID has been changed to SvnExporter.Lib to avoid conflicts with an existing package.

### Building the Project

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/Svn2GitTools/SvnExporter.git  # Replace with your actual repository URL
   cd SvnExporter
   ```

2. **Build with .NET CLI:**
   ```bash
   dotnet build
   ```
   or open the project in Visual Studio or your preferred .NET IDE and build.

### Usage

SvnExporter is primarily a library, but it can be used with example console applications (like those shown in your code snippets) or integrated into other tools.

**Example Usage:**

```csharp
// ... (Project setup and dependencies) ...

ISvnItemsReader itemsReader = new SvnItemsReader();

// Example 1: Console Output with Preview Content
LogRetrievalOptions consoleOptions = new LogRetrievalOptions()
{
    FileContentMode = EFileContentMode.Preview,
    FileContentPreviewLength = 100,
    IncludeChangedPaths = true,
    IncludeRevisionProperties = true
};
Console.WriteLine("--- Console Display Output (Preview Content) ---");
IEnumerable<SvnRevision> logEntriesForConsole = itemsReader.GetLogEntries("your_svn_repo_url", consoleOptions); // Replace with your SVN URL
ISvnItemsExporter consoleExporter = new ConsoleSvnItemsExporter();
consoleExporter.Export(logEntriesForConsole);


// Example 2: Generating Author List
LogRetrievalOptions authorListOptions = new LogRetrievalOptions()
{
    FileContentMode = EFileContentMode.None,
    IncludeChangedPaths = false,
    IncludeRevisionProperties = false
};
Console.WriteLine("\n--- Author List Output ---");
AuthorsListExporter authorsListExporter = new AuthorsListExporter();
IEnumerable<SvnRevision> logEntriesForAuthors = itemsReader.GetLogEntries("your_svn_repo_url", authorListOptions); // Replace with your SVN URL
authorsListExporter.Export(logEntriesForAuthors);
authorsListExporter.WriteToFile("authors.txt", "yourdomain.com"); // Replace with your domain if needed


// Example 3: (Integration with Svn2GitExporter - for Git migration tools)
// ... (Setup AuthorsListImporter if needed) ...
ISvnItemsExporter gitExporter = new Svn2GitExporter("path_to_your_git_repo", /* authorsListImporter if needed */ null); // Replace with your Git repo path
LogRetrievalOptions gitExportOptions = new LogRetrievalOptions()
{
    FileContentMode = EFileContentMode.Full,
    IncludeChangedPaths = true,
    IncludeRevisionProperties = true
};
Console.WriteLine("\n--- Git Export Output (Full Content - for Git migration) ---");
IEnumerable<SvnRevision> logEntriesForGit = itemsReader.GetLogEntries("your_svn_repo_url", gitExportOptions); // Replace with your SVN URL
gitExporter.Export(logEntriesForGit);
```

**Important:**

* **Replace Placeholders:**  Remember to replace `"your_svn_repo_url"`, `"path_to_your_git_repo"`, and `"yourdomain.com"` with your actual SVN repository URL, Git repository path (if applicable), and email domain if using the author list exporter.
* **Command-line Interface (CLI):**  To make this a truly usable tool, you would typically create a command-line interface (CLI) application that takes parameters like SVN URL, output format, options, etc.  This README focuses on the library aspects.

## Code Structure

* **`ISvnItemsReader.cs` / `SvnItemsReader.cs`:**  Defines the interface and implementation for reading SVN log entries using SharpSvn.  Handles batch processing, progress tracking, and conversion of `SvnLogEventArgs` to `SvnRevision` models.
* **`ISvnItemsExporter.cs`:**  Defines the interface for exporting `SvnRevision` data.
* **`ConsoleSvnItemsExporter.cs` (Example):**  Implements `ISvnItemsExporter` to display `SvnRevision` data in the console.
* **`AuthorsListExporter.cs` (Example):** Implements `ISvnItemsExporter` to generate a list of authors from `SvnRevision` data.
* **`Models/` Directory:** Contains data models:
    * `SvnRevision.cs`: Represents an SVN revision with author, date, commit message, changed paths, and properties.
    * `SvnChangeInfo.cs`: Represents information about a changed path within a revision.
    * `FileInfoDetail.cs`: Represents details about a file (size, type, content).
* **`LogRetrievalOptions.cs`:** Defines options to control what data is retrieved from SVN (content mode, changed paths, properties).

## Dependencies

* **SharpSvn:**  [https://sharpsvn.open.collab.net/](https://sharpsvn.open.collab.net/) (SVN client library for .NET)

## Contributing

Contributions are welcome!  Please feel free to submit issues, bug reports, feature requests, or pull requests to improve `SvnExporter`.

## License

This project is licensed under the [MIT License](LICENSE) - see the [LICENSE](LICENSE) file for details.  <!-- Replace with your actual license and LICENSE file -->

---

**[← Back to Svn to Git Tools Collection](https://github.com/Svn2GitTools)**  <!-- Replace with your organization link -->
```
