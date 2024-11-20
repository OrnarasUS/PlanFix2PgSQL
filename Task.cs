using System.Text.Json.Nodes;

namespace PlanFix2PgSQL;

public class Task()
{
    public string Id { get; set; } = null!;
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? Template { get; set; }
    public string? Service { get; set; }
    public string? Partner { get; set; }
    public string? Executors { get; set; }
    public string? Mark { get; set; }
    public string? Created { get; set; }
    public string? Accepted { get; set; }
    public string? Started { get; set; }
    public string? Billsended { get; set; }
    public string? Planended { get; set; }
    public string? Ended { get; set; }
    public string? Retailended { get; set; }
    public string? Owner { get; set; }
    public string? Planendedonly { get; set; }

    public Task(JsonNode info): this()
    {
        Id = $"{info["id"]!.GetValue<int>()}";
        Name = info["name"]?.GetValue<string>();
        Executors = info["assignees"].FormatAssignees();
        var templateId = info["template"]?["id"]?.GetValue<int>();
        Template = templateId is null ? null : Program.Templates[(int)templateId];
        Status = info["status"]?["name"]?.GetValue<string>();
        Owner = info["assigner"]?["name"]?.GetValue<string>();
        Created = info["dateTime"].ParseDateTime();
        Partner = info["counterparty"]?["name"]?.GetValue<string>();
        Planended = info["endDateTime"].ParseDateTime();
        Mark = info.FromCustomField(114320)?.GetValue<string>();
        Ended = info.FromCustomField(114280)?.GetValue<string>();
        Service = info.FromCustomField(114278)?.GetValue<string>();
        Started = info.FromCustomField(114286)?.GetValue<string>();
        Accepted = info.FromCustomField(114320)?.GetValue<string>();
        Billsended = info.FromCustomField(114394)?.GetValue<string>();
        Retailended = info.FromCustomField(114424)?.GetValue<string>();
        Planendedonly = info.FromCustomField(114288)?.GetValue<string>();
    }
}
