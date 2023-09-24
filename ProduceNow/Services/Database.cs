using System;
using System.Collections.Generic;
using ProduceNow.Models;

namespace ProduceNow.Services;

public class Database
{
    private List<ChannelPresentation> _listChannels = new()
    {
        new ChannelPresentation { ShortTitle = "Studio A", IsRecording = true, StateString = "recording" },
        new ChannelPresentation { ShortTitle = "Studio B", IsRecording = false, StateString = "monitoring" },
    };
    
    
    public IEnumerable<ChannelPresentation> GetItems() => _listChannels;


    public void Add(ChannelPresentation modelChannelPresentation)
    {
        _listChannels.Add(modelChannelPresentation);        
    }
    
    
    private static readonly Lazy<Database> lazy =
        new Lazy<Database>(() => new Database());

    
    public static Database Instance { get { return lazy.Value; } }
}

