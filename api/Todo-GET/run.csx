#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

private static TelemetryClient telemetry = new TelemetryClient();
private static string key = TelemetryConfiguration.Active.InstrumentationKey = System.Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);

public static HttpResponseMessage Run(HttpRequestMessage req, IQueryable<TodoItem> inTable, string id, TraceWriter log)
{
    log.Info("todo-serverless-api :: Todo-GET. Started...");
    var startTime = DateTime.UtcNow;
    var timer = System.Diagnostics.Stopwatch.StartNew();
    log.Info($"todo-serverless-api :: Todo-GET. About to get the todo with Id: {id}.");

    var specificEntity =
            (from e in inTable
             where e.PartitionKey == "todo-serverless" && e.RowKey == id
             select new { id = e.RowKey, e.title, e.description, e.due, e.isComplete }).FirstOrDefault();

    
    telemetry.TrackDependency("StorageTransaction-Query", "Context", startTime, timer.Elapsed, true);
    log.Info("todo-serverless-api :: Todo-GET. Retrieved todo item.");

    log.Info("todo-serverless-api :: Todo-GET. Completed.");
    return req.CreateResponse(HttpStatusCode.OK, specificEntity);
}

public class TodoItem : TableEntity
{
    public string id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    
    public DateTime? due { get; set; }
    public bool isComplete { get; set; }
}
