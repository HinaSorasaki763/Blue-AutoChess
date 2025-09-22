using Firebase.Firestore;
using System.Collections.Generic;
using System.Linq;

public static class FirestoreConverter
{
    public static Dictionary<string, object> ToDict(WaveGridSlotData slot)
    {
        return new Dictionary<string, object>
        {
            { "GridIndex", slot.GridIndex },
            { "CharacterID", slot.CharacterID },
            { "Star", slot.Star },
            { "DummyGridIndex", slot.DummyGridIndex },
            { "EquipmentID", slot.EquipmentID }  // List<int> 會自動轉成 Array
        };
    }
}
public static class FirestoreDebugExt
{
    public static string ToDebugString(this Dictionary<string, object> dict)
    {
        List<string> parts = new List<string>();
        foreach (var kv in dict)
        {
            if (kv.Value is IEnumerable<object> list && !(kv.Value is string))
                parts.Add($"{kv.Key}=[{string.Join(",", list)}]");
            else
                parts.Add($"{kv.Key}={kv.Value}");
        }
        return string.Join(", ", parts);
    }
    public static string DictToString(object obj)
    {
        if (obj is Dictionary<string, object> dict)
        {
            return "{" + string.Join(", ", dict.Select(kv => $"{kv.Key}={DictToString(kv.Value)}")) + "}";
        }
        else if (obj is List<object> list)
        {
            return "[" + string.Join(", ", list.Select(DictToString)) + "]";
        }
        else
        {
            return obj?.ToString() ?? "null";
        }
    }
}