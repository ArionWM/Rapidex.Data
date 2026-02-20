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

public class ByteArrayContent : ContentBase, IDisposable
{
    public byte[] Data { get; set; }

    public ByteArrayContent()
    {

    }

    public ByteArrayContent(string name, string contentType, byte[] data)
    {
        this.Data = data;
        this.Name = name;
        this.ContentType = contentType;
    }

    public void Dispose()
    {
        this.Data = null;
    }
}

public class StreamContent : ContentBase, IDisposable
{
    public Stream Stream { get; set; }

    public StreamContent()
    {

    }

    public StreamContent(string name, string contentType, Stream stream)
    {
        this.Stream = stream;
        this.Name = name;
        this.ContentType = contentType;
    }

    public void Dispose()
    {
        if (this.Stream != null && this.Stream.CanRead)
        {
            this.Stream.Dispose();
            this.Stream = null;
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
        this.Content = content;
        this.Name = name;
        this.ContentType = contentType;
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
        this.Content = content;
        this.Name = name;
        this.ContentType = contentType;
    }
}
