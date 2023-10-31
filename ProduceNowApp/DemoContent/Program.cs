using DemoContent;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        var app = builder.Build();
        ApplicationLogging.UseFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var demoContent = new DemoContent.Main();
        demoContent.Start();
        app.Run();
    }
}
