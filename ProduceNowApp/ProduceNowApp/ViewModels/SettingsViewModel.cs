using ReactiveUI;
using System.Reactive;
using ProduceNowApp.Models;
using ProduceNowApp.Services;
using Splat;

namespace ProduceNowApp.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private Database _database = Locator.Current.GetService<Database>();
    
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
        ConfigUrl = _database.ClientConfig.Settings.ConfigUrl;
        RabbitMqServer = _database.ClientConfig.Settings.RabbitMqServer;
        MqttServer = _database.ClientConfig.Settings.MqttServer;
        DebugSettings = _database.ClientConfig.Settings.DebugSettings;

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