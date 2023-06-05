namespace GeometryDash.Console;

internal static class ExConsole
{
    public static void WriteColor(string? text, ConsoleColor color)
    {
        var t = System.Console.ForegroundColor;
        System.Console.ForegroundColor = color;
        System.Console.Write(text);
        System.Console.ForegroundColor = t;
    }

    public static void WriteLineColor(string? text, ConsoleColor color)
    {
        var t = System.Console.ForegroundColor;
        System.Console.ForegroundColor = color;
        System.Console.WriteLine(text);
        System.Console.ForegroundColor = t;
    }
}
