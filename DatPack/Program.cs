using System.Reflection;
using System.Xml.Linq;
using GeometryDashAPI.Data;
using GeometryDashAPI.Serialization;

int Error(string message)
{
    var color = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.DarkRed;
    Console.WriteLine(message);
    Console.ForegroundColor = color;
    Console.WriteLine("usage: datpack <file_name>");
    Console.WriteLine($"version: {Assembly.GetExecutingAssembly().GetName().Version}");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("press any key to exit...");
    Console.ForegroundColor = color;
    Console.ReadKey();
    return 1;
}

int Ok() => 0;

const string xmlExtension = "xml";
const string datExtension = "dat";

string MakeDestination(string source, string extension)
{
    var directory = Path.GetDirectoryName(source);
    var destination = directory == null
        ? Path.Combine(Path.GetFileNameWithoutExtension(source) + "." + extension)
        : Path.Combine(directory, Path.GetFileNameWithoutExtension(source) + "." + extension);
    var modified = destination;
    for (var i = 1; File.Exists(modified); i++)
    {
        modified = directory == null
            ? Path.Combine(Path.GetFileNameWithoutExtension(source) + $" ({i})." + extension)
            : Path.Combine(directory, Path.GetFileNameWithoutExtension(source) + $" ({i})." + extension);
    }

    return modified;
}

if (args.Length != 1)
    return Error("the file isn't specified");

var source = args[0];
if (!File.Exists(source))
    return Error("file doesn't exists");

var bytes = File.ReadAllBytes(source);
if (source.EndsWith(".dat") || bytes.AsSpan().IndexOf(new byte[] { 0x43, 0x3f }) == 0)
{
    var data = new GameData();
    data.LoadAsync(source).GetAwaiter().GetResult();

    var memory = new MemoryStream();
    data.DataPlist.SaveToStream(memory);
    memory.Seek(0, SeekOrigin.Begin);
    var document = XDocument.Load(memory);
    using var stream = new FileStream(MakeDestination(source, xmlExtension), FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
    document.Save(stream);
    return Ok();
}

if (source.EndsWith(".xml") || bytes.AsSpan().IndexOf(new byte[] { 0x3f, 0x78, 0x6d, 0x6c }) >= 0)
{
    var file = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
    var data = new GameData
    {
        DataPlist = new Plist(file)
    };
    data.Save(MakeDestination(source, datExtension));
    return Ok();
}

return Error($"unknown file format: '{source.Split(".").LastOrDefault()}'");

