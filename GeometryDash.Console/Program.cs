using Folleach.ConsoleUtils;
using GeometryDash.Console;
using GeometryDash.Console.Commands;

if (args.Length == 0)
    return HelpPage.Show();

var pargs = new Args(args);
if (pargs.Contains("help") && pargs.Contains("h"))
    return HelpPage.Show();

var commands = new Dictionary<string, Func<ICommand>>()
{
    ["unpack"] = () => new UnpackDataFile(),
    ["pack"] = () => new PackDataFile(),
    ["featured"] = () => new FeaturesCommand()
};

var commandName = args[0];
if (!commands.TryGetValue(commandName, out var getCommand))
{
    ExConsole.WriteLineColor($"command '{commandName}' not found :(", ConsoleColor.DarkRed);
    HelpPage.Show();
    return 1;
}

return getCommand().Execute(args.Skip(1).ToArray());
