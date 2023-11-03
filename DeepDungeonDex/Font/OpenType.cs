using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace DeepDungeonDex.Font;

internal record CmapFormat(ushort Format, ushort Length, ushort Language)
{
    public static CmapFormat Read(BinaryReader reader, uint offset)
    {
        reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        var format = reader.ReadBigEndian<ushort>();
        var length = reader.ReadBigEndian<ushort>();
        var language = reader.ReadBigEndian<ushort>();
        switch (format)
        {
            case 4:
                var segCountX2 = reader.ReadBigEndian<ushort>();
                var searchRange = reader.ReadBigEndian<ushort>();
                var entitySelector = reader.ReadBigEndian<ushort>();
                var rangeShift = reader.ReadBigEndian<ushort>();
                var endCodes = new List<ushort>();
                for (var i = 0; i < segCountX2 / 2; i++)
                {
                    endCodes.Add(reader.ReadBigEndian<ushort>());
                }
                var reservedPad = reader.ReadBigEndian<ushort>();
                var startCodes = new List<ushort>();
                for (var i = 0; i < segCountX2 / 2; i++)
                {
                    startCodes.Add(reader.ReadBigEndian<ushort>());
                }
                var idDeltas = new List<short>();
                for (var i = 0; i < segCountX2 / 2; i++)
                {
                    idDeltas.Add(reader.ReadBigEndian<short>());
                }
                var idRangeOffsets = new List<ushort>();
                for (var i = 0; i < segCountX2 / 2; i++)
                {
                    idRangeOffsets.Add(reader.ReadBigEndian<ushort>());
                }
                var glyphIds = new List<ushort>();
                while (reader.BaseStream.Position < offset + length)
                {
                    glyphIds.Add(reader.ReadBigEndian<ushort>());
                }
                return new CmapFormat4(format, length, language, segCountX2, searchRange, entitySelector, rangeShift, endCodes, reservedPad, startCodes, idDeltas, idRangeOffsets, glyphIds);
            default:
                throw new NotSupportedException();
        }
    }
}

internal record CmapFormat4(ushort Format, ushort Length, ushort Language, ushort SegCountX2, ushort SearchRange, ushort EntitySelector, ushort RangeShift, List<ushort> EndCodes, ushort ReservedPad, List<ushort> StartCodes, List<short> IdDeltas, List<ushort> IdRangeOffsets, List<ushort> GlyphIds) : CmapFormat(Format, Length, Language);

internal record CmapTable(ushort Version, ushort NumTables, List<CmapEncodingRecords> Subtables)
{
    public static CmapTable Read(BinaryReader reader, uint offset)
    {
        reader.BaseStream.Seek(offset, SeekOrigin.Begin);
        var version = reader.ReadBigEndian<ushort>();
        var numTables = reader.ReadBigEndian<ushort>();
        var subtables = new List<CmapEncodingRecords>();
        for (var i = 0; i < numTables; i++)
        {
            subtables.Add(CmapEncodingRecords.Read(reader));
        }
        return new CmapTable(version, numTables, subtables);
    }
}

internal record CmapEncodingRecords(ushort PlatformID, ushort EncodingID, uint Offset)
{
    public static CmapEncodingRecords Read(BinaryReader reader)
    {
        var platformID = reader.ReadBigEndian<ushort>();
        var encodingID = reader.ReadBigEndian<ushort>();
        var offset = reader.ReadBigEndian<uint>();
        return new CmapEncodingRecords(platformID, encodingID, offset);
    }
}

internal record TableRecord(string Tag, uint Checksum, uint Offset, uint Length)
{
    public static TableRecord Read(BinaryReader reader)
    {
        var tag = Encoding.ASCII.GetString(reader.ReadBytes(4));
        var checksum = reader.ReadBigEndian<uint>();
        var offset = reader.ReadBigEndian<uint>();
        var length = reader.ReadBigEndian<uint>();
        return new TableRecord(tag, checksum, offset, length);
    }
}

internal record TableDirectory(uint Version, ushort NumTables, ushort SearchRange, ushort EntrySelector,
    ushort RangeShift)
{
    public static TableDirectory Read(BinaryReader reader)
    {
        var version = reader.ReadBigEndian<uint>();
        var numTables = reader.ReadBigEndian<ushort>();
        var searchRange = reader.ReadBigEndian<ushort>();
        var entrySelector = reader.ReadBigEndian<ushort>();
        var rangeShift = reader.ReadBigEndian<ushort>();
        return new TableDirectory(version, numTables, searchRange, entrySelector, rangeShift);
    }
}

public static class BinaryReaderExtensions
{
    public static T ReadBigEndian<T>(this BinaryReader reader) where T : struct
    {
        return typeof(T) switch
        {
            var t when t == typeof(uint) => (T)(object)BinaryPrimitives.ReadUInt32BigEndian(reader.ReadBytes(4)),
            var t when t == typeof(ushort) => (T)(object)BinaryPrimitives.ReadUInt16BigEndian(reader.ReadBytes(2)),
            var t when t == typeof(short) => (T)(object)BinaryPrimitives.ReadInt16BigEndian(reader.ReadBytes(2)),
            var t when t == typeof(int) => (T)(object)BinaryPrimitives.ReadInt32BigEndian(reader.ReadBytes(4)),
            var t when t == typeof(float) => (T)(object)BinaryPrimitives.ReadSingleBigEndian(reader.ReadBytes(4)),
            var t when t == typeof(double) => (T)(object)BinaryPrimitives.ReadDoubleBigEndian(reader.ReadBytes(8)),
            var t when t == typeof(long) => (T)(object)BinaryPrimitives.ReadInt64BigEndian(reader.ReadBytes(8)),
            var t when t == typeof(ulong) => (T)(object)BinaryPrimitives.ReadUInt64BigEndian(reader.ReadBytes(8)),
            var t when t == typeof(byte) => (T)(object)reader.ReadByte(),
            var t when t == typeof(sbyte) => (T)(object)reader.ReadSByte(),
            var t when t == typeof(bool) => (T)(object)reader.ReadBoolean(),
            _ => throw new NotSupportedException()
        };
    }
}