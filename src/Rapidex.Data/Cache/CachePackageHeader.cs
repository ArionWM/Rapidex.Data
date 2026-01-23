using System;
using System.Buffers.Binary;

namespace Rapidex.Data.Cache;

/// <summary>
/// Cache paketinin header bilgilerini tutar
/// </summary>
internal readonly struct CachePackageHeader
{
    /// <summary>
    /// Header boyutu (byte cinsinden)
    /// </summary>
    public const int HeaderSize = 4;

    /// <summary>
    /// Paket sürüm numarasý (1 byte)
    /// </summary>
    public byte Version { get; init; }

    /// <summary>
    /// Paket özellikleri flag'leri (1 byte)
    /// </summary>
    public CachePackageFlags Flags { get; init; }

    /// <summary>
    /// Gelecekte kullanýlmak üzere rezerve edilmiþ alanlar (2 byte)
    /// </summary>
    public ushort Reserved { get; init; }

    /// <summary>
    /// Verinin sýkýþtýrýlmýþ olup olmadýðýný belirtir
    /// </summary>
    public bool IsCompressed => Flags.HasFlag(CachePackageFlags.Compressed);

    public CachePackageHeader(byte version, CachePackageFlags flags)
    {
        this.Version = version;
        this.Flags = flags;
        this.Reserved = 0;
    }

    /// <summary>
    /// Header'ý byte array'e dönüþtürür
    /// </summary>
    public byte[] ToBytes()
    {
        var bytes = new byte[HeaderSize];
        bytes[0] = this.Version;
        bytes[1] = (byte)this.Flags;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(2), this.Reserved);
        return bytes;
    }

    /// <summary>
    /// Byte array'den header okur
    /// </summary>
    public static CachePackageHeader FromBytes(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < HeaderSize)
            throw new InvalidOperationException($"Invalid header size. Expected at least {HeaderSize} bytes, got {bytes.Length}");

        return new CachePackageHeader
        {
            Version = bytes[0],
            Flags = (CachePackageFlags)bytes[1],
            Reserved = BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(2, 2))
        };
    }

    /// <summary>
    /// Eski format (tek byte flag) için backward compatibility
    /// </summary>
    public static CachePackageHeader FromLegacyFormat(byte compressionFlag)
    {
        var flags = compressionFlag == 1 ? CachePackageFlags.Compressed : CachePackageFlags.None;
        return new CachePackageHeader(1, flags);
    }
}

/// <summary>
/// Cache paketi özellik flag'leri
/// </summary>
[Flags]
internal enum CachePackageFlags : byte
{
    None = 0,
    Compressed = 1 << 0,           
    Encrypted = 1 << 1,            
}