using System;
using System.Reactive.Linq;
using ProduceNowApp.Models;
using ProduceNowApp.Services;
using ReactiveUI;
using Splat;

namespace ProduceNowApp.ViewModels;

public class MainViewModel : ViewModelBase
{
    private Database _database = Locator.Current.GetService<Database>();

    ViewModelBase content;

    public MainViewModel()
    {
        Content = VMRecChannels = new RecChannelsViewModel();
    }

   
    public ViewModelBase Content
    {
        get => content;
        private set => this.RaiseAndSetIfChanged(ref content, value);
    }


    public RecChannelsViewModel VMRecChannels { get; }

    public void AddChannel()
    {
        var vm = new AddChannelViewModel();

        Observable.Merge(
                vm.Ok,
                vm.Cancel.Select(_ => (ChannelPresentation)null))
            .Take(1)
            .Subscribe(modelChannelPresentation =>
            {
                if (modelChannelPresentation != null)
                {
                    _database.ClientConfig.Add(modelChannelPresentation);
                    _database.SaveClientConfig();
                }

                Content = VMRecChannels;
            });

        Content = vm;
    }


    public void OpenSettings()
    {
        var vm = new SettingsViewModel();

        Observable.Merge(
                vm.Ok,
                vm.Cancel.Select(_ => (Models.Settings)null))
            .Take(1)
            .Subscribe(modelSettings =>
            {
                if (modelSettings != null)
                {
                    _database.ClientConfig.SetSettings(modelSettings);
                    _database.SaveClientConfig();
                }

                Content = VMRecChannels;
            });

        Content = vm;
    }
}
