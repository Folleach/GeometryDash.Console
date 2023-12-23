using System;
using System.Reflection;

namespace Common;

public static class ConsoleResult
{
    public static int Error(string message, string usage)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(message);
        Console.ForegroundColor = color;
        Console.WriteLine(usage);
        Console.WriteLine($"version: {Assembly.GetExecutingAssembly().GetName().Version}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("press any key to exit...");
        Console.ForegroundColor = color;
        Console.ReadKey();
        return 1;
    }

    public static int Ok() => 0;
}
