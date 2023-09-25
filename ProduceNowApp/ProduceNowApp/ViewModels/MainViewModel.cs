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
                    Database.Instance.Add(modelChannelPresentation);
                }

                Content = VMRecChannels;
            });

        Content = vm;
    }
}
