using System.IO;
using UnityEngine;

public static class SaveSystem
{
    static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static bool HasSave()
    {
        return File.Exists(FilePath);
    }

    public static void Save(SaveData data)
    {
        File.WriteAllText(FilePath, JsonUtility.ToJson(data, prettyPrint: true));
    }

    // Throws FileNotFoundException when there is no save; callers check HasSave first.
    public static SaveData Load()
    {
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
    }
}
