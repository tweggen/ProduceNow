
using System;
using System.Reactive.Linq;
using ReactiveUI;
using ProduceNow.Models;
using ProduceNow.Services;

namespace ProduceNow.ViewModels;


class MainWindowViewModel : ViewModelBase
{
    ViewModelBase content;

    public MainWindowViewModel(Database db)
    {
        Content = List = new ChannelPresentationsViewModel(db.GetItems());
    }

    public ViewModelBase Content
    {
        get => content;
        private set => this.RaiseAndSetIfChanged(ref content, value);
    }

    public ChannelPresentationsViewModel List { get; }

    public void AddChannel()
    {
        var vm = new AddChennelViewModel();

        Observable.Merge(
                vm.Ok,
                vm.Cancel.Select(_ => (ChannelPresentation)null))
            .Take(1)
            .Subscribe(model =>
            {
                if (model != null)
                {
                    List.Items.Add(model);
                }

                Content = List;
            });

        Content = vm;
    }

}
