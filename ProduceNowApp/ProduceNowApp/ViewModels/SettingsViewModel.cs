using ReactiveUI;
using System.Reactive;
using ProduceNowApp.Models;
using ProduceNowApp.Services;

namespace ProduceNowApp.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private string _configUrl = "";
    public string ConfigUrl
    {
        get => _configUrl;
        set => this.RaiseAndSetIfChanged(ref _configUrl, value);
    }

    private string _rabbitMqServer = "";
    public string RabbitMqServer
    {
        get => _rabbitMqServer;
        set => this.RaiseAndSetIfChanged(ref _rabbitMqServer, value);
    }

    private string _mqttServer = "";
    public string MqttServer
    {
        get => _mqttServer;
        set => this.RaiseAndSetIfChanged(ref _mqttServer, value);
    }
    
    private string _debugSettings = "";
    public string DebugSettings
    {
        get => _debugSettings;
        set => this.RaiseAndSetIfChanged(ref _debugSettings, value);
    }
    
    public ReactiveCommand<Unit, Models.Settings> Ok { get; }
    public ReactiveCommand<Unit, Unit> Cancel { get; }
    
    public SettingsViewModel()
    {
        ConfigUrl = Database.Instance.ClientConfig.Settings.ConfigUrl;
        RabbitMqServer = Database.Instance.ClientConfig.Settings.RabbitMqServer;
        MqttServer = Database.Instance.ClientConfig.Settings.MqttServer;
        DebugSettings = Database.Instance.ClientConfig.Settings.DebugSettings;

        var okEnabled = this.WhenAnyValue(
            x => x.ConfigUrl,
            x => !string.IsNullOrWhiteSpace(x));

        Ok = ReactiveCommand.Create(
            () =>
            {
                var settings = new Settings()
                {
                    ConfigUrl = ConfigUrl,
                    RabbitMqServer = RabbitMqServer,
                    MqttServer = MqttServer,
                    DebugSettings = DebugSettings
                };
                return settings;
            },
            
            okEnabled);
        Cancel = ReactiveCommand.Create(() => { });
    }

    
}