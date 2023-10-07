using DemoSignalServer.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Extensions.Logging;

static Microsoft.Extensions.Logging.ILogger AddConsoleLogger()
{
    var serilogLogger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
        .WriteTo.Console()
        .CreateLogger();
    var factory = new SerilogLoggerFactory(serilogLogger);
    SIPSorcery.LogFactory.Set(factory);
    return factory.CreateLogger("DemoSignalServer");
}

AddConsoleLogger();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<RTCSignalContext>(opt =>
    opt.UseInMemoryDatabase("TodoList"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();