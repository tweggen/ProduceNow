using SIPSorceryMedia.Abstractions;

namespace ProduceNowApp.Services;

public interface IVideoEncoderFactory
{
    public IVideoEncoder CreateVideoEncoder();
}