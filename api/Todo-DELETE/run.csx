#r "Microsoft.WindowsAzure.Storage"

using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

private static TelemetryClient telemetry = new TelemetryClient();
private static string key = TelemetryConfiguration.Active.InstrumentationKey = System.Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, CloudTable outTable, string id, TraceWriter log)
{
    log.Info("todo-serverless-api :: Todo-DELETE. Started.");
    log.Info($"todo-serverless-api :: Todo-DELETE. About to delete the todo with id: {id}...");   

    var item = new TableEntity("todo-serverless", id)
    {
        ETag = "*"
    };

    var operation = TableOperation.Delete(item);
    var startTime = DateTime.UtcNow;
    var timer = System.Diagnostics.Stopwatch.StartNew(); 
    try
    {
        await outTable.ExecuteAsync(operation);
        telemetry.TrackDependency("StorageTransaction-Delete", "Context", startTime, timer.Elapsed, true);
        log.Info("todo-serverless-api :: Todo-DELETE. Deleted todo item.");
    }
    catch (Exception ex)
    {
        telemetry.TrackDependency("StorageTransaction-Delete", "Context", startTime, timer.Elapsed, true);
        log.Error($"todo-serverless-api :: Todo-DELETE. Error: {ex.ToString()}");
        return req.CreateResponse(HttpStatusCode.NotFound);
    }
   
    log.Info("todo-serverless-api :: Todo-DELETE. Complete.");
    return req.CreateResponse(HttpStatusCode.NoContent);
}