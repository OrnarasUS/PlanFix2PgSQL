using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using CommandLine;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;
using ATask = System.Threading.Tasks.Task;

namespace PlanFix2PgSQL;

public static partial class Program
{
    internal static Options Arguments
    {
        get => _arguments;
        private set
        {
            if (_arguments == null!)
                _arguments = value;
        }
    }
    private const string ErrorRequestMessage = "При отправке запроса произошла ошибка...";
    private static readonly HttpClient Http = new();
    private static readonly Logger Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.File("logs/", rollingInterval: RollingInterval.Day,
            flushToDiskInterval: new TimeSpan(0,0,0,0,50),
            outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message}{NewLine}{Exception}")
        .WriteTo.Console(outputTemplate:"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message}{NewLine}{Exception}")
        .CreateLogger();

    private static Options _arguments = null!;

    internal static readonly Dictionary<int, string> Templates = new();
    
    public static async ATask Main(string[] args)
    {
        try
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o => Arguments = o);
            await GetTemplates();
            await using var ctx = new PostgresContext();
            for (var i = Arguments.idEnd; i >= Arguments.idBegin; i--)
            {
                Logger.Information($"Выгрузка заявки №{i}");
                if (await GetTask(i, ctx) is { } task)
                    await PushTask(task, ctx);
                await ATask.Delay(1000);
            }
        }
        catch (Exception e)
        {
            Logger.Fatal(e, "Программа остановлена.");
        }
        
    }

    private static async ATask GetTemplates()
    {
        HttpResponseMessage resp = null!;

        #region Отправка запроса

        try
        {
            var uri = $"https://{Arguments.pfUserName}.planfix.ru/rest/task/templates?fields=name";
            var req = new HttpRequestMessage(HttpMethod.Get, uri);
            req.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Arguments.pfToken);
            resp = await Http.SendAsync(req);
        }
        catch (Exception e)
        {
            Logger.Error(e, ErrorRequestMessage);
        }

        #endregion
        #region Обработка ответа

        try
        {
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                if (resp.StatusCode == HttpStatusCode.BadRequest)
                    throw new HttpRequestException("Не удалось выгрузить шаблоны!");
                throw new HttpRequestException(body);
            }
            var json = JsonNode.Parse(body);
            if (!(json?["templates"]?.AsArray() is {} arr))
                throw new JsonException($"Не удалось конвертировать ответ в JsonNode...\nТело ответа: {body}");
            foreach (var el in arr)
                Templates.Add(el!["id"]!.GetValue<int>(), el["name"]!.GetValue<string>());
        }
        catch (Exception e)
        {
            Logger.Error(e, ErrorRequestMessage);
        }

        #endregion
    }
    
    private static async Task<Task?> GetTask(int id, PostgresContext ctx)
    {
        HttpResponseMessage resp;

        #region Отправка запроса

        
        try
        {
            var fields = new[]
            {
                "id", "name", "status", "assigner", "counterparty", "template", "assignees", "114278", "dateTime",
                "114320", "114394", "114280", "endDateTime", "114288", "114424", "114286", "114320"
            };
            var uri = $"https://{Arguments.pfUserName}.planfix.ru/rest/task/{id}?fields={string.Join(',', fields)}";
            var req = new HttpRequestMessage(HttpMethod.Get, uri);
            req.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Arguments.pfToken);
            resp = await Http.SendAsync(req);
        }
        catch (Exception e)
        {
            Logger.Error(e, ErrorRequestMessage);
            return null;
        }

        #endregion
        #region Обработка ответа

        try
        {
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                if (resp.StatusCode != HttpStatusCode.BadRequest)
                    throw new HttpRequestException(body);
                await TryDelete(id, ctx);
                throw new TaskRemovedException();
            }
            var json = JsonNode.Parse(body);
            if (json is null)
                throw new JsonException($"Не удалось конвертировать ответ в JsonNode...\nТело ответа: {body}");
            var info = json["task"]!;
            return new Task(info);
        }
        catch (Exception e)
        {
            if(e is TaskRemovedException)
                Logger.Warning(e.Message);
            else
                Logger.Error(e, ErrorRequestMessage);
            return null;
        }

        #endregion
    }

    private static async ATask PushTask(Task task, PostgresContext ctx)
    {
        if (await ctx.Tasks.AnyAsync(t => t.Id == task.Id))
            ctx.Tasks.Update(task);
        else
            ctx.Tasks.Add(task);
        await ctx.SaveChangesAsync();
    }

    private static async ATask TryDelete(int id, PostgresContext ctx)
    {
        var t = await ctx.Tasks.FirstOrDefaultAsync(i => i.Id == $"{id}");
        if (t is null) return;
        ctx.Tasks.Remove(t);
        await ctx.SaveChangesAsync();
    }
}
