using System.Text;
using System.Text.Json;
using Folleach.ConsoleUtils;
using GeometryDashAPI.Server;
using GeometryDashAPI.Server.Responses;

namespace GeometryDash.Console.Commands;

public class FeaturesCommand : ICommand
{
    public int Execute(string[] args)
    {
        var pargs = new Args(args);
        var isHelp = pargs.Contains("help");
        if (isHelp)
            return ShowHelp();
        var asJson = pargs.Contains("json");
        var page = pargs.GetString("page") ?? pargs.GetString("p");
        if (string.IsNullOrWhiteSpace(page))
            page = "0";

        var client = new GameClient();
        var response = client.GetFeaturedLevelsAsync(int.TryParse(page, out var p) ? p : 0).GetAwaiter().GetResult();
        var result = response.GetResultOrDefault();
        if (asJson)
            ShowJson(result, true);
        else
            ShowHumanReadable(result);
        return 0;
    }

    private void ShowHumanReadable(LevelPageResponse result)
    {
        var index = 0;
        foreach (var level in result.Levels)
        {
            ExConsole.WriteColor($"{++index}. ", ConsoleColor.DarkGray);
            ExConsole.WriteColor(level.Name, ConsoleColor.Green);
            ExConsole.WriteColor(" by ", ConsoleColor.DarkGray);
            ExConsole.WriteLineColor(result.Authors.FirstOrDefault(x => x.UserId == level.AuthorUserId)?.UserName, ConsoleColor.Gray);

            if (level.Demon)
                ExConsole.WriteColor(Filled($"{level.DemonDifficulty.ToString()}", 7), ConsoleColor.Red);
            else if (level.Auto)
                ExConsole.WriteColor(Filled($"auto", 7), ConsoleColor.DarkYellow);
            else
                ExConsole.WriteColor(Filled(level.DifficultyIcon.ToString().ToLower(), 7), ConsoleColor.Gray);
            ExConsole.WriteColor(" ♦ ", ConsoleColor.Yellow);
            ExConsole.WriteColor(level.Stars.ToString(), ConsoleColor.Gray);
            ExConsole.WriteColor(" ", ConsoleColor.Gray);
            ExConsole.WriteLineColor(level.Length.ToString().ToLower(), ConsoleColor.Gray);

            ExConsole.WriteColor("Downloads: ", ConsoleColor.DarkGray);
            ExConsole.WriteColor(level.Downloads.ToString(), ConsoleColor.Gray);
            ExConsole.WriteColor(", ", ConsoleColor.DarkGray);
            ExConsole.WriteColor("Likes: ", ConsoleColor.DarkGray);
            ExConsole.WriteLineColor(level.Likes.ToString(), ConsoleColor.Gray);

            ExConsole.WriteLineColor(level.Description, ConsoleColor.Gray);
            ExConsole.WriteLineColor(result.Musics.FirstOrDefault(x => x.MusicId == level.MusicId)?.Url, ConsoleColor.Blue);
            System.Console.WriteLine();
        }
    }

    private static string Filled(string value, int length)
    {
        var diff = length - value.Length;
        return diff <= 0 ? value : $"{value}{new string(' ', diff)}";
    }

    private void ShowJson(LevelPageResponse result, bool pretty)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(result, new JsonSerializerOptions()
        {
            WriteIndented = pretty,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        System.Console.WriteLine(Encoding.UTF8.GetString(json));
    }

    private static int ShowHelp()
    {
        System.Console.WriteLine(@"
Usage: features
  --page, -p   page number, starts with 0
  --json       output format in json");
        return 0;
    }
}
