using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;

namespace PlanFix2PgSQL;

public static class Utils
{
    private static readonly CultureInfo Provider = new("ru_RU");
    private const DateTimeStyles Style = DateTimeStyles.None;
    private const string DateFormat = "dd-MM-yyyy";
    private const string TimeFormat = "HH:mm";

    internal static string? ParseDateTime(this JsonNode? node)
    {
        if (node is null) return null;
        if (!DateOnly.TryParseExact(node["date"]!.GetValue<string>(), DateFormat, Provider, Style, out var date) ||
            !TimeOnly.TryParseExact(node["time"]!.GetValue<string>(), TimeFormat, Provider, Style, out var time))
            return null;
        var dt = date.ToDateTime(time) + TimeSpan.FromHours(4);
        return $"{dt:yyyy-MM-dd HH:mm}";
    }

    internal static string? FormatAssignees(this JsonNode? node)
    {
        if (node is null) return null;
        var res = new StringBuilder();
        var users = node["users"]!.AsArray();
        if (users.Count > 0)
        {
            foreach (var user in users)
            {
                if (user?["name"]?.GetValue<string>() is { } name)
                    res.Append($"{name}, ");
            }
        }

        var groups = node["groups"]!.AsArray();
        if (groups.Count > 0)
        {
            foreach (var group in groups)
            {
                if (group?["name"]?.GetValue<string>() is { } name)
                    res.Append($"{name}, ");
            }
        }

        return res.ToString()[..^2];
    }

    internal static JsonNode? FromCustomField(this JsonNode? node, int id)
    {
        if (!(node?["customFieldData"]?.AsArray() is { } arr))
            return null;
        foreach (var elem in arr)
            if (elem!["field"]!["id"]!.GetValue<int>() == id)
                return elem["stringValue"];
        return null;
    }
}