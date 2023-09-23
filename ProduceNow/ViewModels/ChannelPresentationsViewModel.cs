using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using ProduceNow.Models;

namespace ProduceNow.ViewModels;

public class ChannelPresentationsViewModel : ViewModelBase
{
    public ChannelPresentationsViewModel(IEnumerable<ChannelPresentation> items)
    {
        Items = new ObservableCollection<ChannelPresentation>(items);
    }

    public ObservableCollection<ChannelPresentation> Items { get; }
}