using System.Collections.Generic;
using ProduceNowApp.Models;

namespace ProduceNowApp.Services;

public sealed class ClientConfig
{
    [LiteDB.BsonId]
    public int Id { get; set; } = 1;

    private List<ChannelPresentation> _listChannels = new();

    
    public List<ChannelPresentation> ListChannels
    {
        get => _listChannels;
        set => _listChannels = value;
    }
    
    
    private Models.Settings _modelSettings = new();
    public Settings Settings { 
        get => _modelSettings;
        set => _modelSettings = value; 
    }


    public string PrivateKeyString { get; set; } = "";
    public string CertificateString { get; set; } = "";
    

    public void Add(ChannelPresentation modelChannelPresentation)
    {
        _listChannels.Add(modelChannelPresentation);        
    }


    public void Remove(string channelUuid)
    {
        int index = _listChannels.FindIndex(c => c.Uuid==channelUuid);
        if (index >= 0)
        {
            _listChannels.RemoveAt(index);
        }
    }
    
    
    public void SetSettings(Models.Settings modelSettings)
    {
        _modelSettings = modelSettings;
    }


    public ClientConfig(ClientConfig clientConfig)
    {
        if (null != clientConfig._listChannels)
        {
            _listChannels = new(clientConfig._listChannels);
        }
        else
        {
            _listChannels = null;
        }

        if (null != clientConfig._modelSettings)
        {
            _modelSettings = new(clientConfig._modelSettings);
        }
        else
        {
            _modelSettings = null;
        }
    }

    public ClientConfig()
    {
    }
}