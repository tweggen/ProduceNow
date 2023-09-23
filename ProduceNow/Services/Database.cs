using System.Collections.Generic;
using ProduceNow.Models;

namespace ProduceNow.Services;

public class Database
{
    public IEnumerable<ChannelPresentation> GetItems() => new[]
    {
        new ChannelPresentation { ShortTitle = "Studio A", IsRecording = false },
        new ChannelPresentation { ShortTitle = "Studio B", IsRecording = false },
    };
}

