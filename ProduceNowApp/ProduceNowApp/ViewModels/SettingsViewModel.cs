using ReactiveUI;
using System.Reactive;
using ProduceNowApp.Models;

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
    
    public ReactiveCommand<Unit, Models.Settings> Ok { get; }
    public ReactiveCommand<Unit, Unit> Cancel { get; }
    
    public SettingsViewModel()
    {
        var okEnabled = this.WhenAnyValue(
            x => x.ConfigUrl,
            x => !string.IsNullOrWhiteSpace(x));

        Ok = ReactiveCommand.Create(
            () => new Settings()
            {
                ConfigUrl = ConfigUrl,
                RabbitMqServer = RabbitMqServer,
                MqttServer = MqttServer
            },
            okEnabled);
        Cancel = ReactiveCommand.Create(() => { });
    }

    
}