namespace ProduceNowApp.Models;

public class Settings
{
    public string ConfigUrl { get; set; } = "";
    public string RabbitMqServer { get; set; } = "";
    public string MqttServer { get; set; } = "";

    public Settings(Settings o)
    {
        ConfigUrl = o.ConfigUrl;
        RabbitMqServer = o.RabbitMqServer;
        MqttServer = o.MqttServer;
    }

    public Settings()
    {
    }
}