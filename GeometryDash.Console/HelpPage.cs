namespace GeometryDash.Console;

internal static class HelpPage
{
    internal static int Show()
    {
        System.Console.WriteLine(@"
Usage: GeometryDash.Console.exe <command> ...options

Commands:
  pack <dat file> <destination xml file>
    Unpacking .dat file of the game

  unpack <xml file> <destination dat file>
    Packs .dat file of the game
");
        return 0;
    }
}
