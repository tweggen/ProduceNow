
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using ReactiveUI;
using ProduceNow.Models;
using ProduceNow.Services;

namespace ProduceNow.ViewModels;


class MainWindowViewModel : ViewModelBase
{
   ViewModelBase content;

    public MainWindowViewModel()
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
