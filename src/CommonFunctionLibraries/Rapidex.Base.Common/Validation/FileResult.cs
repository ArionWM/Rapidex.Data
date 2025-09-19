using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rapidex
{
    //FileSetResult ?

    [Obsolete("", true)]
    public abstract class FileResultBase<T> : Result<T>
    {
        public string Name { get; set; }
        public string ContentType { get; set; }

        public string SourceFileName { get; set; } // For FilePathResult, this is the original file name
        public string SourceFilePath { get; set; } // For FilePathResult, this is the original file path
        public string SourceFileContentType { get; set; } // For FilePathResult, this is the original file content type


        protected FileResultBase()
        {
        }

        protected FileResultBase(bool success, string name, string contentType, T content) : base(success, null, content)
        {
            this.Name = name;
            this.ContentType = contentType;
        }




        protected new static Result<T> Ok(T content)
        {
            throw new NotSupportedException();
        }

        protected new Result<T> Failure(string message)
        {
            throw new NotSupportedException();
        }
    }

    [Obsolete("", true)]
    public class FilePathResult : FileResultBase<string>
    {
        public FilePathResult()
        {
        }

        public FilePathResult(bool success, string name, string contentType, string content) : base(success, name, contentType, content)
        {
        }
    }

    [Obsolete("", true)]
    public class FileTextContentResult : FileResultBase<string>
    {
        public FileTextContentResult()
        {
        }

        public FileTextContentResult(bool success, string name, string contentType, string content) : base(success, name, contentType, content)
        {
        }
    }

    [Obsolete("", true)]
    public class FileBinaryContentResult : FileResultBase<byte[]>
    {
        public FileBinaryContentResult()
        {
        }

        public FileBinaryContentResult(bool success, string name, string contentType, byte[] content) : base(success, name, contentType, content)
        {
        }
    }

    [Obsolete("", true)]
    public class FileStreamContentResult : FileResultBase<System.IO.Stream>, IDisposable
    {
        private bool disposedValue;

        public FileStreamContentResult()
        {
        }

        public FileStreamContentResult(bool success, string name, string contentType, Stream content) : base(success, name, contentType, content)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Content?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static FileStreamContentResult Ok(string name, string contentType, Stream content)
        {
            return new FileStreamContentResult(true, name, contentType, content);
        }

        public static FileStreamContentResult Fail(string description)
        {
            FileStreamContentResult res = new FileStreamContentResult(false, null, null, null);
            res.Description = description;
            return res;
        }

    }
}
