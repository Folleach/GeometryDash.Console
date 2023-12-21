using System.Xml.Linq;
using GeometryDashAPI.Data;
using GeometryDashAPI.Serialization;
using static Common.ConsoleResult;
using static Common.FileHelper;

if (args.Length != 1)
    return Error("the file isn't specified");

var source = args[0];
if (!File.Exists(source))
    return Error("file doesn't exists");

var bytes = File.ReadAllBytes(source);
if (source.EndsWith(".dat") || bytes.AsSpan().IndexOf("C?"u8) == 0)
{
    var data = new GameData();
    data.LoadAsync(source).GetAwaiter().GetResult();

    var memory = new MemoryStream();
    data.DataPlist.SaveToStream(memory);
    memory.Seek(0, SeekOrigin.Begin);
    var document = XDocument.Load(memory);
    using var stream = new FileStream(MakeDestination(source, XmlExtension), FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
    document.Save(stream);
    return Ok();
}

if (source.EndsWith(".xml") || bytes.AsSpan().IndexOf("?xml"u8) >= 0)
{
    var file = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
    var data = new GameData
    {
        DataPlist = new Plist(file)
    };
    data.Save(MakeDestination(source, DatExtension));
    return Ok();
}

return Error($"unknown file format: '{source.Split(".").LastOrDefault()}'");
