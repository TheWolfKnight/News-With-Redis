using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using StackExchange.Redis;

namespace NewsWithRedis.Infrastructure.Helpers;

internal static class HashHelper
{

    internal static HashEntry[] ToHashEntries<T>(T obj)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var hashList = new List<HashEntry>(properties.Length);

        foreach (var p in properties)
        {
            if (!p.CanRead || p.GetIndexParameters().Length > 0) { continue; }
            var value = p.GetValue(obj);
            if (value is null) { continue; }

            hashList.Add(new HashEntry(p.Name, RedisNormalize(value)));
        }
        return hashList.ToArray();
    }

    internal static HashBuild BuildHashSet<T>(List<T> items, string articleKey)
    {
        List<Hash> members = new();
        List<(string key, int score)> indexEntries = new();
        string indexKey = $"{articleKey}:comments";

        if (items != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                string commentKey = $"{articleKey}:comments:{i}";
                var entries = ToHashEntries(item);

                members.Add(new Hash(entries, commentKey));
                indexEntries.Add((commentKey, i));
            }
        }
        return new HashBuild(members, indexEntries, indexKey);
    }

    internal static T FromHashEntries<T>(HashEntry[] entries) //where T : new()
    {
        var obj = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        var dict = entries.ToDictionary(entry => (string)entry.Name,
                                        entry => (string)entry.Value);
        foreach (var p in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!p.CanWrite || !dict.TryGetValue(p.Name, out var s)) { continue; }
            p.SetValue(obj, FromString(s, p.PropertyType));
        }
        return obj;
    }

    private static RedisValue RedisNormalize(object v) => v switch
    {
        DateTimeOffset offset => offset.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
        DateTime time => DateTime.SpecifyKind(time, DateTimeKind.Utc).ToString("o", CultureInfo.InvariantCulture),
        bool b => b ? "1" : "0",
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => v.ToString()!
    };

    private static object? FromString(string str, Type type)
    {
        if (type == typeof(string)) return str;
        if (type == typeof(int)) return int.Parse(str, CultureInfo.InvariantCulture);
        if (type == typeof(long)) return long.Parse(str, CultureInfo.InvariantCulture);
        if (type == typeof(bool)) return str == "1" || str.Equals("true", StringComparison.OrdinalIgnoreCase);
        if (type == typeof(DateTimeOffset))
        {
            if (DateTimeOffset.TryParseExact(str, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var offset)) return offset;
            if (long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unix)) return DateTimeOffset.FromUnixTimeSeconds(unix);
            throw new FormatException($"Cannot Parse DateTimeOffset from '{str}'.");
        }
        if (type == typeof(DateTime))
        {
            if (DateTime.TryParseExact(str, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var time)) return time.Kind == DateTimeKind.Utc ? time : time.ToUniversalTime();
            if (long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unix)) return DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime;
            throw new FormatException($"Cannot parse DateTime from '{str}'.");
        }
        if (type.IsEnum) return Enum.Parse(type, str, true);
        return str;
    }
}

internal readonly record struct Hash(HashEntry[] Entries, string key);
internal readonly record struct HashBuild(IReadOnlyList<Hash> Members, IReadOnlyList<(string memberkey, int score)> IndexEntries, string IndexKey);
