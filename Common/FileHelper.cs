using System;
using System.IO;

namespace Common;

public static class FileHelper
{
    public const string XmlExtension = "xml";
    public const string DatExtension = "dat";
    public const string JsonExtension = "json";

    public static readonly byte[] DatSignature = "C?"u8.ToArray();
    public static readonly byte[] XmlSignature = "<?xml"u8.ToArray();

    public static string MakeDestination(string source, string extension, string? overrideName = null)
    {
        var directory = Path.GetDirectoryName(source);
        var destination = directory == null
            ? $"{overrideName ?? Path.GetFileNameWithoutExtension(source)}.{extension}"
            : Path.Combine(directory, $"{overrideName ?? Path.GetFileNameWithoutExtension(source)}.{extension}");
        var modified = destination;
        for (var i = 1; File.Exists(modified); i++)
        {
            modified = directory == null
                ? $"{overrideName ?? Path.GetFileNameWithoutExtension(source)} ({i}).{extension}"
                : Path.Combine(directory, $"{overrideName ?? Path.GetFileNameWithoutExtension(source)} ({i}).{extension}");
        }

        return modified;
    }

    public static bool IsDat(string source, byte[] bytes)
    {
        return source.EndsWith(".dat") || bytes.AsSpan().IndexOf(DatSignature) == 0;
    }

    public static bool IsXml(string source, byte[] bytes)
    {
        return source.EndsWith(".xml") || bytes.AsSpan().IndexOf(XmlSignature) >= 0;
    }
}
