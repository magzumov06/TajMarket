using Serilog;
using WebApp.CustomSink;
using Serilog.Configuration;

namespace WebApp.ExtensionMethods;

public static class ServiceFileSinkExtensions
{
    public static LoggerConfiguration WriteToServiceFiles(
        this LoggerConfiguration configuration,
        string folder)
    {
        configuration.WriteTo.Sink(new ServiceFileSink(folder));

        return configuration;
    }
}