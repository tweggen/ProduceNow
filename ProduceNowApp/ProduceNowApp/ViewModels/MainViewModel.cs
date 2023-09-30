using System;
using System.Reactive.Linq;
using ProduceNowApp.Models;
using ProduceNowApp.Services;
using ReactiveUI;

namespace ProduceNowApp.ViewModels;

public class MainViewModel : ViewModelBase
{
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
                    Database.Instance.ClientConfig.Add(modelChannelPresentation);
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
                    Database.Instance.ClientConfig.SetSettings(modelSettings);
                    Database.Instance.SaveClientConfig();
                }

                Content = VMRecChannels;
            });

        Content = vm;
    }
}
