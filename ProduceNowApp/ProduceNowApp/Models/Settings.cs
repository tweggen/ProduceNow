namespace ProduceNowApp.Models;

public class Settings
{
    public string ConfigUrl { get; set; } = "http://192.168.178.21:5245";
    public string RabbitMqServer { get; set; } = "";
    public string MqttServer { get; set; } = "";
    public string DebugSettings { get; set; } = "";

    public Settings(Settings o)
    {
        ConfigUrl = o.ConfigUrl;
        RabbitMqServer = o.RabbitMqServer;
        MqttServer = o.MqttServer;
        DebugSettings = o.DebugSettings;
    }

    public Settings()
    {
    }
}