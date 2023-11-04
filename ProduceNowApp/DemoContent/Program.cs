using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ProduceNow.DemoContent;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        var app = builder.Build();
        Common.ApplicationLogging.UseFactory = app.Services.GetRequiredService<ILoggerFactory>();

        FFmpeg.Owner? ffmpegOwner = FFmpeg.Owner.Instance;
        var demoContent = new DemoContent.Main();
        demoContent.Start();
        app.Run();
    }
}
