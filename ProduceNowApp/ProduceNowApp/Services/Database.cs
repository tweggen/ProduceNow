using System;
using System.Collections.Generic;
using System.IO;
using ProduceNowApp.Models;
using LiteDB;


namespace ProduceNowApp.Services;

public class Database
{
    private object _lo = new();
    private BsonMapper _mappers;
    private LiteDatabase _db;

    private ClientConfig _clientConfig;
    public ClientConfig ClientConfig
    {
        get => _clientConfig;
        set => _clientConfig = value;
    }
    
    private BsonMapper _createMappers()
    {
        BsonMapper m = new();
#if false
        m.RegisterType(
            vector => new BsonArray(new BsonValue[] { vector.X, vector.Y, vector.Z }),
            value => new Vector3(
                (float)value.AsArray[0].AsDouble,
                (float)value.AsArray[1].AsDouble,
                (float)value.AsArray[2].AsDouble)
        );
        m.RegisterType(
            quat => new BsonArray(new BsonValue[] { quat.X, quat.Y, quat.Z, quat.W }),
            value => new Quaternion(
                (float)value.AsArray[0].AsDouble,
                (float)value.AsArray[1].AsDouble,
                (float)value.AsArray[2].AsDouble,
                (float)value.AsArray[3].AsDouble)
        );
#endif
        return m;
    }


    public void _close()
    {
        if (null != _db)
        {
            _db.Commit();
            _db.Dispose();
            _db = null;
        }
    }
    
    
    private void _open()
    {
        string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
        string dbname = "ProduceNowApp.db";

        _db = new LiteDatabase(Path.Combine(path, dbname), _mappers);
    }


    private void _writeSettings<ClientConfig>(ClientConfig clientConfig) where ClientConfig : class
    {
        if (clientConfig == null)
        {
            throw new ArgumentNullException("ClientConfig is null.");
        }
        var col = _db.GetCollection<ClientConfig>();
        col.Upsert(clientConfig);
        _db.Commit();
    }

    
    private bool _readSettings<ClientConfig>(out ClientConfig clientConfig) where ClientConfig : class
    {
        bool haveIt = false;
        clientConfig = null;
        try
        {
            var col = _db.GetCollection<ClientConfig>();
            Console.WriteLine($"Collection has {col.Count()}: {col}");
            var allGameStates = col.FindAll();
            ClientConfig? foundGameState = col.FindById(1);
            if (foundGameState != null)
            {
                clientConfig = foundGameState;
                haveIt = true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unable to load previous clientConfig: {e}");
        }

        return haveIt;
    }
    

    public void LoadClientConfig()
    {
        ClientConfig clientConfig = null;
        lock (_lo)
        {
            try
            {
                _open();
                try
                {
                    _readSettings(out clientConfig);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unable to read clientConfig: {e}");
                }
                _close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to opem/close clientConfig: {e}");
            }
        }

        if (null == clientConfig)
        {
            clientConfig = new();
        }

        lock (_lo)
        {
            _clientConfig = clientConfig;
        }
    }
    
    
    public void SaveClientConfig()
    {
        lock (_lo)
        {
            try
            {
                _open();
                try
                {
                    _writeSettings(_clientConfig);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unable to write clientConfig: {e}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to open/close clientConfig: {e}");
            }
        }
    }

    
    public Database()
    {
        _mappers = _createMappers();
        ClientConfig = new();
        LoadClientConfig();
    }
}

