namespace ProduceNowApp.Models;

public class Settings
{
    public string ConfigUrl { get; set; } = "";
    public string RabbitMqServer { get; set; } = "";
    public string MqttServer { get; set; } = "";
}