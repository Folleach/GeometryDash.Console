using System.Xml.Linq;
using GeometryDashAPI.Data;

namespace GeometryDash.Console.Commands;

public class UnpackDataFile : ICommand
{
    public int Execute(string[] args)
    {
        if (args.Length != 2)
            return ShowUsage();
        var source = args[0];
        var destination = args[1];
        var data = new GameData();
        data.LoadAsync(source).GetAwaiter().GetResult();

        var memory = new MemoryStream();
        data.DataPlist.SaveToStream(memory);
        memory.Seek(0, SeekOrigin.Begin);
        var document = XDocument.Load(memory);
        using var stream = new FileStream(destination, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        document.Save(stream);
        return 0;
    }

    private int ShowUsage()
    {
        System.Console.WriteLine("Usage: unpack <xml file> <destination dat file>");
        return 2;
    }
}
