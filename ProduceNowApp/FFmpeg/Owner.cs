using SIPSorceryMedia.FFmpeg;


namespace ProduceNow.FFmpeg;


public class Owner
{
    static private object _lo = new();
    static private Owner? _instance = null;
    static private bool _tried = false;
    static public Owner Instance
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

    private Owner()
    {
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
                Console.WriteLine($"Unable to open ffmpeg: {e}");
            }
        }
        if (!haveIt)
        {
            try
            {
                FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_TRACE, "../../../ffmpeg-win/");
                haveIt = true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to open ffmpeg: {e}");
            }
        }

        if (!haveIt)
        {
            throw new InvalidOperationException("No ffmpeg found.");
        }
    }
}