using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;
public static class FileHelper
{
    public static string[] GetFiles(string directory, bool recursive, params string[] extensions)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Directory cannot be null or empty", nameof(directory));

        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Directory not found: {directory}");

        var directoryInfo = new DirectoryInfo(directory);
        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        IEnumerable<FileInfo> files = directoryInfo.EnumerateFiles("*", searchOption);

        // Filter by extensions if provided
        if (extensions != null && extensions.Length > 0)
        {
            // Normalize extensions to ensure they start with a dot
            var normalizedExtensions = extensions
                .Where(ext => !string.IsNullOrWhiteSpace(ext))
                .Select(ext => ext.StartsWith('.') ? ext.ToLowerInvariant() : $".{ext.ToLowerInvariant()}")
                .ToHashSet();

            files = files.Where(file => normalizedExtensions.Contains(file.Extension.ToLowerInvariant()));
        }

        return files.Select(file => file.FullName).ToArray();
    }
}
