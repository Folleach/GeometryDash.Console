using GeometryDashAPI.Data.Models;

namespace LevelPack;

public static class LevelCopyMachine
{
    private static readonly HashSet<string> ExcludeInCopy =
    [
        "k1", "k26", "k3", "k6", "k60", "k9", "k10", "k11", "k22", "k17",
        "k80", "k83", "k65", "k27", "k41", "k48", "k67", "k66",

        // These fields are there, but they are entered through the LevelCreatorModel constructor
        "k2", // name
        "k5" // author
    ];

    public static LevelCreatorModel CreateCopy(LevelCreatorModel model, string author)
    {
        var result = LevelCreatorModel.CreateNew($"{model.Name ?? "undefined"} 2", author ?? "undefined");
        foreach (var (key, value) in model.DataLevel)
        {
            if (ExcludeInCopy.Contains(key))
                continue;
            result.DataLevel[key] = value;
        }

        if (model.DataLevel.TryGetValue("k1", out var id))
            result.DataLevel["k42"] = id;
        result.DataLevel["k21"] = 2;
        result.Version = 1;
        // todo: field 'k81' has transform: 24125 -> 81975 what is that?
        return result;
    }
}
