using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using ProduceNowApp.Models;
using ProduceNowApp.Services;

namespace ProduceNowApp.ViewModels;

public class ChannelPresentationsViewModel : ViewModelBase
{
    public ChannelPresentationsViewModel()
    {
        Items = new();
        foreach (var item in Database.Instance.ClientConfig.GetItems())
        {
            Items.Add(new ChannelPresentationViewModel(item));
        }
    }

    public ObservableCollection<ChannelPresentationViewModel> Items { get; }
}