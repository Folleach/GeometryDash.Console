using GeometryDashAPI.Data;
using GeometryDashAPI.Serialization;

namespace GeometryDash.Console.Commands;

public class PackDataFile : ICommand
{
    public int Execute(string[] args)
    {
        if (args.Length != 2)
            return ShowUsage();
        var source = args[0];
        var destination = args[1];
        var file = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
        var data = new GameData
        {
            DataPlist = new Plist(file)
        };
        data.Save(destination);
        return 0;
    }

    private int ShowUsage()
    {
        System.Console.WriteLine("Usage: pack <dat file> <destination xml file>");
        return 2;
    }
}
