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
        string[] pathes = {
            "/lib/x86_64-linux-gnu/",
            "/usr/lib/x86_64-linux-gnu/",
            "../../../../FFmpeg/ffmpeg-win/"
        };
        List<string> errors = new();
        bool haveIt = false;

        foreach (string path in pathes)
        {
            if (!Directory.Exists(path))
            {
                _logger.LogInformation($"Skipping path {path}, does not exist.");
                continue;
            }

            string pathLinux = Path.Combine(path, "libavcodec.so.58");
            string pathWindows = Path.Combine(path, "avcodec-58.dll");
            if (!Path.Exists(pathLinux) && !Path.Exists(pathWindows))
            {
                _logger.LogInformation($"Skipping path {path}, found neither {pathLinux} nor {pathWindows}.");
                continue;
            }
            try
            {
                _logger.LogInformation($"Trying to load ffmpeg from {path}...");
                
                FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_TRACE, path, _logger);
                _logger.LogInformation("...success.");
                haveIt = true;
                break;
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to find ffmpeg from {path}: {e}");
            }
        }

        if (!haveIt)
        {
            throw new InvalidOperationException("No ffmpeg found.");
        }
    }
}