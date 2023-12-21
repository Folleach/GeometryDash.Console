using System.IO;

namespace Common;

public static class FileHelper
{
    public const string XmlExtension = "xml";
    public const string DatExtension = "dat";
    public const string JsonExtension = "json"; 

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
}
