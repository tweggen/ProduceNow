using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using ProduceNow.Models;
using ProduceNow.Services;

namespace ProduceNow.ViewModels;

public class ChannelPresentationsViewModel : ViewModelBase
{
    public ChannelPresentationsViewModel()
    {
        Items = new();
        foreach (var item in Database.Instance.GetItems())
        {
            Items.Add(new ChannelPresentationViewModel(item));
        }
    }

    public ObservableCollection<ChannelPresentationViewModel> Items { get; }
}