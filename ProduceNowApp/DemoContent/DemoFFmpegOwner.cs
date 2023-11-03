using SIPSorceryMedia.FFmpeg;

namespace DemoContent;

public class DemoFFmpegOwner
{
    static private object _lo = new();
    static private DemoFFmpegOwner? _instance = null;
    static private bool _tried = false;
    static public DemoFFmpegOwner Instance
    {
        get
        {
            lock (_lo)
            {
                if (!_tried)
                {
                    try
                    {
                        _instance = new DemoFFmpegOwner();
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

    private DemoFFmpegOwner()
    {
        FFmpegInit.Initialise(FfmpegLogLevelEnum.AV_LOG_INFO, "../../../ffmpeg-win/");
    }
}