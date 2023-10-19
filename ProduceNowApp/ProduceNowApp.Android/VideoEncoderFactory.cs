using ProduceNowApp.Services;
using SIPSorceryMedia.Abstractions;

namespace ProduceNowApp.Android;

public class VideoEncoderFactory : IVideoEncoderFactory
{
    public IVideoEncoder CreateVideoEncoder()
    {
        return new VideoEncoder();
    }
}