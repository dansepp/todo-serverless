#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

private static TelemetryClient telemetry = new TelemetryClient();
private static string key = TelemetryConfiguration.Active.InstrumentationKey = System.Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);


public static HttpResponseMessage Run(HttpRequestMessage req, IQueryable<TodoItem> inTable, TraceWriter log)
{
    log.Info("todo-serverless-api) :: Todo-GET-All. Started.");

    var startTime = DateTime.UtcNow;
    var timer = System.Diagnostics.Stopwatch.StartNew();
    log.Info("todo-serverless-api :: Todo-GET-All. About to retrieve all todo items...");

    var query = from todoitems in inTable select new { id = todoitems.RowKey, todoitems.title, todoitems.description, todoitems.due, todoitems.isComplete };
    var todoList = query.ToList();
    telemetry.TrackDependency("StorageTransaction-Query", "Context", startTime, timer.Elapsed, true);
    log.Info("todo-serverless-api :: Todo-GET-All. Retrieved all todo items.");

    log.Info("todo-serverless-api :: Todo-GET-All. Completed.");
    return req.CreateResponse(HttpStatusCode.OK, todoList);
}

public class TodoItem : TableEntity
{
    public string id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    
    public DateTime? due { get; set; }
    public bool isComplete { get; set; }
}