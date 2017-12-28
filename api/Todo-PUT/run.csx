#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

private static TelemetryClient telemetry = new TelemetryClient();
private static string key = TelemetryConfiguration.Active.InstrumentationKey = System.Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);

public static HttpResponseMessage Run(TodoItem todo, CloudTable outTable, string id, TraceWriter log)
{
    log.Info("todo-serverless-api :: Todo-PUT. Started.");

    if (string.IsNullOrEmpty(todo.title))
    {
        return new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("A non-empty Title must be specified.")
        };
    };
    
    if (string.IsNullOrEmpty(id))
    {
        return new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("A non-empty id must be specified.")
        };
    }
    else if (id != todo.id)
    {
        log.Warning($"todo-serverless-api :: Todo-PUT. Ids mismatch: {id} from URL and {todo.RowKey} from request body.");
        return new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("ID mismatch.  URL id should match Id of todo in Body.")
        };
    };

    log.Info($"todo-serverless-api :: Todo-PUT. About to update Todo with title = {todo.title}...");
    var td = new TodoItem() { PartitionKey = "todo-serverless", RowKey = id, title = todo.title, description = todo.description, isComplete = todo.isComplete, due = todo.due };
    TableOperation updateOperation = TableOperation.InsertOrReplace(td);
    
    var startTime = DateTime.UtcNow;
    var timer = System.Diagnostics.Stopwatch.StartNew();
    TableResult result = outTable.Execute(updateOperation);
    telemetry.TrackDependency("StorageTransaction-Update", "Context", startTime, timer.Elapsed, true);
    
    log.Info("todo-serverless-api :: Todo-PUT. Complete.");

    return new HttpResponseMessage((HttpStatusCode)result.HttpStatusCode);
}

public class TodoItem : TableEntity
{
    public string id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    
    public DateTime? due { get; set; }
    public bool isComplete { get; set; }
}