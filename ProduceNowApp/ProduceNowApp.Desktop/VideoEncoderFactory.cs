using ProduceNowApp.Services;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Encoders;

namespace ProduceNowApp.Desktop;

public class VideoEncoderFactory : IVideoEncoderFactory
{
    public IVideoEncoder CreateVideoEncoder()
    {
        return new VpxVideoEncoder();
    }
}