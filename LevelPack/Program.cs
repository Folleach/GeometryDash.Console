using System.Text;
using Common;
using GeometryDashAPI.Data;
using GeometryDashAPI.Data.Models;
using GeometryDashAPI.Levels;
using GeometryDashAPI.Serialization;
using LevelPack;
using Newtonsoft.Json;
using static Common.ConsoleResult;
using static Common.FileHelper;

var usage = @"usage: LevelPack <CCLocalLevels.dat> - for unpack
usage: LevelPack <CCLocalLevels.dat> <json level file> - for pack
usage: LevelPack <CCLocalLevels.dat> <CCGameManager.dat> - for copy any level to your editor";

if (args.Length != 1 && args.Length != 2)
    return Error("You need to specify one or two files", usage);

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
    return Error($"File '{source}' doesn't exists", usage);
if (altSource != null && !File.Exists(altSource))
    return Error($"File '{altSource}' doesn't exists", usage);

var bytes = File.ReadAllBytes(source);
var altBytes = altSource != null ? File.ReadAllBytes(altSource) : null;
if ((source.EndsWith(".dat") || bytes.AsSpan().IndexOf(DatSignature) == 0) && args.Length == 1)
{
    LocalLevels local;
    try
    {
        local = LocalLevels.LoadFile(source);
    }
    catch (KeyNotFoundException e)
    {
        return Error("Please specify the CCLocalLevels.dat file", usage);
    }

    var model = SelectLevel(local.ToArray(), SelectLevelMode.Unpack);
    if (model == null)
        return Error("Failed to select level", usage);
    var level = model.LoadLevel();

    var destination = MakeDestination(
        source,
        JsonExtension,
        overrideName: $"{model.Name}{(model.Revision > 0 ? $". rev {model.Revision}" : "")}");
    var result = JsonConvert.SerializeObject(level, jsonSettings);
    File.WriteAllBytes(destination, encoding.GetBytes(result));
    return Ok();
}

var failedLoadDatFileTogetherError = @"For copy any level you should specify CCLocalLevels and CCGameManager together.
Also you should download at least one level in the game";
if (altSource != null && altBytes != null && IsDat(source, bytes) && IsDat(altSource, altBytes))
{
    string? localFileName = null;
    LocalLevels? local = null;
    GameManager? manager = null;

    try
    {
        local = LocalLevels.LoadFile(source);
        if (!local.DataPlist.ContainsKey("LLM_01"))
        {
            local = null;
            throw new Exception();
        }

        localFileName = source;
        manager = GameManager.LoadFile(altSource);
        if (!manager.DataPlist.ContainsKey("GLM_03"))
            throw new Exception();
    }
    catch
    {
        // ignored
    }

    if (local == null)
    {
        try
        {
            local = LocalLevels.LoadFile(altSource);
            if (!local.DataPlist.ContainsKey("LLM_01"))
                return Error(failedLoadDatFileTogetherError, usage);
            localFileName = altSource;
        }
        catch
        {
            return Error(failedLoadDatFileTogetherError, usage);
        }
    }

    if (manager == null)
    {
        try
        {
            manager = GameManager.LoadFile(source);
            if (!manager.DataPlist.ContainsKey("GLM_03"))
                return Error(failedLoadDatFileTogetherError, usage);
        }
        catch
        {
            return Error(failedLoadDatFileTogetherError, usage);
        }
    }

    var savedLevels = (manager.DataPlist["GLM_03"] as Plist)!.Select(x => new LevelCreatorModel(x.Key, x.Value));
    var toCopy = SelectLevel(savedLevels.ToArray(), SelectLevelMode.Copy);

    if (toCopy == null)
        return Error("Failed to select level", usage);

    local.AddLevel(LevelCopyMachine.CreateCopy(toCopy, manager.PlayerName));
    local.Save(localFileName);
    return Ok();
}

var targets = SortSources();
if (targets == null)
    return Error("Please specify '.dat' and '.json' files together for save level to editor", usage);

{
    var local = LocalLevels.LoadFile(targets.Value.dat.name);
    var level = JsonConvert.DeserializeObject<Level>(encoding.GetString(targets.Value.json.data), jsonSettings);
    if (level == null)
        return Error($"Failed to read json file '{targets.Value.json.name}'", usage);

    var model = SelectLevel(local.ToArray(), SelectLevelMode.Pack);
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

// end of program

void PrintLevel(LevelCreatorModel level, int index, SelectLevelMode mode)
{
    var color = Console.ForegroundColor;
    Console.Write($"{index}. {level.Name}");
    if (mode == SelectLevelMode.Copy)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(" by ");
        Console.ForegroundColor = color;
        Console.Write(level.AuthorName);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        var id = level.DataLevel.TryGetValue("k42", out var idk42)
            ? idk42
            : level.DataLevel.TryGetValue("k1", out var idk1)
                ? idk1
                : 0;
        Console.WriteLine($" (id: {id})");
        Console.ForegroundColor = color;
        return;
    }
    if (level.Revision == 0)
    {
        Console.WriteLine();
        Console.ForegroundColor = color;
        return;
    }
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($" rev {level.Revision}");
    Console.ForegroundColor = color;
}

LevelCreatorModel? SelectLevel(ICollection<LevelCreatorModel> local, SelectLevelMode mode)
{
    switch (mode)
    {
        case SelectLevelMode.Unpack: Console.WriteLine("select level for unpack"); break;
        case SelectLevelMode.Pack: Console.WriteLine("select level to save json to"); break;
        case SelectLevelMode.Copy: Console.WriteLine("select level for copy to your editor"); break;
    }

    var index = 0;
    var dict = new Dictionary<int, LevelCreatorModel>();
    if (mode == SelectLevelMode.Pack)
        PrintLevel(LevelCreatorModel.CreateNew("(make new level)", "Folleach"), 0, mode);
    foreach (var level in local)
    {
        index++;
        PrintLevel(level, index, mode);
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
            Console.WriteLine($"Select index in range [1, {dict.Count}]");
            continue;
        }

        var levels = local
            .Where(x => x.Name.Equals(input, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        switch (levels.Length)
        {
            case 0:
                Console.WriteLine($"Level by name '{input}' not found");
                continue;
            case > 1:
                Console.WriteLine($"Too many levels by name '{index}', please select an index of level you want");
                continue;
            default:
                return levels[0];
        }
    }
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
