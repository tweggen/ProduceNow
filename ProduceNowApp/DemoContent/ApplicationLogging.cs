using Microsoft.Extensions.Logging;

namespace DemoContent;

public static class ApplicationLogging
{
    private static ILoggerFactory _Factory = null;
    public static ILoggerFactory UseFactory = null;
    
    public static void ConfigureLogger(ILoggerFactory factory)
    {
    }

    public static ILoggerFactory LoggerFactory
    {
        get
        {
            if (_Factory == null)
            {
                if (null != UseFactory)
                {
                    _Factory = UseFactory;
                }
                else
                {
                    _Factory = new LoggerFactory();
                }
                ConfigureLogger(_Factory);
            }
            return _Factory;
        }
        set { _Factory = value; }
    }
}    