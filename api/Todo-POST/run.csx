#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

private static TelemetryClient telemetry = new TelemetryClient();
private static string key = TelemetryConfiguration.Active.InstrumentationKey = System.Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, ICollector<TodoItem> outTable, TraceWriter log)
{
    log.Info("todo-serverless (API) :: Todo-POST. Started.");

    dynamic data = await req.Content.ReadAsAsync<object>();
    string title = data?.title;
    bool isComplete = data?.isComplete;
    string dateString = data?.due;
    DateTime? due;

    if (string.IsNullOrEmpty(title))
    {
        log.Warning("todo-serverless-api :: Todo-POST. No todo item name provided.");
        return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a title in the request body");
    }
    if (string.IsNullOrEmpty(dateString))
    {
        due = DateTime.Now;
    }
    else
    {
        log.Info($"todo-serverless (API) :: Todo-POST. Parsing datetime string...{dateString}.");
        due = Convert.ToDateTime(dateString);
    }
    
    log.Info($"todo-serverless-api :: Todo-POST. About to insert the todo with Name: {title}...");
    var newRowKey = Guid.NewGuid().ToString();

    var startTime = DateTime.UtcNow;
    var timer = System.Diagnostics.Stopwatch.StartNew();

    outTable.Add(new TodoItem()
    {
        PartitionKey = "todo-serverless",
        RowKey = newRowKey,
        title = title,
        isComplete = isComplete,
        description = data?.description,
        due = due
    });
    telemetry.TrackDependency("StorageTransaction-Add", "Context", startTime, timer.Elapsed, true);
    log.Info($"todo-serverless-api :: Todo-POST. Insert todo item. RowKey: {newRowKey}.");

    log.Info("todo-serverless-api :: Todo-POST. Completed.");
    return req.CreateResponse(HttpStatusCode.Created);
}

public class TodoItem : TableEntity
{
    public string id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    
    public DateTime? due { get; set; }
    public bool isComplete { get; set; }
}
