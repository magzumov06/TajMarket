using Serilog.Core;
using Serilog.Events;

namespace WebApp.CustomSink;

public class ServiceFileSink(string folder) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var serviceName = "General";

        if (logEvent.Properties.ContainsKey("SourceContext"))
        {
            serviceName = logEvent.Properties["SourceContext"]
                .ToString()
                .Split('.')
                .Last()
                .Replace("\"", "");
        }

        var directory = Path.Combine(folder, serviceName);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);


        var filePath = Path.Combine(
            directory,
            $"log-{DateTime.Now:yyyy-MM-dd}.txt");


        var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} " +
                      $"{logEvent.Level} " +
                      $"{logEvent.RenderMessage()}" +
                      Environment.NewLine;


        File.AppendAllText(filePath, message);
    }
}