using System.Text;
using Common;
using GeometryDashAPI.Data;
using GeometryDashAPI.Data.Models;
using GeometryDashAPI.Levels;
using LevelPack;
using Newtonsoft.Json;
using static Common.ConsoleResult;
using static Common.FileHelper;

void PrintLevel(LevelCreatorModel level, int index)
{
    var color = Console.ForegroundColor;
    Console.Write($"{index}. {level.Name}");
    if (level.Revision == 0)
    {
        Console.WriteLine();
        return;
    }
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($" rev {level.Revision}");
    Console.ForegroundColor = color;
}

LevelCreatorModel? SelectLevel(LocalLevels local, SelectLevelMode mode)
{
    if (mode == SelectLevelMode.Unpack)
        Console.WriteLine("select level for unpack");
    if (mode == SelectLevelMode.Pack)
        Console.WriteLine("select level to save json to");
    var index = 0;
    var dict = new Dictionary<int, LevelCreatorModel>();
    if (mode == SelectLevelMode.Pack)
        PrintLevel(LevelCreatorModel.CreateNew("(make new level)", "Folleach"), 0);
    foreach (var level in local)
    {
        index++;
        PrintLevel(level, index);
        dict[index] = level;
    }

    while (true)
    {
        var input = Console.ReadLine();
        if (int.TryParse(input, out var number))
        {
            if (number == 0 && mode == SelectLevelMode.Pack)
                return null;
            if (dict.TryGetValue(number, out var level))
                return level;
            Console.WriteLine($"select index in range [1, {dict.Count}]");
            continue;
        }

        var levels = local
            .Where(x => x.Name.Equals(input, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        switch (levels.Length)
        {
            case 0:
                Console.WriteLine($"level by name '{input}' not found");
                continue;
            case > 1:
                Console.WriteLine($"too many levels by name '{index}', please select an index of level you want");
                continue;
            default:
                return levels[0];
        }
    }
}

if (args.Length != 1 && args.Length != 2)
    return Error("you need to specify one or two files");

var jsonSettings = new JsonSerializerSettings()
{
    TypeNameHandling = TypeNameHandling.Auto,
    Formatting = Formatting.Indented,
    ContractResolver = new IgnorePropertiesResolver<Level>(
        x => x.Colors
    )
};
var encoding = Encoding.UTF8;
var source = args[0];
var altSource = args.Length > 1 ? args[1] : null;
if (!File.Exists(source))
    return Error($"file '{source}' doesn't exists");
if (altSource != null && !File.Exists(altSource))
    return Error($"file '{altSource}' doesn't exists");

var bytes = File.ReadAllBytes(source);
var altBytes = altSource != null ? File.ReadAllBytes(altSource) : null;
if ((source.EndsWith(".dat") || bytes.AsSpan().IndexOf("C?"u8) == 0) && args.Length == 1)
{
    LocalLevels local;
    try
    {
        local = LocalLevels.LoadFile(source);
    }
    catch (KeyNotFoundException e)
    {
        return Error("Please specify the CCLocalLevels.dat file");
    }

    var model = SelectLevel(local, SelectLevelMode.Unpack);
    if (model == null)
        return Error("failed to select level");
    var level = model.LoadLevel();

    var destination = MakeDestination(
        source,
        JsonExtension,
        overrideName: $"{model.Name}{(model.Revision > 0 ? $". rev {model.Revision}" : "")}");
    var result = JsonConvert.SerializeObject(level, jsonSettings);
    File.WriteAllBytes(destination, encoding.GetBytes(result));
    return Ok();
}

((byte[] data, string name) json, (byte[] data, string name) dat)? SortSources()
{
    if (source.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
        && altSource!.EndsWith(".dat", StringComparison.OrdinalIgnoreCase))
        return ((bytes, source), (altBytes, altSource))!;
    if (source.EndsWith(".dat", StringComparison.OrdinalIgnoreCase)
        && altSource!.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        return ((altBytes, altSource), (bytes, source))!;
    return null;
}

var targets = SortSources();
if (targets == null)
    return Error("Please specify '.dat' and '.json' files together");

{
    var local = LocalLevels.LoadFile(targets.Value.dat.name);
    var level = JsonConvert.DeserializeObject<Level>(encoding.GetString(targets.Value.json.data), jsonSettings);
    if (level == null)
        return Error($"failed to read json file '{targets.Value.json.name}'");

    var model = SelectLevel(local, SelectLevelMode.Pack);
    if (model == null)
    {
        var levelName = Question.Make(
            "Level name",
            x => !string.IsNullOrWhiteSpace(x),
            "The level name cannot be empty")!;
        var authorName = Question.Make(
            "Author name",
            x => x?.Length > 2,
            "The author's name must have at least 3 letters")!;
        model = LevelCreatorModel.CreateNew(levelName, authorName);
        model.SaveLevel(level);
        local.AddLevel(model);
    }
    else
        model.SaveLevel(level);

    local.Save(targets.Value.dat.name);
    return Ok();
}
