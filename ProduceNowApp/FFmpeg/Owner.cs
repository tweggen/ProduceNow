using Microsoft.Extensions.Logging;
using SIPSorceryMedia.FFmpeg;


namespace ProduceNow.FFmpeg;


public class Owner
{
    static private object _lo = new();
    static private Owner? _instance = null;
    static private bool _tried = false;
    static public Owner? Instance
    {
        get
        {
            lock (_lo)
            {
                if (!_tried)
                {
                    try
                    {
                        _instance = new Owner();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Unable to instantiate ffmpeg: {e}.");
                    }

                    _tried = true;
                }

                return _instance;
            }
        }
    }

    private ILogger _logger = Common.ApplicationLogging.LoggerFactory.CreateLogger<Owner>();

    private Owner()
    {
        string error1 = "", error2 = "";
        bool haveIt = false;
        if (!haveIt)
        {
            try
            {
                FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_TRACE, "/lib/x86_64-linux-gnu/");
                haveIt = true;
            }
            catch (Exception e)
            {
                error1 = e.ToString();
            }
        }
        if (!haveIt)
        {
            try
            {
                FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_TRACE, "../../../../FFmpeg/ffmpeg-win/");
                haveIt = true;
            }
            catch (Exception e)
            {
                error2 = e.ToString();
            }
        }

        if (!haveIt)
        {
            _logger.LogError($"Unable to find ffmpeg linux libraries: {error1} and unable to find windows libraries {error2}");
            throw new InvalidOperationException("No ffmpeg found.");
        }
    }
}