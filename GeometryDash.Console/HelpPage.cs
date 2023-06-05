namespace GeometryDash.Console;

internal static class HelpPage
{
    internal static int Show()
    {
        System.Console.WriteLine(@"
Usage: gd <command> ...options

Commands:
  pack <dat file> <destination xml file>
    Unpacking .dat file of the game

  unpack <xml file> <destination dat file>
    Packs .dat file of the game

  featured
    Lists the featured levels
");
        return 0;
    }
}
