using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Hybrid;

namespace Rapidex.Data.Cache;

internal class HybridCacheSerializer<T> : IHybridCacheSerializer<T>
{
    private readonly CompressionLevel compressionLevel;
    private readonly int compressionThreshold;

    public T Deserialize(ReadOnlySequence<byte> source)
    {
        var bytes = source.ToArray();

        if (bytes.Length == 0)
            throw new InvalidOperationException("Empty data cannot be deserialized");

        // İlk byte sıkıştırma flag'i (1 = compressed, 0 = uncompressed)
        bool isCompressed = bytes[0] == 1;
        var data = bytes.AsSpan(1); // Flag'i atla

        if (isCompressed)
        {
            using var compressedStream = new MemoryStream(data.ToArray());
            using var brotliStream = new BrotliStream(compressedStream, CompressionMode.Decompress);
            using var decompressedStream = new MemoryStream();

            brotliStream.CopyTo(decompressedStream);
            decompressedStream.Position = 0;

            return JsonSerializer.Deserialize<T>(decompressedStream.ToArray(), JsonHelper.DefaultJsonSerializerOptions)!;
        }
        else
        {
            return JsonSerializer.Deserialize<T>(data, JsonHelper.DefaultJsonSerializerOptions)!;
        }
    }


    public void Serialize(T value, IBufferWriter<byte> target)
    {
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonHelper.DefaultJsonSerializerOptions);

        // Sıkıştırma gerekli mi kontrol et
        bool shouldCompress = jsonBytes.Length >= compressionThreshold;

        if (shouldCompress)
        {
            // Compressed flag (1)
            target.Write(new byte[] { 1 });

            // Brotli ile sıkıştır
            using var compressedStream = new MemoryStream();
            using (var brotliStream = new BrotliStream(compressedStream, compressionLevel))
            {
                brotliStream.Write(jsonBytes);
            }

            target.Write(compressedStream.ToArray());
        }
        else
        {
            // Uncompressed flag (0)
            target.Write(new byte[] { 0 });
            target.Write(jsonBytes);
        }

    }
}
