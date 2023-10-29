using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Channels;
using ProduceNowApp.Models;
using ProduceNowApp.Services;
using Splat;

namespace ProduceNowApp.ViewModels;

public class ChannelPresentationsViewModel : ViewModelBase
{
    private Database _database = Locator.Current.GetService<Database>();
    public ChannelPresentationsViewModel()
    {
        
        Items = new();
        foreach (var item in _database.ClientConfig.ListChannels)
        {
            Items.Add(new ChannelPresentationViewModel(item));
        }
    }

 

    public void RemoveChannel(string channelUuid)
    {
        _database.ClientConfig.Remove(channelUuid);
        _database.SaveClientConfig();
        var ch = Items.Where(x => x.ChannelUuid == channelUuid).FirstOrDefault();
        if (ch != null)
        {
            Items.Remove(ch);
        }
    }
    
    public ObservableCollection<ChannelPresentationViewModel> Items { get; }
}