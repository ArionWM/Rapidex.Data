using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex;
public class ContentBase
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ContentType { get; set; }

    public string? SourceNavigation { get; set; }
    public string? SourceFileName { get; set; }
    public string? SourceFilePath { get; set; }
    public string? SourceWebUrl { get; set; }
    public string? SourceFileContentType { get; set; }

    public int TokenCount { get; set; } = 0;

    public string Language { get; set; }

}

public class StreamContent : ContentBase, IDisposable
{
    public Stream Stream { get; set; }

    public StreamContent()
    {

    }

    public StreamContent(string name, string contentType, Stream stream)
    {
        Stream = stream;
        Name = name;
        ContentType = contentType;
    }

    public void Dispose()
    {
        if (Stream != null && Stream.CanRead)
        {
            Stream.Dispose();
            Stream = null;
        }
    }
}

public class TextContent : ContentBase
{
    public string Content { get; set; }

    public TextContent()
    {

    }

    public TextContent(string name, string contentType, string content)
    {
        Content = content;
        Name = name;
        ContentType = contentType;
    }
}

public class BinaryContent : ContentBase
{
    public byte[] Content { get; set; }

    public BinaryContent()
    {

    }

    public BinaryContent(string name, string contentType, byte[] content)
    {
        Content = content;
        Name = name;
        ContentType = contentType;
    }
}
