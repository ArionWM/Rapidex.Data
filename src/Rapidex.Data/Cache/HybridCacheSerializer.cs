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
using Rapidex.Data.SerializationAndMapping.JsonConverters;

namespace Rapidex.Data.Cache;

internal class HybridCacheSerializer<T> : IHybridCacheSerializer<T>
{
    private const byte CurrentVersion = 1;
    
    private readonly CompressionLevel compressionLevel;
    private readonly int compressionThreshold;

    public T Deserialize(ReadOnlySequence<byte> source)
    {
        var bytes = source.ToArray();

        if (bytes.Length == 0)
            throw new InvalidOperationException("Empty data cannot be deserialized");

        // Header'ı oku
        CachePackageHeader header = this.ReadHeader(bytes, out int headerSize);

        // Versiyon kontrolü
        var data = bytes.AsSpan(headerSize);

        return header.Version switch
        {
            1 => this.DeserializeV1(data, header),
            _ => throw new NotSupportedException($"Unsupported cache package version: {header.Version}")
        };
    }

    public void Serialize(T value, IBufferWriter<byte> target)
    {
        EntityJsonConverter.UseNestedEntities = false;

        try
        {
            var json = JsonSerializer.Serialize(value, JsonHelper.DefaultJsonSerializerOptions);
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            // Sıkıştırma gerekli mi kontrol et
            bool shouldCompress = jsonBytes.Length >= this.compressionThreshold;

            // Header oluştur
            var flags = shouldCompress ? CachePackageFlags.Compressed : CachePackageFlags.None;
            var header = new CachePackageHeader(CurrentVersion, flags);

            // Header'ı yaz
            target.Write(header.ToBytes());

            // Veriyi yaz
            if (shouldCompress)
            {
                this.WriteCompressed(jsonBytes, target);
            }
            else
            {
                target.Write(jsonBytes);
            }
        }
        finally
        {
            EntityJsonConverter.UseNestedEntities = null;
        }
    }

    private CachePackageHeader ReadHeader(byte[] bytes, out int headerSize)
    {
        // Yeni format kontrolü: İlk byte sürüm numarası olabilir
        // Eski format: İlk byte 0 veya 1 (compression flag)
        // Yeni format: İlk byte sürüm >= 1, ikinci byte flags
        
        // Backward compatibility için kontrol
        if (bytes.Length >= CachePackageHeader.HeaderSize)
        {
            var potentialVersion = bytes[0];
            var potentialFlags = bytes[1];

            // Eğer ikinci byte makul flag değerleri içeriyorsa yeni format
            if (potentialVersion >= 1 && potentialFlags <= 0x0F) // Flags maksimum 4 bit kullanıyor
            {
                headerSize = CachePackageHeader.HeaderSize;
                return CachePackageHeader.FromBytes(bytes.AsSpan(0, CachePackageHeader.HeaderSize));
            }
        }

        // Eski format (backward compatibility)
        headerSize = 1;
        return CachePackageHeader.FromLegacyFormat(bytes[0]);
    }

    private T DeserializeV1(ReadOnlySpan<byte> data, CachePackageHeader header)
    {
        if (header.IsCompressed)
        {
            return this.DeserializeCompressed(data);
        }
        else
        {
            return JsonSerializer.Deserialize<T>(data, JsonHelper.DefaultJsonSerializerOptions)!;
        }
    }

    private T DeserializeCompressed(ReadOnlySpan<byte> compressedData)
    {
        using var compressedStream = new MemoryStream(compressedData.ToArray());
        using var brotliStream = new BrotliStream(compressedStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();

        brotliStream.CopyTo(decompressedStream);
        decompressedStream.Position = 0;

        var byteData = decompressedStream.ToArray();
        var json = Encoding.UTF8.GetString(byteData);
        return JsonSerializer.Deserialize<T>(json, JsonHelper.DefaultJsonSerializerOptions)!;
    }

    private void WriteCompressed(byte[] data, IBufferWriter<byte> target)
    {
        using var compressedStream = new MemoryStream();
        using (var brotliStream = new BrotliStream(compressedStream, this.compressionLevel))
        {
            brotliStream.Write(data);
        }

        target.Write(compressedStream.ToArray());
    }
}
