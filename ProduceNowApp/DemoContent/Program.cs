using System.Net;
using DemoContent;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Serilog;
using Serilog.Extensions.Logging;
using SIPSorcery.Net;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        var app = builder.Build();        
        var demoContent = new DemoContent.Main();
        demoContent.Start();
        app.Run();
    }
}
